using Microsoft.AspNetCore.Components;

namespace Nexus.Portal.Components.Layout
{
    public class DockService
    {
        public RenderFragment? BottomContent { get; private set; }

        public event Action? Changed;

        public void Set(RenderFragment content)
        {
            BottomContent = content;
            Action? handler = Changed;
            if (handler != null)
            {
                handler.Invoke();
            }
        }

        public void Clear()
        {
            BottomContent = null;
            Action? handler = Changed;
            if (handler != null)
            {
                handler.Invoke();
            }
        }
    }
}

