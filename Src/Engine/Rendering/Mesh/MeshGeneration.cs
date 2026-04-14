using System.Numerics;

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
            
            meshData.vertexData.tangents = new[]
            {
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
            };
            
            meshData.vertexData.biTangents = new[]
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
            MeshData meshData = ModelLoader.LoadModelFile("cube.obj");
            Vertex[] vertexes = meshData.GetVertexes();
            
            for (int i = 0; i < sphereSubDivide; i++)
            {
                List<int> newIndices = new List<int>();
                List<Vertex> newVerts = new List<Vertex>();
                for (int g = 0; g < meshData.indices.Length; g += 3)
                {
                    //Get the required vertexes
                    int ia = meshData.indices[g]; 
                    int ib = meshData.indices[g + 1];
                    int ic = meshData.indices[g + 2]; 
                    Vertex aTri = vertexes[ia];
                    Vertex bTri = vertexes[ib];
                    Vertex cTri = vertexes[ic];
                    
                    aTri.position = Vector3.Normalize(aTri.position);
                    bTri.position = Vector3.Normalize(bTri.position);
                    cTri.position = Vector3.Normalize(cTri.position);

                    //Create New Points
                    Vector3 ab = Vector3.Normalize((aTri.position + bTri.position) * 0.5f);
                    Vector3 bc = Vector3.Normalize((bTri.position + cTri.position) * 0.5f);
                    Vector3 ca = Vector3.Normalize((cTri.position + aTri.position) * 0.5f);
                    
                    //calculate new uvs
                    Vector2 aUV = (aTri.uv + bTri.uv) * 0.5f;
                    
                    if (MathF.Abs(aTri.uv.X - bTri.uv.X) > 0.5f)
                    {
                        if (aTri.uv.X > bTri.uv.X)
                            aUV.X = ((aTri.uv.X - 1.0f + bTri.uv.X) * 0.5f + 1.0f);
                        else
                            aUV.X = ((aTri.uv.X + bTri.uv.X - 1.0f) * 0.5f + 1.0f);
                    }
                    
                    Vector2 bUV = (bTri.uv + cTri.uv) * 0.5f;
                    
                    if (MathF.Abs(bTri.uv.X - cTri.uv.X) > 0.5f)
                    {
                        if (bTri.uv.X > cTri.uv.X)
                            bUV.X = ((bTri.uv.X - 1.0f + cTri.uv.X) * 0.5f + 1.0f);
                        else
                            bUV.X = ((bTri.uv.X + cTri.uv.X - 1.0f) * 0.5f + 1.0f);
                    }
                    
                    Vector2 cUV = (aTri.uv + cTri.uv) * 0.5f;
                    
                    if (MathF.Abs(aTri.uv.X - cTri.uv.X) > 0.5f)
                    {
                        if (aTri.uv.X > cTri.uv.X)
                            cUV.X = ((aTri.uv.X - 1.0f + cTri.uv.X) * 0.5f + 1.0f);
                        else
                            cUV.X = ((aTri.uv.X + cTri.uv.X - 1.0f) * 0.5f + 1.0f);
                    }

                    //Add the new vertexes
                    ia = newVerts.Count;
                    newVerts.Add(aTri);
                    ib = newVerts.Count;
                    newVerts.Add(bTri);
                    ic = newVerts.Count;
                    newVerts.Add(cTri);
                    int iab = newVerts.Count;
                    newVerts.Add(new Vertex(ab, ab, Vector3.Zero, Vector3.Zero, aUV));
                    int ibc = newVerts.Count; 
                    newVerts.Add(new Vertex(bc, bc, Vector3.Zero, Vector3.Zero, bUV));
                    int ica = newVerts.Count; 
                    newVerts.Add(new Vertex(ca, ca, Vector3.Zero, Vector3.Zero, cUV));
                    newIndices.Add(ia); newIndices.Add(iab); newIndices.Add(ica);
                    newIndices.Add(ib); newIndices.Add(ibc); newIndices.Add(iab);
                    newIndices.Add(ic); newIndices.Add(ica); newIndices.Add(ibc);
                    newIndices.Add(iab); newIndices.Add(ibc); newIndices.Add(ica);
                }
                meshData.indices = newIndices.ToArray();
                vertexes = newVerts.ToArray();
            }
            
            Vector3[] tangents = new Vector3[vertexes.Length];
            Vector3[] bitangents = new Vector3[vertexes.Length];
            //fix uv seams and calculate tangent and bitangent
            for (int i = 0; i < meshData.indices.Length; i += 3)
            {
                int ia = meshData.indices[i]; 
                int ib = meshData.indices[i + 1];
                int ic = meshData.indices[i + 2];

                Vector2 aUV = vertexes[ia].uv;
                Vector2 bUV = vertexes[ib].uv;
                Vector2 cUV = vertexes[ic].uv;
                
                Vector3 pos0 = vertexes[ia].position;
                Vector3 pos1 = vertexes[ib].position;
                Vector3 pos2 = vertexes[ic].position;

                Vector3 edge1 = pos1 - pos0;
                Vector3 edge2 = pos2 - pos0;
                Vector2 deltaUV1 = bUV - aUV;
                Vector2 deltaUV2 = cUV - aUV;

                float f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

                Vector3 tangent = f * (deltaUV2.Y * edge1 - deltaUV1.Y * edge2);
                Vector3 bitangent = f * (-deltaUV2.X * edge1 + deltaUV1.X * edge2);

                tangents[ia] = tangent;
                tangents[ib] = tangent;
                tangents[ic] = tangent;

                bitangents[ia] = bitangent;
                bitangents[ib] = bitangent;
                bitangents[ic] = bitangent;
            }
            
            meshData.vertexData.vertices = new Vector3[vertexes.Length];
            meshData.vertexData.normals = new Vector3[vertexes.Length];
            meshData.vertexData.tangents = new Vector3[vertexes.Length];
            meshData.vertexData.biTangents = new Vector3[vertexes.Length];
            meshData.vertexData.uvs = new Vector2[vertexes.Length];
            for (int i = 0; i < vertexes.Length; i++)
            {
                meshData.vertexData.vertices[i] = vertexes[i].position;
                meshData.vertexData.normals[i] = Vector3.Normalize(vertexes[i].normal);
                meshData.vertexData.tangents[i] = Vector3.Normalize(tangents[i]);
                meshData.vertexData.biTangents[i] = Vector3.Normalize(bitangents[i]);
                meshData.vertexData.uvs[i] = vertexes[i].uv;
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
            
            meshData.vertexData.tangents = new[]
            {
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
            };
            
            meshData.vertexData.biTangents = new[]
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