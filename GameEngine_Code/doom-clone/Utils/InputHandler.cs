using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace doom_clone.Utils
{
    
    public class InputHandler
    {       
        public static int KeyStates { get { return _keyStates; } }
        public static byte MouseState { get { return _mouseButtonsState; } }

        private static MapKey[] _keys;
        private static int _keyStates;

        // Up = 1, Still = 0, Down = -1
        public static float MouseWheel;
        private static byte _mouseButtonsState;
        // Mouse coordinates in screen from top-left corner
        public static Vector2 MousePosition;
        // Difference between last frame's mouse position and current one
        public static Vector2 DeltaMousePosition;
        public static void InitInput()
        {
            _keys = new MapKey[Enum.GetValues(typeof(Inputs)).Length];
            _keys[0] = new MapKey(Inputs.MoveLeft, Keys.A);
            _keys[1] = new MapKey(Inputs.MoveUp, Keys.W);
            _keys[2] = new MapKey(Inputs.MoveRight, Keys.D);
            _keys[3] = new MapKey(Inputs.MoveDown, Keys.S);
            _keys[4] = new MapKey(Inputs.Jump, Keys.Space);
            _keys[5] = new MapKey(Inputs.Jump, Keys.LeftShift);
            _keys[6] = new MapKey(Inputs.Interact, Keys.E);
            _keys[7] = new MapKey(Inputs.SwitchDebugMode, Keys.F3);
            _keys[8] = new MapKey(Inputs.Escape, Keys.Escape);
            _keys[9] = new MapKey(Inputs.LeftControl, Keys.LeftControl);
        }

        public static void UpdateInput(KeyboardState input)
        {
            for(int i = 0; i < _keys.Length; i++)
            {
                _keyStates &= ~(0b11 << (i<<1));
                if (input.IsKeyPressed(_keys[i].Key))
                {
                    _keyStates |= ((int)KeyState.Pressed) << (i<<1);
                } else if (input.IsKeyDown(_keys[i].Key))
                {
                    _keyStates |= ((int)KeyState.Down) << (i<<1);
                } else if(input.IsKeyReleased(_keys[i].Key))
                {
                    _keyStates |= ((int)KeyState.Released) << (i<<1);
                }
            }
        }
        public static void UpdateMouseState(MouseState mouseState)
        {
            DeltaMousePosition = mouseState.Position - MousePosition;
            MousePosition = mouseState.Position;
            MouseWheel = mouseState.ScrollDelta.Y;
        }
        public static void UpdateMouseButtonStates(MouseState mouseState)
        {
            UpdateMouseButton(mouseState, MouseButton.Button1);
            UpdateMouseButton(mouseState, MouseButton.Button2);
            UpdateMouseButton(mouseState, MouseButton.Button3);
        }
        private static void UpdateMouseButton(MouseState state, MouseButton mouseButton)
        {
            _mouseButtonsState &= (byte)(~(0b11 << ((int)mouseButton << 1)));
            if (state.IsButtonPressed(mouseButton))
            {
                _mouseButtonsState |= (byte)((int)KeyState.Pressed << ((int)mouseButton << 1));
            } else if(state.IsButtonDown(mouseButton))
            {
                _mouseButtonsState |= (byte)((int)KeyState.Down << ((int)mouseButton << 1));
            } else if(state.IsButtonReleased(mouseButton))
            {
                _mouseButtonsState |= (byte)((int)KeyState.Released << ((int)mouseButton << 1));
            }
        }

        public static KeyState GetMouseButtonState(byte mouseButton)
        {
            return (KeyState)(Enum.GetValues(typeof(KeyState)).GetValue((int)(_mouseButtonsState & (byte)(0b11 << (mouseButton))) >>mouseButton));
        }
        public static bool IsMouseButtonDown(byte mouseButton)
        {
            return GetMouseButtonState(mouseButton) == KeyState.Down;
        }
        public static bool IsMouseButtonPressed(byte mouseButton)
        {
            return GetMouseButtonState(mouseButton) == KeyState.Pressed;
        }
        public static bool IsMouseButtonReleased(byte mouseButton)
        {
            return GetMouseButtonState(mouseButton) == KeyState.Released;
        }
        /**
         * <param name="control">Specific input to check</param>
         * <returns> State of a input </returns>
         */
        public static KeyState GetInputState(Inputs control)
        {
            return (KeyState)(Enum.GetValues(typeof(KeyState)).GetValue((int)(_keyStates & (long)(0b11 << ((int)control)))>>(int)control));
        }
        public static bool IsKeyDown(Inputs control)
        {
            return GetInputState(control) == KeyState.Down;
        }
        public static bool IsKeyPressed(Inputs control)
        {
            return GetInputState(control) == KeyState.Pressed;
        }
        public static bool IsKeyReleased(Inputs control)
        {
            return GetInputState(control) == KeyState.Released;
        }
    }
    
    struct MapKey
    {
        public Inputs Input;
        public Keys Key;
        public MapKey(Inputs input, Keys key)
        {
            Input = input;
            Key = key;
        }
    }
    [Flags]
    public enum Inputs
    {
        MoveLeft = 0,
        MoveUp = 2,
        MoveRight = 4,
        MoveDown = 6,
        Jump = 8,
        Down = 10,
        Interact = 12,
        SwitchDebugMode = 14,
        Escape = 16,
        LeftControl = 18,
    }
    [Flags]
    public enum KeyState
    {
        Up = 0b00,
        Pressed = 0b01,
        Down = 0b10,
        Released = 0b11
    }
}
