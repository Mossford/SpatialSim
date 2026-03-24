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
        
        public static MeshData CreateSphereMesh(float size, int sphereSubDivide = 2)
        {
            MeshData meshData = new MeshData();
            float t = 0.52573111f;
            float y = 0.850650808f;

            Vertex[] vertexes = 
            {
                new Vertex(new Vector3(-t,  y,  0), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(t,  y,  0),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-t, -y,  0), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(t, -y,  0),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0, -t,  y),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0,  t,  y),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0, -t, -y),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0,  t, -y),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(y,  0, -t),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(y,  0,  t),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-y,  0, -t), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-y,  0,  t), new Vector3(0), new Vector2(0))
            };

            uint[] indices = 
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
                List<uint> newIndices = new List<uint>();
                List<Vertex> newVerts = new List<Vertex>();
                for (int g = 0; g < indices.Length; g += 3)
                {
                    //Get the required vertexes
                    uint ia = indices[g]; 
                    uint ib = indices[g + 1];
                    uint ic = indices[g + 2]; 
                    Vertex aTri = vertexes[ia];
                    Vertex bTri = vertexes[ib];
                    Vertex cTri = vertexes[ic];

                    //Create New Points
                    Vector3 ab = Vector3.Normalize((aTri.position + bTri.position) * 0.5f);
                    Vector3 bc = Vector3.Normalize((bTri.position + cTri.position) * 0.5f);
                    Vector3 ca = Vector3.Normalize((cTri.position + aTri.position) * 0.5f);

                    //Create Normals
                    Vector3 u = bc - ab;
                    Vector3 v = ca - ab;
                    Vector3 normal = Vector3.Normalize(Vector3.Cross(u,v));

                    //Add the new vertexes
                    ia = (uint)newVerts.Count;
                    newVerts.Add(aTri);
                    ib = (uint)newVerts.Count;
                    newVerts.Add(bTri);
                    ic = (uint)newVerts.Count;
                    newVerts.Add(cTri);
                    uint iab = (uint)newVerts.Count;
                    newVerts.Add(new Vertex(ab, normal, Vector2.Zero));
                    uint ibc = (uint)newVerts.Count; 
                    newVerts.Add(new Vertex(bc, normal, Vector2.Zero));
                    uint ica = (uint)newVerts.Count; 
                    newVerts.Add(new Vertex(ca, normal, Vector2.Zero));
                    newIndices.Add(ia); newIndices.Add(iab); newIndices.Add(ica);
                    newIndices.Add(ib); newIndices.Add(ibc); newIndices.Add(iab);
                    newIndices.Add(ic); newIndices.Add(ica); newIndices.Add(ibc);
                    newIndices.Add(iab); newIndices.Add(ibc); newIndices.Add(ica);
                }
                indices = newIndices.ToArray();
                vertexes = newVerts.ToArray();
            }

            for (int g = 0; g < vertexes.Length; g++)
            {
                Vector3 normal = Vector3.Zero;
                for (int i = 0; i < indices.Length; i += 3)
                {
                    uint a, b, c;
                    a = indices[i];
                    b = indices[i + 1];
                    c = indices[i + 2];
                    if (vertexes[g].position == vertexes[a].position)
                    {
                        Vector3 u = vertexes[b].position - vertexes[a].position;
                        Vector3 v = vertexes[c].position - vertexes[a].position;
                        Vector3 tmpnormal = Vector3.Normalize(Vector3.Cross(u, v));
                        normal += tmpnormal;
                    }

                    if (vertexes[g].position == vertexes[b].position)
                    {
                        Vector3 u = vertexes[c].position - vertexes[b].position;
                        Vector3 v = vertexes[a].position - vertexes[b].position;
                        Vector3 tmpnormal = Vector3.Normalize(Vector3.Cross(u, v));
                        normal += tmpnormal;
                    }

                    if (vertexes[g].position == vertexes[c].position)
                    {
                        Vector3 u = vertexes[a].position - vertexes[c].position;
                        Vector3 v = vertexes[b].position - vertexes[c].position;
                        Vector3 tmpnormal = Vector3.Normalize(Vector3.Cross(u, v));
                        normal += tmpnormal;
                    }
                }
                vertexes[g].normal = Vector3.Normalize(normal);
            }
            
            meshData.vertexData.vertices = new Vector3[vertexes.Length];
            meshData.vertexData.normals = new Vector3[vertexes.Length];
            meshData.vertexData.uvs = new Vector2[vertexes.Length];
            meshData.indices = new int[indices.Length];
            for (int i = 0; i < vertexes.Length; i++)
            {
                meshData.vertexData.vertices[i] = vertexes[i].position;
                meshData.vertexData.normals[i] = vertexes[i].normal;
                meshData.vertexData.uvs[i] = new Vector2(
                    0.5f - MathF.Atan2(vertexes[i].position.Z, vertexes[i].position.X) / (2.0f * MathF.PI),
                    0.5f - MathF.Asin(vertexes[i].position.Y) / MathF.PI
                );
            }
            
            for (int i = 0; i < indices.Length; i++)
            {
                meshData.indices[i] = (int)indices[i];
            }
            
            return meshData;
        }
        
        public static MeshData Create2DQuad()
        {
            MeshData meshData = new MeshData();
            meshData.vertexData.vertices = new[]
            {
                new Vector3(-1.0f, -1.0f, 0.0f),
                new Vector3(1.0f, -1.0f, 0.0f),
                new Vector3(-1.0f, 1.0f, 0.0f),
                new Vector3(1.0f, -1.0f, 0.0f),
                new Vector3(1.0f, 1.0f, 0.0f), 
            };

            meshData.vertexData.normals = new[]
            {
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
            };

            meshData.vertexData.uvs = new[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1)
            };
            
            meshData.indices = new []
            {
                0, 1, 2, 3, 4, 2
            };
           
            return meshData;
        }
    }
}