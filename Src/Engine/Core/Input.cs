using System.Numerics;
using SDL;
using Silk.NET.Input;

namespace SpatialSim.Engine.Core
{
    public static class Input
    {
        public static Dictionary<int, bool> keysPressed;
        public static Dictionary<int, bool> mousePressed;

        public static Vector2 mousePosition;
        public static Vector2 mouseLocalPosition;
        static Vector2 lastMousePosition;
        public static Vector2 mouseDelta;
        public static bool uiWantMouse;
        public static Vector2 mouseWheel;
        public static bool mouseLocked;

        public static void Init()
        {
            keysPressed = new Dictionary<int, bool>();
            mousePressed = new Dictionary<int, bool>();
        }

        public static void Update(in SDL_Event sdlEvent)
        {
            if (sdlEvent.type is (uint)SDL_EventType.SDL_EVENT_KEY_DOWN or (uint)SDL_EventType.SDL_EVENT_KEY_UP)
            {
                unsafe
                {
                    int keyCount;
                    bool* keyState = (bool*)SDL3.SDL_GetKeyboardState(&keyCount);
                    for (int i = 0; i < keyCount; i++)
                    {
                        if (keyState[i])
                        {
                            if(!keysPressed.TryAdd(i, true))
                            {
                                keysPressed[i] = true;
                            }
                        }
                        else
                        {
                            keysPressed[i] = false;
                        }
                    }    
                }
            }
            
            if (sdlEvent.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_WHEEL)
            {
                mouseWheel = new Vector2(sdlEvent.wheel.x, sdlEvent.wheel.y);
            }

            if (sdlEvent.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN)
            {
                if(!mousePressed.TryAdd((int)sdlEvent.button.Button, true))
                {
                    mousePressed[(int)sdlEvent.button.Button] = true;
                }
            }
            
            if (sdlEvent.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP)
            {
                if(!mousePressed.TryAdd((int)sdlEvent.button.Button, false))
                {
                    mousePressed[(int)sdlEvent.button.Button] = false;
                }
            }
        }

        public static void UpdateNonEvent()
        {
            mouseWheel = new Vector2();
            
            lastMousePosition = mousePosition;
            unsafe
            {
                float x, y;
                SDL3.SDL_GetMouseState(&x, &y);
                mousePosition = new Vector2(x, y) / Window.windowScale;
            }
            mouseDelta = mousePosition - lastMousePosition;
            mouseLocalPosition = ((mousePosition * 2) - Window.size) / 2;
        }

        public static bool IsKeyDown(SDL_Scancode key)
        {
            if (keysPressed.ContainsKey((int)key))
            {
                if (keysPressed[(int)key])
                    return true;
            }
            return false;
        }

        public static bool IsKeyUp(SDL_Scancode key)
        {
            if (keysPressed.ContainsKey((int)key))
            {
                if (!keysPressed[(int)key])
                    return true;
            }
            return false;
        }

        public static bool IsMouseButtonDown(SDLButton button)
        {
            if (mousePressed.ContainsKey((int)button))
            {
                if(mousePressed[(int)button])
                    return true;
            }
            return false;
        }
        
        public static bool IsMouseButtonUp(SDLButton button)
        {
            if (mousePressed.ContainsKey((int)button))
            {
                if(!mousePressed[(int)button])
                    return true;
            }
            return false;
        }

        public static unsafe void SetMouseLocked(bool state)
        {
            //TODO this should be correct as the mouse will go to window edge which stops mouse movement
            SDL3.SDL_SetWindowRelativeMouseMode(AppState.window, state);
            mouseLocked = state;
        }

        public static void Clear()
        {
            keysPressed.Clear();
            mousePressed.Clear();
        }

        public static void Clean()
        {
            
        }
    }
}
