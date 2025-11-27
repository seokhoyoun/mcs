using System;
using System.Runtime.InteropServices.JavaScript;
using MudBlazor;

namespace Nexus.Playground.Client.Rendering
{
    internal static partial class ThreeInterop
    {
        [JSImport("initScene", "__threeBridge")]
        internal static partial void InitScene(string canvasId);

        [JSImport("setBackground", "__threeBridge")]
        internal static partial void SetBackground(string hexColor);

        [JSImport("disposeScene", "__threeBridge")]
        internal static partial void DisposeScene();

        [JSImport("resetMap", "__threeBridge")]
        internal static partial void ResetMap();

        [JSImport("addSpaceFromJson", "__threeBridge")]
        internal static partial void AddSpaceJson(string payloadJson);

        [JSImport("addLocationFromJson", "__threeBridge")]
        internal static partial void AddLocationJson(string payloadJson);

        [JSImport("addEdgeFromJson", "__threeBridge")]
        internal static partial void AddEdgeJson(string payloadJson);

        [JSImport("addRobotFromJson", "__threeBridge")]
        internal static partial void AddRobotJson(string payloadJson);

        internal static string ToHexColor(Palette palette)
        {
            if (palette == null)
            {
                throw new ArgumentNullException(nameof(palette));
            }

            string value = palette.Background.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                return "#0F172A";
            }

            if (value.StartsWith("#", StringComparison.Ordinal))
            {
                return value;
            }

            return "#" + value;
        }
    }
}
