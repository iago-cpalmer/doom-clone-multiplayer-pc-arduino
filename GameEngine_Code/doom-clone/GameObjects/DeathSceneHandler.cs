using doom_clone.WorldClasses;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace doom_clone.GameObjects
{
    public class DeathSceneHandler : GameObject, IUpdatable, IStartable
    {
        private const float COUNTDOWN_MAX = 3.0f;
        private float _countdown = 0.0f;
        public DeathSceneHandler()
        : base(Vector3.Zero, Vector3.Zero, Vector3.Zero, null)
        {
            Name = "InGameHandler";
            
        }

        public void Start()
        {
            _countdown = COUNTDOWN_MAX;
        }

        public void Update()
        {
            _countdown -= Window.DeltaTime;
            if(_countdown <= 0.0f)
            {
                GameManager.Instance.SetActiveScene("InGame");
            }
        }
    }
}
