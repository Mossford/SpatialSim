using System.Numerics;
using Silk.NET.SDL;

namespace SpatialSim.Engine.Rendering
{
    public static class MeshGeneration
    {
        public static MeshData Create2DTriangle()
        {
            MeshData meshData = new MeshData();
            meshData.vertexData.vertices = new[]
            {
                new Vector3(-1.0f, -1.0f, 0.0f),
                new Vector3(1.0f, -1.0f, 0.0f),
                new Vector3(-1.0f, 1.0f, 0.0f),
            };

            meshData.vertexData.normals = new[]
            {
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
            };

            meshData.vertexData.uvs = new[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1)
            };
            
           meshData.indices = new []
            {
                0, 1, 2
            };
           
            return meshData;
        }
        
        public static MeshData CreateSpikerMesh(float size, int sphereSubDivide = 2)
        {
            MeshData meshData = new MeshData();
            meshData.vertexData.vertices = new[]
            {
                new Vector3(-1.0f, 1.0f, 0),
                new Vector3(1.0f, 1.0f, 0),
                new Vector3(-1.0f, -1.0f, 0),
                new Vector3(1.0f, -1.0f, 0),
                new Vector3(0, -1.0f, 1.0f),
                new Vector3(0, 1.0f, 1.0f),
                new Vector3(0, -1.0f, -1.0f),
                new Vector3(0, 1.0f, -1.0f),
                new Vector3(1.0f, 0, -1.0f),
                new Vector3(1.0f, 0, 1.0f),
                new Vector3(-1.0f, 0, -1.0f),
                new Vector3(-1.0f, 0, 1.0f),
            };

            meshData.vertexData.normals = new[]
            {
                new Vector3(0),
                new Vector3(0),
                new Vector3(0),
                new Vector3(0),
                new Vector3(0),
                new Vector3(0),
                new Vector3(0),
                new Vector3(0),
                new Vector3(0),
                new Vector3(0),
                new Vector3(0),
                new Vector3(0),
            };

            meshData.vertexData.uvs = new[]
            {
                new Vector2(0),
                new Vector2(0),
                new Vector2(0),
                new Vector2(0),
                new Vector2(0),
                new Vector2(0),
                new Vector2(0),
                new Vector2(0),
                new Vector2(0),
                new Vector2(0),
                new Vector2(0),
                new Vector2(0),
            };

            meshData.indices = new[]
            {
                0, 11, 5, 
                0, 5, 1,
                0, 1, 7,
                0, 7, 10,
                0, 10, 11,
                
                1, 5, 9,
                5, 11, 4,
                11, 10, 2,
                10, 7, 6,
                7, 1, 8,
                
                3, 9, 4,
                3, 4, 2,
                3, 2, 6,
                3, 6, 8,
                3, 8, 9,
                
                4, 9, 5,
                2, 4, 11,
                6, 2, 10,
                8, 6, 7,
                9, 8, 1
            };

            for (int i = 0; i < sphereSubDivide; i++)
            {
                List<int> newIndices = new List<int>();
                List<Vector3> newVerts = new List<Vector3>();
                List<Vector3> newNormals = new List<Vector3>();
                List<Vector2> newUvs = new List<Vector2>();
                for (int g = 0; g < meshData.indices.Length; g += 3)
                {
                    //Get the required vertexes
                    int ia = meshData.indices[g]; 
                    int ib = meshData.indices[g + 1];
                    int ic = meshData.indices[g + 2]; 
                    Vector3 aTri = meshData.vertexData.vertices[ia];
                    Vector3 bTri = meshData.vertexData.vertices[ib];
                    Vector3 cTri = meshData.vertexData.vertices[ic];

                    //Create New Points
                    Vector3 ab = Vector3.Normalize((aTri + bTri) * 0.5f) * size;
                    Vector3 bc = Vector3.Normalize((bTri + cTri) * 0.5f) * size;
                    Vector3 ca = Vector3.Normalize((cTri + aTri) * 0.5f) * size;

                    //Create Normals
                    Vector3 u = bc - ab;
                    Vector3 v = ca - ab;
                    Vector3 normal = Vector3.Normalize(Vector3.Cross(u,v));

                    //Add the new vertexes
                    ia = newVerts.Count;
                    newVerts.Add(aTri);
                    ib = newVerts.Count;
                    newVerts.Add(bTri);
                    ic = newVerts.Count;
                    newVerts.Add(cTri);
                    int iab = newVerts.Count;
                    newVerts.Add(ab);
                    newNormals.Add(normal);
                    newUvs.Add(new Vector2());
                    int ibc = newVerts.Count; 
                    newVerts.Add(bc);
                    newNormals.Add(normal);
                    newUvs.Add(new Vector2());
                    int ica = newVerts.Count; 
                    newVerts.Add(ca);
                    newNormals.Add(normal);
                    newUvs.Add(new Vector2());
                    newIndices.Add(ia); newIndices.Add(iab); newIndices.Add(ica);
                    newIndices.Add(ib); newIndices.Add(ibc); newIndices.Add(iab);
                    newIndices.Add(ic); newIndices.Add(ica); newIndices.Add(ibc);
                    newIndices.Add(iab); newIndices.Add(ibc); newIndices.Add(ica);
                }
                meshData.indices = newIndices.ToArray();
                meshData.vertexData.vertices = newVerts.ToArray();
                meshData.vertexData.normals = newNormals.ToArray();
                meshData.vertexData.uvs = newUvs.ToArray();
            }
            
            return meshData;
        }
    }
}