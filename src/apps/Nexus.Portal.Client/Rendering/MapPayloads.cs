using System;

namespace Nexus.Portal.Rendering
{
    public class SpacePayload
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string LocationType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string ParentId { get; set; } = string.Empty;

        public bool IsVisible { get; set; }

        public bool IsRelativePosition { get; set; }

        public double RotateX { get; set; }

        public double RotateY { get; set; }

        public double RotateZ { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public double Depth { get; set; }

        public string MarkerRole { get; set; } = string.Empty;

        public string CurrentItemId { get; set; } = string.Empty;
    }

    public class LocationPayload
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string LocationType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string ParentId { get; set; } = string.Empty;

        public bool IsVisible { get; set; }

        public bool IsRelativePosition { get; set; }

        public double RotateX { get; set; }

        public double RotateY { get; set; }

        public double RotateZ { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public double Depth { get; set; }

        public string MarkerRole { get; set; } = string.Empty;

        public string CurrentItemId { get; set; } = string.Empty;
    }

    public class EdgePayload
    {
        public string Id { get; set; } = string.Empty;

        public double FromX { get; set; }

        public double FromY { get; set; }

        public double FromZ { get; set; }

        public double ToX { get; set; }

        public double ToY { get; set; }

        public double ToZ { get; set; }

        public string Color { get; set; } = string.Empty;
    }

    public class RobotPayload
    {
        public string Id { get; set; } = string.Empty;

        public string RobotType { get; set; } = string.Empty;

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }
    }
}
