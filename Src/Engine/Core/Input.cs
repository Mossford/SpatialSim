using System.Numerics;
using Silk.NET.Input;

namespace SpatialSim.Engine.Core
{
    public static class Input
    {
        public static Dictionary<int, int> keysPressed;
        public static IInputContext input;
        public static IKeyboard keyboard;
        
        public static IMouse mouse;
        public static Vector2 position;
        public static Vector2 localPosition;
        public static Vector2 lastPosition;
        public static Vector2 lastLocalPosition;
        public static bool uiWantMouse;
        public static int scroll;
        static int lastScroll;

        public static void Init()
        {
            input = AppState.window.CreateInput();
            keyboard = input.Keyboards.FirstOrDefault();
            keysPressed = new Dictionary<int, int>();
            
            mouse = input.Mice[0];
        }

        public static void Update()
        {
            for (int i = 0; i < keyboard.SupportedKeys.Count; i++)
            {
                if (keyboard.IsKeyPressed(keyboard.SupportedKeys[i]))
                {
                    if(!keysPressed.ContainsKey((int)keyboard.SupportedKeys[i]))
                    {
                        keysPressed.Add((int)keyboard.SupportedKeys[i], 1);
                    }
                    else
                    {
                        keysPressed[(int)keyboard.SupportedKeys[i]] = 1;
                    }
                }
                else
                {
                    if (!keysPressed.ContainsKey((int)keyboard.SupportedKeys[i]))
                    {
                        keysPressed.Add((int)keyboard.SupportedKeys[i], 0);
                    }
                    else
                    {
                        keysPressed[(int)keyboard.SupportedKeys[i]] = 0;
                    }
                }
            }
            
            lastPosition = position;
            lastLocalPosition = ((lastPosition * 2) - Window.size) / 2;
            position = mouse.Position * Window.windowScale;
            localPosition = ((position * 2) - Window.size) / 2;

            scroll = (int)mouse.ScrollWheels.FirstOrDefault().Y;
        }

        public static bool IsKeyDown(Key key)
        {
            if (keysPressed.ContainsKey((int)key))
            {
                if (keysPressed[(int)key] == 1)
                    return true;
            }
            return false;
        }

        public static bool IsKeyUp(Key key)
        {
            if (keysPressed.ContainsKey((int)key))
            {
                if (keysPressed[(int)key] == 0)
                    return true;
            }
            return false;
        }

        public static bool IsMouseButtonDown(MouseButton button)
        {
            return mouse.IsButtonPressed(button);
        }

        public static void Clear()
        {
            keysPressed.Clear();
        }

        public static void Clean()
        {
            input.Dispose();
        }
    }
}