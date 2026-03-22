using System.Numerics;
using System.Text;
using Silk.NET.Assimp;
using SpatialSim.Engine.Core;
using File = System.IO.File;

namespace SpatialSim.Engine.Rendering
{
    public static class ModelLoader
    {
        static List<Vector2> tmpUV;
        static List<Vector3> tmpNormal;
        static List<Vector3> tmpVertice;
        static int objectCount = 0;
        static int materialCount = 0;
        static long lastPosition;
        
        static Dictionary<string, Material> loadedMaterials;
        static Dictionary<string, string> loadedTextures;
        static List<string> objectToMat;

        static StreamReader reader;

        static Queue<string> textureQueue;
        
        public static Entity LoadModelFile(string modelFile, string materialFile, Transform transform, EcsComponentRef camera)
        {
            Entity entity = EcsManager.AddEntity();
            textureQueue = new Queue<string>();
            
            if (!File.Exists(Resources.ModelPath + modelFile))
            {
                Debug.Warning($"Could not file model at path {modelFile} to load");
                return entity;
            }

            Debug.LogDebug($"Loading model {modelFile}");
            lock (entity)
            {
                ThreadPool.QueueUserWorkItem(state => LoadModel(entity, modelFile, materialFile, transform, camera));
            }

            for (int i = 0; i < textureQueue.Count; i++)
            {
                TextureManager.LoadTexture(textureQueue.Dequeue());
            }
            
            Debug.LogDebug($"Loaded model {modelFile}");
            
            return entity;
        }

        unsafe static void LoadModel(Entity entity, string modelFile, string materialFile, Transform transform, EcsComponentRef camera)
        {
            Assimp assimp = Assimp.GetApi();
            Scene* scene = assimp.ImportFile(Resources.ModelPath + modelFile, (uint)PostProcessPreset.TargetRealTimeMaximumQuality);
            
            loadedMaterials = new Dictionary<string, Material>();
            loadedTextures = new Dictionary<string, string>();
            objectToMat = new List<string>();

            if (scene != null)
            {
                if (scene->MRootNode != null)
                {
                    LoadNode(assimp, scene, scene->MRootNode, entity, transform, camera);
                    Debug.LogDebug($"Loaded root node");
                    
                    for (int i = 0; i < scene->MRootNode->MNumChildren; i++)
                    {
                        Debug.LogDebug($"Loaded node {i}");
                        LoadNode(assimp, scene, scene->MRootNode->MChildren[i], entity, transform, camera);
                    }
                }
                else
                {
                    for (int i = 0; i < scene->MNumMeshes; i++)
                    {
                        Debug.LogDebug($"Loaded mesh {i}");
                        LoadSubMesh(assimp, scene, scene->MMeshes[i], entity, transform, camera);
                    }
                }
            }
            else
            {
                Debug.Error($"Assimp failed with, {assimp.GetErrorStringS()}");
            }
            
            assimp.ReleaseImport(scene);
        }

        unsafe static void LoadNode(Assimp assimp, Scene* scene, Node* node, Entity entity, Transform transform, EcsComponentRef camera)
        {
            for (int i = 0; i < node->MNumMeshes; i++)
            {
                LoadSubMesh(assimp, scene, scene->MMeshes[node->MMeshes[i]], entity, transform, camera);
            }
        }

        unsafe static void LoadSubMesh(Assimp assimp, Scene* scene, Silk.NET.Assimp.Mesh* mesh, Entity entity, Transform transform, EcsComponentRef camera)
        {
            MeshData meshData = new MeshData();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();

            for (int f = 0; f < mesh->MNumFaces; f++)
            {
                Face face = mesh->MFaces[f];

                for (int i = 0; i < face.MNumIndices; i++)
                {
                    uint index = face.MIndices[i];
                    indices.Add(vertices.Count);

                    vertices.Add(mesh->MVertices[index]);
                    if(mesh->MNormals != null)
                        normals.Add(mesh->MNormals[index]);
                    else
                        normals.Add(new Vector3(0f));
                    if(mesh->MTextureCoords.Element0 != null)
                        uvs.Add(new Vector2(mesh->MTextureCoords.Element0[index].X, mesh->MTextureCoords.Element0[index].Y));
                    else
                        uvs.Add(new Vector2(0f));
                }
            }

            meshData.vertexData.vertices = vertices.ToArray();
            meshData.vertexData.normals = normals.ToArray();
            meshData.vertexData.uvs = uvs.ToArray();
            meshData.indices = indices.ToArray();
            
            Mesh meshComp = new Mesh(meshData, entity.AddComponentThr(transform));
            int materialIndex = (int)mesh->MMaterialIndex;
            //grab the material associated with this mesh
            Material mat = LoadSubMaterial(assimp, scene, materialIndex);
            
            entity.AddComponentThr(new MeshRenderer(entity.AddComponentThr(meshComp), entity.AddComponentThr(mat), camera));
            
            Debug.LogDebug($"Loaded sub mesh {mesh->MName.AsString}");
        }

        unsafe static Material LoadSubMaterial(Assimp assimp, Scene* scene, int index)
        {
            Material material = new Material();

            if (index >= scene->MNumMaterials)
            {
                Debug.Warning($"Material index {index} was greater than number of loaded materials {scene->MNumMaterials}, skipping material loading");
                return material;
            }
            
            Silk.NET.Assimp.Material* mat = scene->MMaterials[index];
            
            for (int i = 0; i < mat->MNumProperties; i++)
            {
                if (mat->MProperties[i]->MKey == Assimp.MatkeyColorDiffuse)
                {
                    Vector4 color = new Vector4();
                    assimp.GetMaterialColor(mat, mat->MProperties[i]->MKey.Data, 0, 0, ref color);
                    material.diffuse = new Vector3(color.X, color.Y, color.Z);
                }
                else if (mat->MProperties[i]->MKey == Assimp.MatkeyColorAmbient)
                {
                    Vector4 color = new Vector4();
                    assimp.GetMaterialColor(mat, mat->MProperties[i]->MKey.Data, 0, 0, ref color);
                    material.ambient = new Vector3(color.X, color.Y, color.Z);
                }
                else if (mat->MProperties[i]->MKey == Assimp.MatkeyColorSpecular)
                {
                    Vector4 color = new Vector4();
                    assimp.GetMaterialColor(mat, mat->MProperties[i]->MKey.Data, 0, 0, ref color);
                    material.specular = new Vector3(color.X, color.Y, color.Z);
                }
                else if (mat->MProperties[i]->MKey == Assimp.MatkeySpecularFactor)
                {
                    float value = 0f;
                    uint count = 0;
                    assimp.GetMaterialFloatArray(mat, mat->MProperties[i]->MKey.Data, 0, 0, ref value, ref count);
                    material.specularExp = value;
                }
                else if (mat->MProperties[i]->MKey == Assimp.MatkeyTextureBase)
                {
                    AssimpString texFile = new AssimpString();
                    assimp.GetMaterialString(mat, mat->MProperties[i]->MKey, (uint)TextureType.Diffuse, 0, ref texFile);
                    material.textureRef = texFile.AsString;
                    //just load from where the model is also located
                    if(!textureQueue.Contains(texFile.AsString))
                        textureQueue.Enqueue(texFile.AsString);
                }
            }
            
            return material;
        }
    }
}