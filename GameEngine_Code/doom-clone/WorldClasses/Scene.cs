using doom_clone.GameObjects;
using doom_clone.GameObjects.Cameras;
using doom_clone.Rendering.Lighting;
using doom_clone.Rendering.Materials;
using doom_clone.Rendering.Model;
using doom_clone.UI;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace doom_clone.WorldClasses
{
    public class Scene
    {
        public UICanvas UICanvas { get { return _uiCanvas; } set { _uiCanvas = value; } }
        public String SceneName { get { return _sceneName; } }
        private String _sceneName;
        private List<Camera> _cameras;
        private int activeCamera = 0;
        private List<GameObject> _gameObjects;
        private UICanvas _uiCanvas;
        // TODO: LIGHTS WILL BE AN ARRAY OF LIGHTS
        private DirectionalLight _mainlight;
        public Scene(String sceneName)
        {
            _sceneName = sceneName;
            _gameObjects = new List<GameObject>();
            _cameras = new List<Camera>();

            // TODO: SET-UP LIGHTS IN GAME MANAGER WHEN CREATING A SCENE
            _mainlight = new DirectionalLight(direction: new Vector3(0.0f, 1.0f, 0.0f), ambientColor: new Vector3(0.5f, 0.5f, 0.5f),
    diffuseColor: new Vector3(0.0f, 0.0f, 0.0f), specularColor: new Vector3(0f, 0f, 0f));
        }

        public void Update()
        {
            // Update enemies positions
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                if (_gameObjects[i] is IUpdatable)
                {
                    ((IUpdatable)_gameObjects[i]).Update();
                }
            } 
        }

        public void RenderScene(ref Renderer renderer)
        {
            renderer.SetupLights(_mainlight);
            // Render all objects of scene
            for(int i = 0; i < _gameObjects.Count; i++)
            {
                if (_gameObjects[i].Mesh != null)
                {
                    renderer.Render(_gameObjects[i]);
                }
            }
        }

        public void LoadScene()
        {
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                if (_gameObjects[i] is IStartable)
                {
                    ((IStartable)_gameObjects[i]).Start();
                }
            }
        }


        public void AddGameObject(GameObject gameObject)
        {
            _gameObjects.Add(gameObject);
        }
        public void RemoveGameObject(GameObject gameObject)
        {
            _gameObjects.Remove(gameObject);
        }
        public void AddCamera(Camera camera)
        {
            _cameras.Add(camera);
        }
        public Camera GetActiveCamera()
        {
            return _cameras[activeCamera];
        }
        public void SetActiveCamera(int id)
        {
            activeCamera = id;
        }
        public GameObject FindGameObject(String name)
        {
            int i = 0;
            foreach (GameObject gameObject in _gameObjects)
            {
                if(gameObject.Name==name)
                {
                    return _gameObjects[i];
                }
                i++;
            }
            return null;
        }
    }
}
