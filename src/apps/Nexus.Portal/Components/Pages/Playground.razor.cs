using Microsoft.AspNetCore.Components;
using Nexus.Core.Domain.Models.Locations;
using Nexus.Core.Domain.Models.Locations.Base;
using Nexus.Core.Domain.Models.Locations.Enums;
using Nexus.Core.Domain.Models.Locations.Graphs;
using Nexus.Core.Domain.Models.Locations.Interfaces;
using Nexus.Core.Domain.Models.Locations.ValueObjects;
using Nexus.Core.Domain.Models.Robots;
using Nexus.Core.Domain.Models.Transports.Enums;
using Nexus.Core.Domain.Shared.Bases;
using Nexus.Portal.Rendering;

namespace Nexus.Portal.Components.Pages
{
    public partial class Playground
    {
        private readonly string _canvasElementId = "nexus-playground-canvas";
        private ElementReference _canvasRef;
        private bool _initialized;

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender || _initialized)
            {
                return Task.CompletedTask;
            }

            ThreeInterop.InitScene(_canvasElementId);
            RenderDemoMap();
            _initialized = true;

            return Task.CompletedTask;
        }

        private void RenderDemoMap()
        {
            ThreeInterop.ResetMap();

            List<SpaceDisplay> spaces = BuildSpaces();
            foreach (SpaceDisplay space in spaces)
            {
                SpacePayload payload = ToSpacePayload(space);
                string json = Serialize(payload);
                ThreeInterop.AddSpaceJson(json);
            }

            List<LocationDisplay> locations = BuildLocations(spaces);
            foreach (LocationDisplay location in locations)
            {
                LocationPayload payload = ToLocationPayload(location);
                string json = Serialize(payload);
                ThreeInterop.AddLocationJson(json);
            }

            Dictionary<string, LocationGeometry> geometryLookup = BuildGeometryLookup(spaces, locations);

            List<LocationEdge> edges = BuildEdges();
            foreach (LocationEdge edge in edges)
            {
                EdgePayload? payload = ToEdgePayload(edge, geometryLookup);
                if (payload != null)
                {
                    string json = Serialize(payload);
                    ThreeInterop.AddEdgeJson(json);
                }
            }

            List<Robot> robots = BuildRobots();
            foreach (Robot robot in robots)
            {
                RobotPayload payload = ToRobotPayload(robot);
                string json = Serialize(payload);
                ThreeInterop.AddRobotJson(json);
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

        private SpacePayload ToSpacePayload(SpaceDisplay display)
        {
            SpacePayload payload = new SpacePayload();
            payload.Id = display.Space.Id;
            payload.Name = display.Space.Name;
            payload.LocationType = "Space";
            payload.Status = ELocationStatus.Available.ToString();
            payload.ParentId = string.Empty;
            payload.IsVisible = true;
            payload.IsRelativePosition = false;
            payload.RotateX = 0;
            payload.RotateY = 0;
            payload.RotateZ = 0;
            payload.X = display.X;
            payload.Y = display.Y;
            payload.Z = display.Z;
            payload.Width = display.Width;
            payload.Height = display.Height;
            payload.Depth = display.Depth;
            payload.MarkerRole = display.MarkerRole;
            payload.CurrentItemId = string.Empty;
            return payload;
        }

        private LocationPayload ToLocationPayload(LocationDisplay display)
        {
            LocationPayload payload = new LocationPayload();
            payload.Id = display.Location.Id;
            payload.Name = display.Location.Name;
            payload.LocationType = display.LocationType;
            payload.Status = display.Location.Status.ToString();
            payload.ParentId = display.Location.ParentId;
            payload.IsVisible = display.Location.IsVisible;
            payload.IsRelativePosition = display.Location.IsRelativePosition;
            payload.RotateX = display.Location.Rotation.X;
            payload.RotateY = display.Location.Rotation.Y;
            payload.RotateZ = display.Location.Rotation.Z;
            payload.X = display.Location.Position.X;
            payload.Y = display.Location.Position.Y;
            payload.Z = display.Location.Position.Z;
            payload.Width = display.Location.Width;
            payload.Height = display.Location.Height;
            payload.Depth = display.Location.Depth;
            payload.MarkerRole = display.MarkerRole;
            payload.CurrentItemId = display.Location.CurrentItemId;
            return payload;
        }

        private EdgePayload? ToEdgePayload(LocationEdge edge, IReadOnlyDictionary<string, LocationGeometry> geometryLookup)
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

            EdgePayload payload = new EdgePayload();
            payload.Id = edge.FromLocationId + "->" + edge.ToLocationId;
            payload.FromX = fromX;
            payload.FromY = fromY;
            payload.FromZ = fromZ;
            payload.ToX = toX;
            payload.ToY = toY;
            payload.ToZ = toZ;
            payload.Color = edge.IsBidirectional ? "#22c55e" : "#64748b";
            return payload;
        }

        private RobotPayload ToRobotPayload(Robot robot)
        {
            RobotPayload payload = new RobotPayload();
            payload.Id = robot.Id;
            payload.RobotType = "Logistics";
            payload.X = robot.Position.X;
            payload.Y = robot.Position.Y;
            payload.Z = robot.Position.Z;
            return payload;
        }

        private string Serialize<T>(T payload)
        {
            return System.Text.Json.JsonSerializer.Serialize(payload);
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
