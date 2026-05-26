using System.Numerics;
using SDL;
using Silk.NET.Input;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Rendering;

namespace SpatialSim.Game
{
    public class CameraController
    {
        EcsComponentRef cameraRef;

        public Camera camera
        {
            get
            {
                return EcsManager.GetComponent<Camera>(cameraRef);  
            }
            private set;
        }

        public float sensitivity = 0.003f;
        public float speed = 0.001f;
        private static bool keyUp;

        public CameraController(EcsComponentRef cameraRef)
        {
            this.cameraRef = cameraRef;
        }

        public void Update()
        {
            if (keyUp)
            {
                if (Input.IsKeyDown(SDL_Scancode.SDL_SCANCODE_F) && !Input.mouseLocked)
                {
                    Input.SetMouseLocked(true);
                
                    Vector2 mousePosMoved = Input.mouseDelta;
                    mousePosMoved *= camera.fov * sensitivity;
                    camera.transformRef.rotation += new Vector3(-mousePosMoved.X, mousePosMoved.Y, 0f);

                    if (Input.mouseWheel.Y != 0)
                    {
                        camera.fov -= camera.fov * 0.1f * Input.mouseWheel.Y;
                        camera.fov = MathF.Max(MathF.Min(170.0f, camera.fov), 0f);
                    }
                }
                else if(Input.IsKeyDown(SDL_Scancode.SDL_SCANCODE_F) && Input.mouseLocked)
                {
                    Input.SetMouseLocked(false);
                }

                keyUp = false;
            }

            if (Input.IsKeyUp(SDL_Scancode.SDL_SCANCODE_F))
                keyUp = true;

            if (Input.mouseLocked)
            {
                Vector2 mousePosMoved = Input.mouseDelta;
                mousePosMoved *= camera.fov * sensitivity;
                camera.transformRef.rotation += new Vector3(-mousePosMoved.X, mousePosMoved.Y, 0f);
                if(camera.transformRef.rotation.Y > 89.0f)
                    camera.transformRef.rotation.Y =  89.0f;
                if(camera.transformRef.rotation.Y < -89.0f)
                    camera.transformRef.rotation.Y = -89.0f;

                if (Input.mouseWheel.Y != 0)
                {
                    camera.fov -= camera.fov * 0.1f * Input.mouseWheel.Y;
                    camera.fov = MathF.Max(MathF.Min(170.0f, camera.fov), 0f);
                }
            }
            
            if (Input.IsKeyDown(SDL_Scancode.SDL_SCANCODE_W))
            {
                camera.transformRef.position += camera.transformRef.GetForward() * speed;
            }
            if (Input.IsKeyDown(SDL_Scancode.SDL_SCANCODE_S))
            {
                camera.transformRef.position -= camera.transformRef.GetForward() * speed;
            }
            if (Input.IsKeyDown(SDL_Scancode.SDL_SCANCODE_A))
            {
                camera.transformRef.position += camera.transformRef.GetRight() * speed;
            }
            if (Input.IsKeyDown(SDL_Scancode.SDL_SCANCODE_D))
            {
                camera.transformRef.position -= camera.transformRef.GetRight() * speed;
            }
            if (Input.IsKeyDown(SDL_Scancode.SDL_SCANCODE_SPACE))
            {
                camera.transformRef.position -= camera.transformRef.GetUp() * speed;
            }
            if (Input.IsKeyDown(SDL_Scancode.SDL_SCANCODE_LSHIFT))
            {
                camera.transformRef.position += camera.transformRef.GetUp() * speed;
            }
        }
    }
}