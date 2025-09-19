using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Transports;

namespace Nexus.Portal.Components.Pages.Production
{
    public partial class Cassettes
    {
        private bool _isLoading = true;
        private string _searchTerm = string.Empty;
        private List<CassetteViewModel> _cassettes = new List<CassetteViewModel>();
        private Dictionary<string, CassetteViewModel> _cassetteLookup = new Dictionary<string, CassetteViewModel>();
        private Dictionary<string, string> _cassetteLocationMap = new Dictionary<string, string>();

        protected override async Task OnInitializedAsync()
        {
            await LoadCassetteDataAsync();
        }

        private async Task LoadCassetteDataAsync()
        {
            _isLoading = true;
            StateHasChanged();

            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                Task<IReadOnlyList<Location>> cassetteLocationTask = LocationRepository.GetLocationsByTypeAsync(ELocationType.Cassette);
                Task<IReadOnlyList<Cassette>> cassetteTask = TransportRepository.GetCassettesWithoutTraysAsync();

                await Task.WhenAll(cassetteLocationTask, cassetteTask);

                IReadOnlyList<Location> cassetteLocations = await cassetteLocationTask;
                IReadOnlyList<Cassette> cassettes = await cassetteTask;

                _cassetteLocationMap = BuildLocationMap(cassetteLocations);

                _cassettes.Clear();
                _cassetteLookup.Clear();

                foreach (Cassette cassette in cassettes.OrderBy(item => item.Id))
                {
                    string locationId = ResolveLocationId(_cassetteLocationMap, cassette.Id);
                    CassetteViewModel viewModel = new CassetteViewModel(cassette, locationId);
                    _cassettes.Add(viewModel);
                    _cassetteLookup[cassette.Id] = viewModel;
                }

                stopwatch.Stop();
                Logger.LogInformation("Cassettes page data loaded in {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to load cassette data.");
            }
            finally
            {
                _isLoading = false;
                StateHasChanged();
            }
        }

        private static Dictionary<string, string> BuildLocationMap(IReadOnlyList<Location> locations)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (Location location in locations)
            {
                if (string.IsNullOrEmpty(location.CurrentItemId))
                {
                    continue;
                }

                if (!map.ContainsKey(location.CurrentItemId))
                {
                    map[location.CurrentItemId] = location.Id;
                }
            }

            return map;
        }

        private static string ResolveLocationId(Dictionary<string, string> map, string transportId)
        {
            if (map.TryGetValue(transportId, out string? locationId))
            {
                return locationId;
            }

            return string.Empty;
        }

        private IEnumerable<CassetteViewModel> GetFilteredCassettes()
        {
            if (string.IsNullOrWhiteSpace(_searchTerm))
            {
                return _cassettes;
            }

            string search = _searchTerm.Trim();
            return _cassettes.Where(cassette =>
                cassette.Cassette.Id.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(cassette.LocationId) && cassette.LocationId.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        private async Task ChangeLocationAsync(ELocationType locationType, string transportId, string? currentLocationId)
        {
            IReadOnlyList<Location> locations = await LocationRepository.GetLocationsByTypeAsync(locationType);
            List<string> availableLocationIds = new List<string>();
            foreach (Location location in locations)
            {
                if (string.IsNullOrEmpty(location.CurrentItemId))
                {
                    availableLocationIds.Add(location.Id);
                }
            }

            DialogParameters parameters = new DialogParameters();
            parameters.Add("Title", "위치 변경");
            parameters.Add("Locations", availableLocationIds);

            string currentLocationValue = string.Empty;
            if (!string.IsNullOrEmpty(currentLocationId))
            {
                currentLocationValue = currentLocationId;
            }

            parameters.Add("CurrentLocationId", currentLocationValue);

            DialogOptions options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            IDialogReference dialog = await DialogService.ShowAsync<Nexus.Portal.Components.Dialogs.SelectLocationDialog>("위치 변경", parameters, options);
            DialogResult? result = await dialog.Result;
            if (result == null || result.Canceled)
            {
                return;
            }

            string? newLocationId = result.Data as string;
            if (!string.IsNullOrEmpty(newLocationId))
            {
                await ApplyLocationChangeAsync(locationType, transportId, currentLocationId, newLocationId);
            }
        }

        private async Task ClearLocationAsync(ELocationType locationType, string transportId, string? currentLocationId)
        {
            if (string.IsNullOrEmpty(currentLocationId))
            {
                return;
            }

            Location? location = await LocationRepository.GetByIdAsync(currentLocationId);
            if (location != null)
            {
                location.CurrentItemId = string.Empty;
                await LocationRepository.UpdateAsync(location);
            }

            UpdateLocationForTransport(locationType, transportId, string.Empty);
            StateHasChanged();
        }

        private async Task ApplyLocationChangeAsync(ELocationType locationType, string transportId, string? currentLocationId, string newLocationId)
        {
            if (!string.IsNullOrEmpty(currentLocationId))
            {
                Location? oldLocation = await LocationRepository.GetByIdAsync(currentLocationId);
                if (oldLocation != null)
                {
                    oldLocation.CurrentItemId = string.Empty;
                    await LocationRepository.UpdateAsync(oldLocation);
                }
            }

            Location? newLocation = await LocationRepository.GetByIdAsync(newLocationId);
            if (newLocation != null)
            {
                newLocation.CurrentItemId = transportId;
                await LocationRepository.UpdateAsync(newLocation);
                UpdateLocationForTransport(locationType, transportId, newLocationId);
                StateHasChanged();
            }
        }

        private void UpdateLocationForTransport(ELocationType locationType, string transportId, string locationId)
        {
            if (locationType != ELocationType.Cassette)
            {
                return;
            }

            if (string.IsNullOrEmpty(locationId))
            {
                _cassetteLocationMap.Remove(transportId);
            }
            else
            {
                _cassetteLocationMap[transportId] = locationId;
            }

            if (_cassetteLookup.TryGetValue(transportId, out CassetteViewModel? cassette))
            {
                cassette.LocationId = locationId;
            }
        }

        private sealed class CassetteViewModel
        {
            public CassetteViewModel(Cassette cassette, string locationId)
            {
                Cassette = cassette;
                LocationId = locationId;
            }

            public Cassette Cassette { get; }
            public string LocationId { get; set; }
        }
    }
}
