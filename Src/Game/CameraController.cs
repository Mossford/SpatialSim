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

        public CameraController(EcsComponentRef cameraRef)
        {
            this.cameraRef = cameraRef;
        }

        public void Update()
        {
            if (Input.IsMouseButtonDown(MouseButton.Right))
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
            else
            {
                Input.mouse.Cursor.CursorMode = CursorMode.Normal;
            }
        }
    }
}