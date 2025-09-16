using Microsoft.AspNetCore.Components;
using MudBlazor;
using Nexus.Core.Domain.Models.Lots;
using Nexus.Core.Domain.Models.Lots.Enums;
using Nexus.Core.Domain.Models.Lots.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nexus.Portal.Components.Pages.Production
{
    public partial class Lots
    {
        [Inject]
        private ILotRepository LotRepository { get; set; } = default!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = default!;

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        private List<Lot> _lots = new List<Lot>();
        private HashSet<Lot> _selectedLots = new HashSet<Lot>();
        private HashSet<LotStep> _selectedSteps = new HashSet<LotStep>();

        private string _searchLot = string.Empty;
        private string _searchStep = string.Empty;

        // Lot editor state (panel below grid)
        private bool _showLotEditor = false;
        private bool _isCreatingLot = false;
        private LotEditModel _lotEditor = new LotEditModel();
        private MudForm? _lotForm;

        private bool _showStepEditor = false;
        private bool _isCreatingStep = false;
        private LotStepEditModel _stepEditor = new LotStepEditModel();
        private MudForm? _stepForm;

        protected override async Task OnInitializedAsync()
        {
            await LoadLotsAsync();
        }

        private async Task LoadLotsAsync()
        {
            try
            {
                IReadOnlyList<Lot> lots = await LotRepository.GetAllAsync();
                _lots = lots.ToList();

                // Select the first lot by default so its steps are visible on initial load
                if (_lots.Count > 0)
                {
                    _selectedLots = new HashSet<Lot>();
                    _selectedLots.Add(_lots[0]);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to load lots: {ex.Message}", Severity.Error);
                _lots = new List<Lot>();
            }
        }

        private Color GetLotStatusColor(ELotStatus status)
        {
            if (status == ELotStatus.Waiting)
            {
                return Color.Info;
            }
            if (status == ELotStatus.Assigned)
            {
                return Color.Secondary;
            }
            if (status == ELotStatus.Processing)
            {
                return Color.Primary;
            }
            if (status == ELotStatus.Completed)
            {
                return Color.Success;
            }
            if (status == ELotStatus.Error)
            {
                return Color.Error;
            }
            return Color.Default;
        }

        private Func<Lot, bool> QuickLotFilter
        {
            get
            {
                return lot =>
                {
                    if (string.IsNullOrWhiteSpace(_searchLot))
                    {
                        return true;
                    }
                    if (lot.Id != null && lot.Id.Contains(_searchLot, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (lot.Name != null && lot.Name.Contains(_searchLot, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (lot.PartNo != null && lot.PartNo.Contains(_searchLot, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    return false;
                };
            }
        }

        private Func<LotStep, bool> QuickStepFilter
        {
            get
            {
                return step =>
                {
                    if (string.IsNullOrWhiteSpace(_searchStep))
                    {
                        return true;
                    }
                    if (step.Id != null && step.Id.Contains(_searchStep, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (step.Name != null && step.Name.Contains(_searchStep, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (step.PGM != null && step.PGM.Contains(_searchStep, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    return false;
                };
            }
        }

        private List<LotStep> GetCurrentSteps()
        {
            if (_selectedLots != null)
            {
                if (_selectedLots.Count > 0)
                {
                    Lot lot = _selectedLots.First();
                    if (lot != null)
                    {
                        if (lot.LotSteps != null)
                        {
                            return lot.LotSteps;
                        }
                    }
                }
            }
            return new List<LotStep>();
        }

        private void OnNewLot()
        {
            _isCreatingLot = true;
            _lotEditor = LotEditModel.CreateNew();
            _lotEditor.Status = ELotStatus.None;
            _showLotEditor = true;
        }

        private void OnEditLot(Lot lot)
        {
            if (lot == null)
            {
                return;
            }
            _lotEditor = LotEditModel.FromLot(lot);
            _showLotEditor = true;
            _isCreatingLot = false;
        }

        private async Task OnSaveLot()
        {
            if (_lotForm != null)
            {
                await _lotForm.Validate();
                if (!_lotForm.IsValid)
                {
                    return;
                }
            }

            try
            {
                if (_isCreatingLot)
                {
                    Lot newLot = _lotEditor.ToLot();
                    await LotRepository.AddAsync(newLot);
                    Snackbar.Add($"Lot '{newLot.Id}' created", Severity.Success);
                }
                else
                {
                    Lot? target = _lots.FirstOrDefault(x => x.Id == _lotEditor.Id);
                    if (target == null)
                    {
                        Snackbar.Add("Selected lot not found.", Severity.Warning);
                        return;
                    }
                    _lotEditor.ApplyTo(target);
                    await LotRepository.UpdateAsync(target);
                    Snackbar.Add($"Lot '{target.Id}' updated", Severity.Success);
                }
                _showLotEditor = false;
                _isCreatingLot = false;
                await LoadLotsAsync();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to save lot: {ex.Message}", Severity.Error);
            }
        }

        private void OnCancelLot()
        {
            _showLotEditor = false;
            _isCreatingLot = false;
        }

        private async Task OnDeleteSelectedLots()
        {
            if (_selectedLots == null)
            {
                return;
            }
            if (_selectedLots.Count == 0)
            {
                return;
            }

            string message = $"Delete [{_selectedLots.First().Id}]? This action cannot be undone.";
            DialogOptions dialogOptions = new DialogOptions
            {
                CloseOnEscapeKey = true
            };
            bool? confirmed = await DialogService.ShowMessageBox("Confirm Delete", message, yesText: "Delete", cancelText: "Cancel", options: dialogOptions);
            if (!(confirmed == true))
            {
                return;
            }
            try
            {
                foreach (Lot lot in _selectedLots)
                {
                    await LotRepository.DeleteAsync(lot.Id);
                }
                Snackbar.Add($"Deleted {_selectedLots.Count} lot(s)", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to delete: {ex.Message}", Severity.Error);
            }
            finally
            {
                _selectedLots.Clear();
                await LoadLotsAsync();
            }
        }

        private async Task OnReloadLots()
        {
            await LoadLotsAsync();
        }

        private void OnNewStep()
        {
            if (_selectedLots == null)
            {
                return;
            }
            if (_selectedLots.Count == 0)
            {
                return;
            }
            _isCreatingStep = true;
            _showStepEditor = true;
            Lot lot = _selectedLots.First();
            _stepEditor = LotStepEditModel.CreateNew(lot.Id);
            _stepEditor.Status = ELotStatus.None;
        }

        private void OnEditStep(LotStep step)
        {
            if (step == null)
            {
                return;
            }
            _isCreatingStep = false;
            _showStepEditor = true;
            _stepEditor = LotStepEditModel.FromStep(step);
        }

        private async Task OnSaveStep()
        {
            if (_stepForm != null)
            {
                await _stepForm.Validate();
                if (!_stepForm.IsValid)
                {
                    return;
                }
            }

            if (_selectedLots == null)
            {
                return;
            }
            if (_selectedLots.Count == 0)
            {
                return;
            }
            Lot lot = _selectedLots.First();

            try
            {
                if (_isCreatingStep)
                {
                    LotStep newStep = _stepEditor.ToLotStep();
                    await LotRepository.AddLotStepAsync(lot.Id, newStep);
                    Snackbar.Add($"Step '{newStep.Id}' added", Severity.Success);
                }
                else
                {
                    Lot? fresh = await LotRepository.GetByIdAsync(lot.Id);
                    if (fresh == null)
                    {
                        Snackbar.Add("Lot not found.", Severity.Warning);
                        return;
                    }
                    LotStep? target = fresh.LotSteps.FirstOrDefault(s => s.Id == _stepEditor.Id);
                    if (target == null)
                    {
                        Snackbar.Add("Step not found.", Severity.Warning);
                        return;
                    }
                    _stepEditor.ApplyTo(target);
                    await LotRepository.UpdateAsync(fresh);
                    Snackbar.Add($"Step '{target.Id}' updated", Severity.Success);
                }

                await LoadLotsAsync();
                _showStepEditor = false;
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to save step: {ex.Message}", Severity.Error);
            }
        }

        private void OnCancelStep()
        {
            _showStepEditor = false;
        }

        private async Task OnDeleteSelectedSteps()
        {
            if (_selectedLots == null)
            {
                return;
            }
            if (_selectedLots.Count == 0)
            {
                return;
            }
            if (_selectedSteps == null)
            {
                return;
            }
            if (_selectedSteps.Count == 0)
            {
                return;
            }

            Lot lot = _selectedLots.First();
            string message = $"Delete [{_selectedSteps.First().Id}] from lot '{lot.Id}'? This action cannot be undone.";
            DialogOptions dialogOptions = new DialogOptions
            {
                CloseOnEscapeKey = true
            };
            bool? confirmed = await DialogService.ShowMessageBox("Confirm Delete", message, yesText: "Delete", cancelText: "Cancel", options: dialogOptions);
            if (!(confirmed == true))
            {
                return;
            }

            try
            {
                // Prefer repository-level removal to also delete underlying step records
                Nexus.Infrastructure.Persistence.Redis.RedisLotRepository? redisRepo = LotRepository as Nexus.Infrastructure.Persistence.Redis.RedisLotRepository;
                if (redisRepo != null)
                {
                    foreach (LotStep step in _selectedSteps)
                    {
                        await redisRepo.RemoveLotStepAsync(lot.Id, step.Id);
                    }
                }
                else
                {
                    // Fallback: update lot without actually deleting the step keys
                    Lot? fresh = await LotRepository.GetByIdAsync(lot.Id);
                    if (fresh == null)
                    {
                        return;
                    }
                    List<string> toDelete = _selectedSteps.Select(s => s.Id).ToList();
                    List<LotStep> remain = new List<LotStep>();
                    foreach (LotStep s in fresh.LotSteps)
                    {
                        if (!toDelete.Contains(s.Id))
                        {
                            remain.Add(s);
                        }
                    }
                    fresh.LotSteps = remain;
                    await LotRepository.UpdateAsync(fresh);
                }

                Snackbar.Add($"Deleted {_selectedSteps.Count} step(s)", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to delete steps: {ex.Message}", Severity.Error);
            }
            finally
            {
                _selectedSteps.Clear();
                await LoadLotsAsync();
            }
        }

        private sealed class LotEditModel
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public ELotStatus Status { get; set; } = ELotStatus.None;
            public int Priority { get; set; } = 0;
            public DateTime? ReceivedTime { get; set; } = DateTime.UtcNow;
            public string Purpose { get; set; } = string.Empty;
            public string EvalNo { get; set; } = string.Empty;
            public string PartNo { get; set; } = string.Empty;
            public int Qty { get; set; } = 0;
            public string Option { get; set; } = string.Empty;
            public string Line { get; set; } = string.Empty;

            public static LotEditModel CreateNew()
            {
                LotEditModel m = new LotEditModel();
                m.Id = $"LOT-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
                m.ReceivedTime = DateTime.UtcNow;
                return m;
            }

            public static LotEditModel FromLot(Lot lot)
            {
                LotEditModel m = new LotEditModel();
                m.Id = lot.Id;
                m.Name = lot.Name;
                m.Status = lot.Status;
                m.Priority = lot.Priority;
                m.ReceivedTime = lot.ReceivedTime;
                m.Purpose = lot.Purpose;
                m.EvalNo = lot.EvalNo;
                m.PartNo = lot.PartNo;
                m.Qty = lot.Qty;
                m.Option = lot.Option;
                m.Line = lot.Line;
                return m;
            }

            public Lot ToLot()
            {
                List<string> cassetteIds = new List<string>();
                DateTime rt;
                if (ReceivedTime.HasValue)
                {
                    rt = ReceivedTime.Value;
                }
                else
                {
                    rt = DateTime.UtcNow;
                }
                Lot lot = new Lot(Id, Name, Status, Priority, rt, Purpose, EvalNo, PartNo, Qty, Option, Line, cassetteIds);
                return lot;
            }

            public void ApplyTo(Lot lot)
            {
                lot.Name = Name;
                lot.Status = Status;
                lot.Priority = Priority;
                DateTime rt;
                if (ReceivedTime.HasValue)
                {
                    rt = ReceivedTime.Value;
                }
                else
                {
                    rt = lot.ReceivedTime;
                }
                lot.ReceivedTime = rt;
                lot.Purpose = Purpose;
                lot.EvalNo = EvalNo;
                lot.PartNo = PartNo;
                lot.Qty = Qty;
                lot.Option = Option;
                lot.Line = Line;
            }
        }

        private sealed class LotStepEditModel
        {
            public string Id { get; set; } = string.Empty;
            public string LotId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public int LoadingType { get; set; } = 0;
            public string DpcType { get; set; } = string.Empty;
            public string Chipset { get; set; } = string.Empty;
            public string PGM { get; set; } = string.Empty;
            public int PlanPercent { get; set; } = 100;
            public ELotStatus Status { get; set; } = ELotStatus.None;

            public static LotStepEditModel CreateNew(string lotId)
            {
                LotStepEditModel m = new LotStepEditModel();
                m.Id = $"STEP-{DateTime.UtcNow:HHmmssfff}";
                m.LotId = lotId;
                return m;
            }

            public static LotStepEditModel FromStep(LotStep step)
            {
                LotStepEditModel m = new LotStepEditModel();
                m.Id = step.Id;
                m.LotId = step.LotId;
                m.Name = step.Name;
                m.LoadingType = step.LoadingType;
                m.DpcType = step.DpcType;
                m.Chipset = step.Chipset;
                m.PGM = step.PGM;
                m.PlanPercent = step.PlanPercent;
                m.Status = step.Status;
                return m;
            }

            public LotStep ToLotStep()
            {
                LotStep s = new LotStep(Id, LotId, Name, LoadingType, DpcType, Chipset, PGM, PlanPercent, Status);
                return s;
            }

            public void ApplyTo(LotStep step)
            {
                step.Name = Name;
                step.LoadingType = LoadingType;
                step.DpcType = DpcType;
                step.Chipset = Chipset;
                step.PGM = PGM;
                step.PlanPercent = PlanPercent;
                step.Status = Status;
            }
        }
    }
}
