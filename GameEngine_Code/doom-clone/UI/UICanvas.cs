using OpenTK.Mathematics;
using System.Collections.Generic;

namespace doom_clone.UI
{
    public class UICanvas
    {
        private List<UIElement> elements;

        public UICanvas() {
            elements = new List<UIElement>();
        }

        public void AddElement(UIElement element)
        {
            elements.Add(element);
        }

        public void RenderUI(ref Renderer renderer)
        {
            for(int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Mesh.Material != null)
                {
                    Matrix4 transform =
                    Matrix4.CreateTranslation(new Vector3(-0.5f, -0.5f, -0.5f)) *
                    Matrix4.CreateScale(new Vector3(elements[i].Scale.X * Window.DESIRED_ASPECT_RATIO, elements[i].Scale.Y, elements[i].Scale.Z)) *
                    Matrix4.CreateRotationX(elements[i].Rotation.X) *
                    Matrix4.CreateRotationY(elements[i].Rotation.Y) *
                    Matrix4.CreateRotationZ(elements[i].Rotation.Z) *
                    Matrix4.CreateTranslation(elements[i].Position);

                    elements[i].Mesh.Material.Shader.SetMatrix4("model", transform);
                    renderer.Render(elements[i].Mesh);
                }
                    
            }
            
        }
    }
} 
