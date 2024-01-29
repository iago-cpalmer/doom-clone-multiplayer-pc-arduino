using doom_clone.GameObjects;
using doom_clone.GameObjects.Cameras;
using doom_clone.Rendering.Materials;
using doom_clone.Rendering.Model;
using doom_clone.Rendering.Textures;
using doom_clone.UI;
using doom_clone.Utils;
using doom_clone.WorldClasses;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace doom_clone
{
    public class GameManager
    {
        public static GameManager Instance { get { return _instance; } set { _instance = value; } }

        private static GameManager _instance;

        public const int WALL_HEIGHT = 5;

        private Dictionary<String, Scene> _scenes;
        // Game objects that are synchronized with the arduino board with each its unique id (uid).
        private Dictionary<int, GameObject> _networkGameObjects;
        private String activeScene = "InGame";
        public GameManager()
        {
            Instance = this;
            _scenes = new Dictionary<String, Scene>();
            _networkGameObjects = new Dictionary<int, GameObject>();
            // Initialize Main Menu Scene
            Scene mainMenu = new Scene("MainMenu");
            AddScene(mainMenu);
            // Initialize Death Scene
            Scene deathScene = new Scene("DeathScene");
            deathScene.AddGameObject(new DeathSceneHandler());
            UICanvas deathCanvas = new UICanvas();
            deathCanvas.AddElement(new UIImage(new Vector3(0.0f, 0.0f, 0.0f), Vector3.Zero, new Vector3(-0.5f, 0.5f, 1.0f), Textures.DEATH_SCREEN));
            deathScene.UICanvas = deathCanvas;
            deathScene.AddCamera(new FPCamera(new Vector3(5, 31, 25), new Vector3(0, -90, 0)));
            AddScene(deathScene);

            // Initialize in-game scene
            Scene inGame = new Scene("InGame");
            // Add cameras
            inGame.AddCamera(new FPCamera(new Vector3(5, 31, 25), new Vector3(0, -90, 0)));
            // Load level
            string jsonString = File.ReadAllText("..\\..\\..\\Res\\LevelData\\test3.json");
            List<Line> lines = JsonSerializer.Deserialize<List<Line>>(jsonString);
            foreach (Line l in lines)
            {
                inGame.AddGameObject(new WallObject(Vector3.Zero, Vector3.Zero, new Vector3(1,10,1), l));
            }
            // Floor
            inGame.AddGameObject(new GameObject(Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1), Mesh.CreatePlane(Vector2.Zero, new Vector2(500, 500), Materials.FLOOR_MATERIAL)));
            // Ceiling
            inGame.AddGameObject(new GameObject(new Vector3(0, WALL_HEIGHT*10, 0), Vector3.Zero, new Vector3(1,1,1), Mesh.CreatePlane(Vector2.Zero, new Vector2(500, 500), Materials.WALL_MATERIAL)));

            // Add UI Canvas...
            UIElement[] hearts = new UIElement[3];
            UIElement[] ammo = new UIElement[20];
            UICanvas canvas = new UICanvas();
            hearts[0] = new UIImage(new Vector3(1.0f, 0.5f, 1.0f), Vector3.Zero, new Vector3(0.1f, 0.1f, 1.0f), Textures.HEART_FULL_ICON_PATH);
            hearts[1] = new UIImage(new Vector3(0.875f, 0.5f, 1.0f), Vector3.Zero, new Vector3(0.1f, 0.1f, 1.0f), Textures.HEART_FULL_ICON_PATH);
            hearts[2] = new UIImage(new Vector3(0.75f, 0.5f, 1.0f), Vector3.Zero, new Vector3(0.1f, 0.1f, 1.0f), Textures.HEART_FULL_ICON_PATH);
            canvas.AddElement(hearts[0]);
            canvas.AddElement(hearts[1]);
            canvas.AddElement(hearts[2]);
            for(int i = 0; i < 20; i++)
            {
                ammo[i] = new UIImage(new Vector3(1.05f-(i*0.025f), 0.4f, 1.0f), Vector3.Zero, new Vector3(0.05f, 0.05f, 1.0f), Textures.BULLET_FULL_ICON_PATH);
                canvas.AddElement(ammo[i]);
            }
            canvas.AddElement(new UIImage(new Vector3(0.0f,0.0f,0.0f), Vector3.Zero, new Vector3(-0.5f, 0.5f, 1.0f), Textures.WEAPON_IN_HAND));

            inGame.UICanvas = canvas;

            inGame.AddGameObject(new InGameHandler(inGame, hearts, ammo));
            // Add scene to scenes dictionary
            AddScene(inGame);
        }

        public void Update()
        {
            // Update main scene
            _scenes[activeScene].Update();
        }
        public void Render(ref Renderer renderer)
        {
            _scenes[activeScene].RenderScene(ref renderer);
        }
        public void RenderUI(ref Renderer renderer)
        {
            // Will render current's active scene UI Canvas
            if(GetActiveScene().UICanvas!=null)
            {
                GetActiveScene().UICanvas.RenderUI(ref renderer);
            }
        }
        public void SetActiveScene(String scene)
        {
            activeScene = scene;
            _scenes[scene].LoadScene();
        }
        public void AddScene(Scene scene)
        {
            _scenes.Add(scene.SceneName, scene);
        }
        public void RegisterNetworkObject(int uid, GameObject gameObject, String sceneName)
        {
            _networkGameObjects.Add(uid, gameObject);
            _scenes[sceneName].AddGameObject(gameObject);
        }
        public void UnregisterNetworkObject(int uid)
        {
            _scenes["InGame"].RemoveGameObject(_networkGameObjects[uid]);
            _networkGameObjects.Remove(uid);
        }
        public void UpdateNetworkObject(int uid, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            _networkGameObjects[uid].Position = position;
            _networkGameObjects[uid].Rotation = rotation;
            _networkGameObjects[uid].Scale = scale;
        }
        public Scene GetActiveScene()
        {
            return _scenes[activeScene];
        }
        public Camera GetCurrentActiveCamera()
        {
            return _scenes[activeScene].GetActiveCamera();
        }
        public GameObject GetNetworkObject(int uid)
        {
            return _networkGameObjects[uid];
        }

        public Scene GetScene(String name)
        {
            return _scenes[name];
        }
    }
}
