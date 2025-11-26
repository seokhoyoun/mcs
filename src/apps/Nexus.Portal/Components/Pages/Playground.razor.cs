using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Graphs;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Locations.ValueObjects;
using Nexus.Core.Domain.Models.Robots;
using Nexus.Core.Domain.Shared.Bases;
using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Core.Domain.Models.Locations.Base;

namespace Nexus.Portal.Components.Pages
{
    public partial class Playground
    {
        private readonly string _canvasElementId = "nexus-playground-canvas";
        private ElementReference _canvasRef;
        private bool _initialized;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender || _initialized)
            {
                return;
            }

            await InitializeSceneAsync();
            _initialized = true;
        }

        private async Task InitializeSceneAsync()
        {
            await JS.InvokeVoidAsync("nexus3d.init3D", _canvasElementId);

            List<SpaceDisplay> spaces = BuildSpaces();
            foreach (SpaceDisplay space in spaces)
            {
                object spacePayload = ToSpacePayload(space);
                await JS.InvokeVoidAsync("nexus3d.addSpace3D", spacePayload);
            }

            List<LocationDisplay> locations = BuildLocations(spaces);
            foreach (LocationDisplay location in locations)
            {
                object locationPayload = ToLocationPayload(location);
                await JS.InvokeVoidAsync("nexus3d.addLocation3D", locationPayload);
            }

            Dictionary<string, LocationGeometry> geometryLookup = BuildGeometryLookup(spaces, locations);

            List<LocationEdge> edges = BuildEdges();
            foreach (LocationEdge edge in edges)
            {
                object? edgePayload = ToEdgePayload(edge, geometryLookup);
                if (edgePayload != null)
                {
                    await JS.InvokeVoidAsync("nexus3d.addEdge3D", edgePayload);
                }
            }

            List<Robot> robots = BuildRobots();
            foreach (Robot robot in robots)
            {
                object robotPayload = ToRobotPayload(robot);
                await JS.InvokeVoidAsync("nexus3d.addRobot3D", robotPayload);
            }
        }

        private List<SpaceDisplay> BuildSpaces()
        {
            List<SpaceDisplay> layout = new List<SpaceDisplay>();

            SpaceSpecification defaultSpec = new SpaceSpecification("SPACE-STD", new List<string> { "CARRIER-STD" }, new List<ECargoKind> { ECargoKind.Container, ECargoKind.Box });

            List<CarrierLocation> floorCarriers = new List<CarrierLocation>();
            Space baseFloor = new Space("area-floor", "Base Floor", defaultSpec, floorCarriers, new List<ISpace>());
            SpaceDisplay baseFloorDisplay = new SpaceDisplay(baseFloor, "Area", 0, 0, 0, 520, 2, 520);
            layout.Add(baseFloorDisplay);

            List<CarrierLocation> workspaceACarriers = new List<CarrierLocation>();
            Space workspaceA = new Space("workspace-a", "Workspace A", defaultSpec, workspaceACarriers, new List<ISpace>());
            SpaceDisplay workspaceADisplay = new SpaceDisplay(workspaceA, "Area", 40, 40, 2, 150, 6, 200);
            layout.Add(workspaceADisplay);

            List<CarrierLocation> workspaceBCarriers = new List<CarrierLocation>();
            Space workspaceB = new Space("workspace-b", "Workspace B", defaultSpec, workspaceBCarriers, new List<ISpace>());
            SpaceDisplay workspaceBDisplay = new SpaceDisplay(workspaceB, "Area", 240, 120, 2, 150, 6, 200);
            layout.Add(workspaceBDisplay);

            CarrierLocation staging = new CarrierLocation("staging", "Staging", "SPEC-TRAY", ECargoKind.Container);
            staging.Status = ELocationStatus.Occupied;
            staging.CurrentItemId = "item-001";
            staging.Position = new Position(180, 380, 2);
            staging.Width = 90;
            staging.Height = 16;
            staging.Depth = 90;

            CarrierLocation cassette = new CarrierLocation("cassette-1", "Cassette Slot", "SPEC-CASS", ECargoKind.Container);
            cassette.Status = ELocationStatus.Available;
            cassette.Position = new Position(380, 340, 2);
            cassette.Width = 90;
            cassette.Height = 60;
            cassette.Depth = 90;

            List<CarrierLocation> bufferCarriers = new List<CarrierLocation> { staging, cassette };
            Space bufferZone = new Space("buffer-zone", "Buffer Zone", defaultSpec, bufferCarriers, new List<ISpace>());
            SpaceDisplay bufferZoneDisplay = new SpaceDisplay(bufferZone, "Area", 160, 320, 2, 220, 4, 120);
            layout.Add(bufferZoneDisplay);

            return layout;
        }

        private List<LocationDisplay> BuildLocations(List<SpaceDisplay> spaces)
        {
            List<LocationDisplay> layout = new List<LocationDisplay>();

            foreach (SpaceDisplay space in spaces)
            {
                foreach (CarrierLocation carrier in space.Space.CarrierLocations)
                {
                    string locationType = ResolveCarrierType(carrier.SpecificationCode);
                    LocationDisplay carrierDisplay = new LocationDisplay(carrier, locationType, string.Empty);
                    layout.Add(carrierDisplay);
                }
            }

            MarkerLocation laneMarker = new MarkerLocation("lane-marker", "Transfer Lane");
            laneMarker.Status = ELocationStatus.Available;
            laneMarker.Position = new Position(40, 260, 2);
            laneMarker.Width = 420;
            laneMarker.Height = 3;
            laneMarker.Depth = 40;
            LocationDisplay laneDisplay = new LocationDisplay(laneMarker, "Marker", "MoveArea");
            layout.Add(laneDisplay);

            return layout;
        }

        private List<LocationEdge> BuildEdges()
        {
            List<LocationEdge> edges = new List<LocationEdge>();

            LocationEdge lane = new LocationEdge("workspace-a", "workspace-b", 1.0, true);
            edges.Add(lane);

            LocationEdge cross = new LocationEdge("workspace-b", "buffer-zone", 1.0, true);
            edges.Add(cross);

            return edges;
        }

        private List<Robot> BuildRobots()
        {
            List<Robot> robots = new List<Robot>();

            Robot botA = new Robot("robot-a", "Robot A");
            botA.Position = new Position(120, 140, 9);
            robots.Add(botA);

            Robot botB = new Robot("robot-b", "Robot B");
            botB.Position = new Position(320, 240, 9);
            robots.Add(botB);

            return robots;
        }

        private Dictionary<string, LocationGeometry> BuildGeometryLookup(IEnumerable<SpaceDisplay> spaces, IEnumerable<LocationDisplay> locations)
        {
            Dictionary<string, LocationGeometry> lookup = new Dictionary<string, LocationGeometry>(StringComparer.OrdinalIgnoreCase);

            foreach (SpaceDisplay space in spaces)
            {
                lookup[space.Space.Id] = new LocationGeometry(space.X, space.Y, space.Z, space.Width, space.Height, space.Depth);
            }

            foreach (LocationDisplay location in locations)
            {
                lookup[location.Location.Id] = new LocationGeometry(location.Location.Position.X, location.Location.Position.Y, location.Location.Position.Z, location.Location.Width, location.Location.Height, location.Location.Depth);
            }

            return lookup;
        }

        private object ToSpacePayload(SpaceDisplay display)
        {
            return new
            {
                id = display.Space.Id,
                name = display.Space.Name,
                locationType = "Space",
                status = ELocationStatus.Available.ToString(),
                parentId = string.Empty,
                isVisible = true,
                isRelativePosition = false,
                rotateX = 0,
                rotateY = 0,
                rotateZ = 0,
                x = display.X,
                y = display.Y,
                z = display.Z,
                width = display.Width,
                height = display.Height,
                depth = display.Depth,
                markerRole = display.MarkerRole,
                currentItemId = string.Empty
            };
        }

        private object ToLocationPayload(LocationDisplay display)
        {
            string statusText = display.Location.Status.ToString();

            return new
            {
                id = display.Location.Id,
                name = display.Location.Name,
                locationType = display.LocationType,
                status = statusText,
                parentId = display.Location.ParentId,
                isVisible = display.Location.IsVisible,
                isRelativePosition = display.Location.IsRelativePosition,
                rotateX = display.Location.Rotation.X,
                rotateY = display.Location.Rotation.Y,
                rotateZ = display.Location.Rotation.Z,
                x = display.Location.Position.X,
                y = display.Location.Position.Y,
                z = display.Location.Position.Z,
                width = display.Location.Width,
                height = display.Location.Height,
                depth = display.Location.Depth,
                markerRole = display.MarkerRole,
                currentItemId = display.Location.CurrentItemId
            };
        }

        private object? ToEdgePayload(LocationEdge edge, IReadOnlyDictionary<string, LocationGeometry> geometryLookup)
        {
            if (!geometryLookup.TryGetValue(edge.FromLocationId, out LocationGeometry from))
            {
                return null;
            }

            if (!geometryLookup.TryGetValue(edge.ToLocationId, out LocationGeometry to))
            {
                return null;
            }

            double fromX = from.X + (from.Width / 2.0);
            double fromY = from.Y + (from.Depth / 2.0);
            double fromZ = from.Z + (from.Height / 2.0);

            double toX = to.X + (to.Width / 2.0);
            double toY = to.Y + (to.Depth / 2.0);
            double toZ = to.Z + (to.Height / 2.0);

            string edgeId = edge.FromLocationId + "->" + edge.ToLocationId;
            string edgeColor;
            if (edge.IsBidirectional)
            {
                edgeColor = "#22c55e";
            }
            else
            {
                edgeColor = "#64748b";
            }

            return new
            {
                id = edgeId,
                fromX,
                fromY,
                fromZ,
                toX,
                toY,
                toZ,
                color = edgeColor
            };
        }

        private object ToRobotPayload(Robot robot)
        {
            return new
            {
                id = robot.Id,
                x = robot.Position.X,
                y = robot.Position.Y,
                z = robot.Position.Z
            };
        }

        private sealed class SpaceDisplay
        {
            public SpaceDisplay(Space space, string markerRole, uint x, uint y, uint z, uint width, uint height, uint depth)
            {
                Space = space;
                MarkerRole = markerRole;
                X = x;
                Y = y;
                Z = z;
                Width = width;
                Height = height;
                Depth = depth;
            }

            public Space Space { get; }

            public string MarkerRole { get; }

            public uint X { get; }

            public uint Y { get; }

            public uint Z { get; }

            public uint Width { get; }

            public uint Height { get; }

            public uint Depth { get; }
        }

        private sealed class LocationDisplay
        {
            public LocationDisplay(Location location, string locationType, string markerRole)
            {
                Location = location;
                LocationType = locationType;
                MarkerRole = markerRole;
            }

            public Location Location { get; }

            public string LocationType { get; }

            public string MarkerRole { get; }
        }

        private sealed class LocationGeometry
        {
            public LocationGeometry(uint x, uint y, uint z, uint width, uint height, uint depth)
            {
                X = x;
                Y = y;
                Z = z;
                Width = width;
                Height = height;
                Depth = depth;
            }

            public uint X { get; }

            public uint Y { get; }

            public uint Z { get; }

            public uint Width { get; }

            public uint Height { get; }

            public uint Depth { get; }
        }

        private string ResolveCarrierType(string specificationCode)
        {
            string code = specificationCode;
            if (code == null)
            {
                code = string.Empty;
            }

            string upper = code.ToUpperInvariant();
            if (upper.Contains("CASS"))
            {
                return "Cassette";
            }

            if (upper.Contains("TRAY"))
            {
                return "Tray";
            }

            return "Carrier";
        }
    }
}
