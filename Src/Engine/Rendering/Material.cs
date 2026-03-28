using System.Numerics;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class Material : IComponent
    {
        public int type => EcsComponentType.Material.GetId();
        public int id { get; set; } = -1;

        public Vector3 ambient;
        public Vector3 diffuse;
        public Vector3 specular;
        public float specularExp;
        public int materialId;

        public string textureRef;
        public string normalMapRef;

        public Material()
        {
            textureRef = "";
            normalMapRef = "";
            ambient = new Vector3(0.1f);
            diffuse = new Vector3(1f);
            specular = new Vector3(0f);
            specularExp = 1f;
        }
        
        public void Dispose()
        {
            
        }
    }
}