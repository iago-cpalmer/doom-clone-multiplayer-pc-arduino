using OpenTK.Mathematics;
using doom_clone.GameObjects.Cameras;
using doom_clone.Utils;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using doom_clone.WorldClasses;
using System;
using doom_clone.UI;
using doom_clone.Rendering.Materials;
using doom_clone.Rendering.Shaders;
using doom_clone.Rendering.Textures;

namespace doom_clone.GameObjects
{
    public class InGameHandler : GameObject, IUpdatable
    {
        private Scene _scene;
        private Camera _cameraToUpdate;
        private Camera _playerCamera;
        private GameObject _player;
        private bool debugMode;
        private UIElement[] _hearts;
        private UIElement[] _ammo;

        double sinStep1 = 0.0f;
        double sinStep2 = 0.0f;

        private Material _matFullHearth = new DefaultMaterial(Shaders.UI_SHADER, texturePath: Textures.HEART_FULL_ICON_PATH, specularTexturePath: Textures.BLACK_TEXTURE_PATH,
                emissionTexturePath: Textures.BLACK_TEXTURE_PATH,
                ambientColor: new Vector3(1.0f, 1.0f, 1.0f),
                shininess: 0.0f);
        private Material _matEmptyHeart = new DefaultMaterial(Shaders.UI_SHADER, texturePath: Textures.HEART_EMPTY_ICON_PATH, specularTexturePath: Textures.BLACK_TEXTURE_PATH,
                emissionTexturePath: Textures.BLACK_TEXTURE_PATH,
                ambientColor: new Vector3(1.0f, 1.0f, 1.0f),
                shininess: 0.0f);
        private Material _matFullBullet = new DefaultMaterial(Shaders.UI_SHADER, texturePath: Textures.BULLET_FULL_ICON_PATH, specularTexturePath: Textures.BLACK_TEXTURE_PATH,
                emissionTexturePath: Textures.BLACK_TEXTURE_PATH,
                ambientColor: new Vector3(1.0f, 1.0f, 1.0f),
                shininess: 0.0f);
        private Material _matEmptyBullet = new DefaultMaterial(Shaders.UI_SHADER, texturePath: Textures.BULLET_EMPTY_ICON_PATH, specularTexturePath: Textures.BLACK_TEXTURE_PATH,
                emissionTexturePath: Textures.BLACK_TEXTURE_PATH,
                ambientColor: new Vector3(1.0f, 1.0f, 1.0f),
                shininess: 0.0f);
        public InGameHandler(Scene scene, UIElement[] hearts, UIElement[] ammo)
        : base(Vector3.Zero, Vector3.Zero, Vector3.Zero, null)
        {
            Name = "InGameHandler";
            _cameraToUpdate = scene.GetActiveCamera();
            _playerCamera = new FPCamera(Vector3.Zero, Vector3.Zero);
            _scene = scene;
            _scene.AddCamera(_playerCamera);
            debugMode = Window.Instance.DebugMode;
            _hearts = hearts;
            _ammo = ammo;
        }

        public void Update()
        {
            debugMode = Window.Instance.DebugMode;
            _scene.SetActiveCamera(debugMode ? 0 : 1);
            if (InputHandler.IsKeyPressed(Inputs.Escape) && Window.Instance.Focused)
            {
                Window.Instance.SetCursorState(CursorState.Normal);
                Window.Instance.Focused = false;
            }
            if (InputHandler.IsMouseButtonDown(0) && !Window.Instance.Focused)
            {
                Window.Instance.SetCursorState(CursorState.Grabbed);
                Window.Instance.FirstMoveAfterFocus = true;
                Window.Instance.Focused = true;
            }
            // TODO: Send camera update state as player rotation to board
            if (_player != null)
            {
                _playerCamera.Update();
                // Update main camera
                _playerCamera.Position = _player.Position;
                sinStep1 = (sinStep1 + 90*Window.DeltaTime) % 360;
                sinStep2 = (sinStep2 + 90 * Window.DeltaTime) % 360;
                _playerCamera.Position.Y += (float)MathHelper.Sin(MathHelper.DegreesToRadians(sinStep2));
                _playerCamera.Rotation = _player.Rotation;
            }
            if (_player != null)
            {
                // Update hearts depending on player's health
                for (int i = 0; i < _hearts.Length; i++)
                {
                    if (i < _player.Rotation.Z)
                    {
                        // Full heart
                        _hearts[i].Mesh.Material = _matFullHearth;
                    }
                    else
                    {
                        // Emtpy heart
                        _hearts[i].Mesh.Material = _matEmptyHeart;
                    }
                }

                // Update ammo
                for (int i = 0; i < _ammo.Length; i++)
                {
                    if (i < _player.Scale.Z)
                    {
                        _ammo[i].Mesh.Material = _matFullBullet;
                    }
                    else
                    {
                        _ammo[i].Mesh.Material = _matEmptyBullet;
                    }
                }
            }
        }

        public void SetPlayer(GameObject player)
        {
            _player = player;
        }
    }
}
