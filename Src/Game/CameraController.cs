using System.Numerics;
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
        private static bool keyUp;

        public CameraController(EcsComponentRef cameraRef)
        {
            this.cameraRef = cameraRef;
        }

        public void Update()
        {
            if (keyUp)
            {
                if (Input.IsKeyDown(Key.F) && Input.mouse.Cursor.CursorMode == CursorMode.Normal)
                {
                    Input.mouse.Cursor.CursorMode = CursorMode.Raw;
                
                    Vector2 mousePosMoved = Input.position - Input.lastPosition;
                    mousePosMoved *= camera.fov * sensitivity;
                    camera.transformRef.rotation += new Vector3(mousePosMoved.X, -mousePosMoved.Y, 0f);

                    if (Input.scroll != 0)
                    {
                        camera.fov -= camera.fov * 0.1f * Input.scroll;
                        camera.fov = MathF.Max(MathF.Min(170.0f, camera.fov), 0f);
                    }
                }
                else if(Input.IsKeyDown(Key.F) && Input.mouse.Cursor.CursorMode == CursorMode.Raw)
                {
                    Input.mouse.Cursor.CursorMode = CursorMode.Normal;
                }

                keyUp = false;
            }

            if (Input.IsKeyUp(Key.F))
                keyUp = true;

            if (Input.mouse.Cursor.CursorMode == CursorMode.Raw)
            {
                Vector2 mousePosMoved = Input.position - Input.lastPosition;
                mousePosMoved *= camera.fov * sensitivity;
                camera.transformRef.rotation += new Vector3(-mousePosMoved.X, mousePosMoved.Y, 0f);

                if (Input.scroll != 0)
                {
                    camera.fov -= camera.fov * 0.1f * Input.scroll;
                    camera.fov = MathF.Max(MathF.Min(170.0f, camera.fov), 0f);
                }
            }

            if (Input.IsKeyDown(Key.W))
            {
                camera.transformRef.position += new Vector3(0, 0, 0.001f);
            }
            if (Input.IsKeyDown(Key.S))
            {
                camera.transformRef.position -= new Vector3(0, 0, 0.001f);
            }
            if (Input.IsKeyDown(Key.A))
            {
                camera.transformRef.position -= new Vector3(0.001f, 0, 0);
            }
            if (Input.IsKeyDown(Key.D))
            {
                camera.transformRef.position += new Vector3(0.001f, 0, 0);
            }
        }
    }
}