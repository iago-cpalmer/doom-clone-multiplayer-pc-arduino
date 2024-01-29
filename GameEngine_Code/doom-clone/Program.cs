using doom_clone.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace doom_clone
{
    class Program
    {
        static void Main(string[] args)
        {
            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "Doom Clone For Arduino",
                // run on macos
                Flags = ContextFlags.ForwardCompatible
            };
            new Window(GameWindowSettings.Default, nativeWindowSettings);
            Prefabs.Init();
            new GameManager();
            Console.WriteLine("What's your name?");
            new NetworkManager(Console.ReadLine());

            
            Window.Instance.Run();
        }
    }
}