using doom_clone.GameObjects;
using doom_clone.Rendering.Model;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace doom_clone.UI
{
    public abstract class UIElement : GameObject
    {
        public UIElement(Vector3 position, Vector3 rotation, Vector3 scale) : base(position, rotation, scale, null)
        {
            Name = "UIElement";
        }

    }
}
