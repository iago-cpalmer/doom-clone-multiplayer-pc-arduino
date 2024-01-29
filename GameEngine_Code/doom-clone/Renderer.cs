using doom_clone.Rendering.Lighting;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using doom_clone.Rendering.Model;
using doom_clone.Rendering.Shaders;
using System;
using doom_clone.Rendering.Textures;
using doom_clone.GameObjects;

namespace doom_clone
{
    struct UniformMatrices
    {
        public Matrix4 Projection;
        public Matrix4 View;
        public UniformMatrices(Matrix4 view, Matrix4 projection)
        {
            View = view;
            Projection = projection;
        }
    };
    public class Renderer
    {
        private int drawCalls = 0;
        //public UniformMatrices Matrices { get { return _matrices; } set { _matrices.View = value.View; _matrices.Projection = value.Projection;  } }
        private int _uboMatrices;
        private UniformMatrices _matrices;

        private int _uboMainLight;
        private int _uboPointLights;
        private int _uboSpotLights;
        public Renderer()
        {
            CreateLightBuffer();

            IntPtr offset = new IntPtr(sizeof(int) * 16);
            _matrices = new UniformMatrices(Matrix4.Zero, Matrix4.Identity);
            // Generate uniform buffers
            _uboMatrices = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, _uboMatrices);
            GL.BufferData(BufferTarget.UniformBuffer, sizeof(int)*16*2, ref _matrices.View, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Shader.UboBindings["Matrices"], _uboMatrices);
        }
        public void CreateLightBuffer()
        {
            if (!Shader.UboBindings.TryGetValue("MainLight", out _))
            {
                int x = Shaders.DEFAULT_SHADER.Handle;
            }
            _uboMainLight = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, _uboMainLight);
            GL.BufferData(BufferTarget.UniformBuffer, 64, (IntPtr) null, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Shader.UboBindings["MainLight"], _uboMainLight);
            /*
            _uboPointLights = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, _uboPointLights);
            GL.BufferData(BufferTarget.UniformBuffer, 512, (IntPtr) null, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Shader.UboBindings["PointLightsArray"], _uboPointLights);
            */
        }
        
        public void PrepareUI(ref Matrix4 view)
        {
            IntPtr offset = new IntPtr(sizeof(int) * 16);
            _matrices.View = view;
            GL.BindBuffer(BufferTarget.UniformBuffer, _uboMatrices);
            GL.BufferSubData(BufferTarget.UniformBuffer, offset, sizeof(int) * 16, ref _matrices.View);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        public void Prepare(ref Matrix4 view)
        {
            drawCalls = 0;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            IntPtr offset = new IntPtr(sizeof(int) * 16);

            _matrices.View = view;
            GL.BindBuffer(BufferTarget.UniformBuffer, _uboMatrices);
            GL.BufferSubData(BufferTarget.UniformBuffer, offset, sizeof(int) * 16, ref _matrices.View);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            
        }

        public void SetupLights(DirectionalLight light)
        {
            // Directional Light
            GL.BindBuffer(BufferTarget.UniformBuffer, _uboMainLight);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)0, 12, ref light.Direction);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)16, 12, ref light.AmbientColor);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)32, 12, ref light.DiffuseColor);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)48, 12, ref light.SpecularColor);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        public void EndRender()
        {
        }

        /**
         * <summary>
         * Method to write a point light to the point light uniform buffer.
         * It will be used when dynamic lighting gets added (like torch in player's hand)
         * </summary>
         */
        public void WritePointLightToBuffer(PointLight pointLight, int idLight, ref Matrix4 view)
        {
            pointLight.Position = Vector3.TransformPerspective(pointLight.Position, view);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)0 + idLight * 64, 12, ref pointLight.Position);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)12 + idLight * 64, 4, ref pointLight.constant);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)16 + idLight * 64, 12, ref pointLight.AmbientColor);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)28 + idLight * 64, 4, ref pointLight.linear);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)32 + idLight * 64, 12, ref pointLight.DiffuseColor);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)44 + idLight * 64, 4, ref pointLight.quadratic);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)48 + idLight * 64, 12, ref pointLight.SpecularColor);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)60 + idLight * 64, 4, ref pointLight.Intensity);
            
        }

        public void Render(GameObject gameObject)
        {
            drawCalls++;
            Matrix4 scale = Matrix4.CreateScale(new Vector3(gameObject.Scale.X, gameObject.Scale.Y, 1));            
            Matrix4 rotationY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(gameObject.Rotation.Y));
           // Matrix4 rotationX = rotationY * Matrix4.CreateRotationX(gameObject.Rotation.X);
            Matrix4 transform =  scale * rotationY * Matrix4.CreateTranslation(gameObject.Position);
            gameObject.Mesh.Material.Shader.SetMatrix4("model", transform);

            gameObject.Mesh.Material.BindMaterial();
            GL.BindVertexArray(gameObject.Mesh.VaoId);
            GL.DrawElements(PrimitiveType.Triangles, gameObject.Mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        public void Render(Mesh mesh)
        {
            drawCalls++;

            mesh.Material.BindMaterial();
            GL.BindVertexArray(mesh.VaoId);
            GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        public void SetProjectionMatrix(ref Matrix4 projection)
        {
            _matrices.Projection = projection;
            GL.BindBuffer(BufferTarget.UniformBuffer, _uboMatrices);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr) 0, sizeof(int)*16, ref _matrices.Projection);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }
    }
}
