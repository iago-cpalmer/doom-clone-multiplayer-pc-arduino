using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using doom_clone.Rendering.Shaders;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using System;
using doom_clone.Rendering.Lighting;
using doom_clone.Utils;
using doom_clone.GameObjects.Cameras;
using doom_clone.WorldClasses;
using doom_clone.GameObjects;
using doom_clone.Rendering.Model;
using doom_clone.Rendering.Materials;
using System.ComponentModel;

namespace doom_clone
{
    public class Window : GameWindow, IDisposable
    {
        public static Window Instance { get { return _instance; } }
        private static Window _instance;

        public bool Focused { get { return _focused; } set { _focused = value; } }
        public bool DebugMode { get { return _debugMode; } set { _debugMode = value; } }
        public bool FirstMoveAfterFocus { get { return _firstMoveAfterFocus; } set { _firstMoveAfterFocus = value; } }
        
        public static readonly float DESIRED_ASPECT_RATIO = 1920f / 1080f;

        private Renderer _renderer;

        private Stopwatch _timer = new Stopwatch();

        // TODO: UI CANVAS
        //private Mesh _uiMesh;
        private Camera _uiCamera;

        public static float DeltaTime;

        // Bool to save whether is the first frame after focusing the window to avoid big jumps in the
        // delta mouse position
        private bool _firstMoveAfterFocus = true;
        private bool _focused = false;

        // FPS Cap. Set to -1 for Unlimited fps
        public static int MaxFps = -1;
        public static int Width;
        public static int Height;
        private bool _debugMode = false;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            if (_instance == null)
            {
                _instance = this;
            } else
            {
                Dispose();
                return;
            }
            _timer.Start();
            InputHandler.InitInput();
            _uiCamera = new FPCamera(new Vector3(0, 0,-1), new Vector3(0, -90,0));
            CursorState = CursorState.Normal;
            if (MaxFps != -1)
            {
                UpdateFrequency = MaxFps;
            }
            Width = nativeWindowSettings.Size.X;
            Height = nativeWindowSettings.Size.Y;


            //_uiMesh = Mesh.CreateYPlane(new Vector2(-0.5f, -0.5f), new Vector2(0.5f, 0.5f), Materials.UI_MATERIAL);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.49f, 0.8f, 0.92f, 1f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.CullFace);

            _renderer = new Renderer();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            base.OnRenderFrame(e);
            Matrix4 view;
            Camera camera = GameManager.Instance.GetCurrentActiveCamera();
            // Render scene
            if (DebugMode)
            {
                view = Matrix4.LookAt(camera.Position, camera.Position - camera.Forward, new Vector3(0, 1, 0));
            }
            else
            {
                //TODO: Use main camera
                view = Matrix4.LookAt(camera.Position, camera.Position - camera.Forward, new Vector3(0, 1, 0));
            }
            _renderer.Prepare(ref view);
            GameManager.Instance.Render(ref _renderer);
            GL.Disable(EnableCap.DepthTest);
            
            // RENDER UI
            
            view = Matrix4.LookAt(_uiCamera.Position, _uiCamera.Position - _uiCamera.Forward, new Vector3(0, 1, 0));
            _renderer.PrepareUI(ref view);
            GameManager.Instance.RenderUI(ref _renderer);
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            DeltaTime = (float)e.Time;
            //Console.WriteLine("FPS: " + 1 / DeltaTime);
            InputHandler.UpdateMouseButtonStates(MouseState);

            if (Window.Instance.Focused)
            {
                InputHandler.UpdateInput(KeyboardState);

                if (!_firstMoveAfterFocus)
                {
                    InputHandler.UpdateMouseState(MouseState);
                }
                else
                {
                    _firstMoveAfterFocus = false;
                    InputHandler.MousePosition = MousePosition;
                }
            }

            // Switch debug mode
            if (InputHandler.IsKeyReleased(Inputs.SwitchDebugMode))
            {
                _debugMode = false;
            }
            GameManager.Instance.Update();
            NetworkManager.Instance.Update();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            Width = Size.X;
            Height = Size.Y;
            float aspectRatio = Width / Height;
            if(aspectRatio> DESIRED_ASPECT_RATIO)
            {
                Width = (int)(DESIRED_ASPECT_RATIO * Height);
            } else if(aspectRatio < DESIRED_ASPECT_RATIO)
            {
                Height = (int)(Width / DESIRED_ASPECT_RATIO);
            }
            GL.Viewport((Size.X - Width) / 2, (Size.Y - Height)/2, Width, Height);
            UpdateProjectionMatrix();
        }

        public void UpdateProjectionMatrix()
        {
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(GameManager.Instance.GetCurrentActiveCamera().FovY), DESIRED_ASPECT_RATIO, 0.1f, 1000.0f);
            _renderer.SetProjectionMatrix(ref projection);
        }

        public void SetCursorState(CursorState cursor)
        {
            CursorState = cursor;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            NetworkManager.Instance.SendDisconnection();
            base.OnClosing(e);
        }
    }
}