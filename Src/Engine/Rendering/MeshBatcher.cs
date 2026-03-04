using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    record MeshOffset(int offset, int offsetByte);

    public class MeshBatcher : IDisposable
    {
        List<MeshOffset> meshOffsets;
        MeshData meshData;
        int meshCount;
        Matrix4x4[] modelMats;

        public MeshBatcher()
        {
            meshOffsets = new List<MeshOffset>();
            meshData = new MeshData();
            modelMats = new Matrix4x4[0];
        }

        public void CreateBatch(in List<MeshRenderer> meshes, int materialId, int countBE, int countTO)
        {
            modelMats = new Matrix4x4[countTO - countBE];
            
            int vertexSize = 0;
            int indiceSize = 0;
            for (int i = countBE; i < countTO; i++)
            {
                //sort by specified material
                if(meshes[i].mesh.id == -1 || meshes[i].material.id == -1 || ((Material)EcsManager.GetComponent(meshes[i].material)).materialId != materialId)
                   continue;

                Mesh mesh = ((Mesh)EcsManager.GetComponent(meshes[i].mesh));
                vertexSize += mesh.meshData.vertexData.vertices.Length;
                indiceSize += mesh.meshData.indices.Length;
                meshCount++;
            }
            
            meshData.vertexData.vertices = new Vector3[vertexSize];
            meshData.indices = new int[indiceSize];
            
            int countV = 0;
            int countI = 0;
            int count = 0;
            for (int i = countBE; i < countTO; i++)
            {
                Mesh mesh = ((Mesh)EcsManager.GetComponent(meshes[i].mesh));
                modelMats[count] = mesh.modelMat;
                
                for (int j = 0; j < mesh.meshData.vertexData.vertices.Length; j++)
                {
                    meshData.vertexData.vertices[countV] = mesh.meshData.vertexData.vertices[j];
                    meshData.vertexData.normals[countV] = mesh.meshData.vertexData.normals[j];
                    meshData.vertexData.uvs[countV] = mesh.meshData.vertexData.uvs[j];
                    countV++;
                }
                
                for (int j = 0; j < mesh.meshData.indices.Length; j++)
                {
                    meshData.indices[countI] = mesh.meshData.indices[j];
                    countI++;
                }
                
                count++;
            }

            /*modelMatrixes = new BufferObject<Matrix4x4>(modelMats.ToArray(), Settings.RendererSettings.ModelMatrixBuffer, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);

            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vertex* buf = meshData.ToArray())
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexSize * sizeof(Vertex)), buf, BufferUsageARB.StreamDraw);
            fixed (uint* buf = indices.ToArray())
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indiceSize * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)0);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(3 * sizeof(float)));
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(6 * sizeof(float)));
            gl.BindVertexArray(0);*/
        }

        public void UpdateDrawSet(in List<MeshRenderer> meshes, int materialId, int countBE, int countTO)
        {
            if (countTO - countBE == meshCount)
                return;

            modelMats = new Matrix4x4[countTO - countBE];
            
            int vertexSize = 0;
            int indiceSize = 0;
            for (int i = countBE; i < countTO; i++)
            {
                //sort by specified material
                if(meshes[i].mesh.id == -1 || meshes[i].material.id == -1 || ((Material)EcsManager.GetComponent(meshes[i].material)).materialId != materialId)
                    continue;

                Mesh mesh = ((Mesh)EcsManager.GetComponent(meshes[i].mesh));
                vertexSize += mesh.meshData.vertexData.vertices.Length;
                indiceSize += mesh.meshData.indices.Length;
                meshCount++;
            }
            
            meshData.vertexData.vertices = new Vector3[vertexSize];
            meshData.indices = new int[indiceSize];
            
            int countV = 0;
            int countI = 0;
            int count = 0;
            for (int i = countBE; i < countTO; i++)
            {
                if(meshes[i].mesh.id == -1 || meshes[i].material.id == -1 || ((Material)EcsManager.GetComponent(meshes[i].material)).materialId != materialId)
                    continue;
                
                Mesh mesh = ((Mesh)EcsManager.GetComponent(meshes[i].mesh));
                modelMats[count] = mesh.modelMat;
                
                for (int j = 0; j < mesh.meshData.vertexData.vertices.Length; j++)
                {
                    meshData.vertexData.vertices[countV] = mesh.meshData.vertexData.vertices[j];
                    meshData.vertexData.normals[countV] = mesh.meshData.vertexData.normals[j];
                    meshData.vertexData.uvs[countV] = mesh.meshData.vertexData.uvs[j];
                    countV++;
                }
                
                for (int j = 0; j < mesh.meshData.indices.Length; j++)
                {
                    meshData.indices[countI] = mesh.meshData.indices[j];
                    countI++;
                }
                
                count++;
            }

            /*modelMatrixes.Realloc(modelMats.ToArray());

            gl.BindVertexArray(vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vertex* buf = meshData.ToArray())
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(meshData.Length * sizeof(Vertex)), buf, BufferUsageARB.StreamDraw);
            fixed (uint* buf = indices.ToArray())
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            gl.BindVertexArray(0);*/
        }

        public void UpdateModelBuffer(in List<MeshRenderer> meshes, int materialId, int countBE, int countTO)
        {
            if (countTO - countBE != modelMats.Length)
            {
                modelMats = new Matrix4x4[countTO - countBE];
            }

            int count = 0;
            for (int i = countBE; i < countTO; i++)
            {
                if(meshes[i].mesh.id == -1 || meshes[i].material.id == -1 || ((Material)EcsManager.GetComponent(meshes[i].material)).materialId != materialId)
                    continue;
                
                modelMats[count] = ((Mesh)EcsManager.GetComponent(meshes[i].mesh)).modelMat;
                count++;
            }
            
            //modelMatrixes.Realloc(modelMats.ToArray());
        }

        public void Dispose()
        {
            //gl.DeleteVertexArray(vao);
            //gl.DeleteBuffer(vbo);
            //gl.DeleteBuffer(ebo);
            GC.SuppressFinalize(this);
        }

        int GetOffsetIndex(int countBE, int count, int index, in List<MeshRenderer> meshes, int materialId)
        {
            int offset = 0;
            int offsetByte = 0;
            for (int i = countBE; i < index; i++)
            {
                if(meshes[i].mesh.id == -1 || meshes[i].material.id == -1 || ((Material)EcsManager.GetComponent(meshes[i].material)).materialId != materialId)
                    continue;
                
                Mesh mesh = ((Mesh)EcsManager.GetComponent(meshes[i].mesh));
                offset += mesh.meshData.vertexData.vertices.Length;
                offsetByte += mesh.meshData.indices.Length;
            }
            
            meshOffsets.Add(new MeshOffset(offset, offsetByte * sizeof(int)));
            return meshOffsets.Count - 1;
        }

        /// <summary>
        /// Draws the RenderSet using MultiDraw, warning that on Mesa drivers gl_DrawID will have some issues
        /// </summary>
        /// <param name="objs">The objects to draw</param>
        /// <param name="countBE">The index to start from</param>
        /// <param name="countTO">The index to end to</param>
        /// <param name="shader">The shader to use</param>
        /// <param name="view">View matrix</param>
        /// <param name="proj">Projection matrix</param>
        /// <param name="camPos">Camera position</param>
        /*public unsafe void DrawSet(in List<SpatialObject> objs, int countBE, int countTO, ref Shader shader, in Matrix4x4 view, in Matrix4x4 proj, in Vector3 camPos)
        {
            //return if scene is empty as will crash because obarray and others are empty since countBE - countTO is 0
            if (objs.Count == 0)
                return;

            gl.BindVertexArray(vao);
            modelMatrixes.Bind();
            shader.setMat4("view", view);
            shader.setMat4("projection", proj);
            shader.setVec3("viewPos", camPos);
            shader.setBool("meshDraw", false);
            int count = 0;
            uint[] indCounts = new uint[countTO - countBE];
            int[] offsetBytes = new int[countTO - countBE];
            int[] offsets = new int[countTO - countBE];
            for (int i = countBE; i < countTO; i++)
            {
                if(!objs[i].enabled)
                    continue;
                
                int index = count;
                if (count >= meshOffsets.Count)
                    index = GetOffsetIndex(countBE, count, i, objs);
                indCounts[count] = (uint)objs[i].mesh.indices.Length;
                offsetBytes[count] = meshOffsets[index].offsetByte;
                offsets[count] = meshOffsets[index].offset;
                count++;
            }

            //indices paramater needed a array of void* and this allows for it as it creates pointers to each value and creates a pointer array with it
            int*[] obArray = new int*[countTO - countBE];

            for (int i = 0; i < offsetBytes.Length; i++)
            {
                obArray[i] = (int*)offsetBytes[i];
            }

            fixed (void* ptr = &obArray[0])
                gl.MultiDrawElementsBaseVertex(GLEnum.Triangles, indCounts, GLEnum.UnsignedInt, (void**)ptr, offsets);
            drawCallCount++;
            gl.BindVertexArray(0);
        }*/

        /// <summary>
        /// Draws the RenderSet, but uses the shader from the SO_Object
        /// </summary>
        /// <param name="objs">The objects to draw</param>
        /// <param name="countBE">The index to start from</param>
        /// <param name="countTO">The index to end to</param>
        /*public unsafe void DrawSetObject(in List<SpatialObject> objs, int countBE, int countTO)
        {
            gl.BindVertexArray(vao);
            modelMatrixes.Bind();
            int count = 0;
            for (int i = countBE; i < countTO; i++)
            {
                //early from the current object
                if (objs[i].shader is null)
                    continue;
                
                if(!objs[i].enabled)
                    continue;

                int index = count;
                if (count >= meshOffsets.Count)
                    index = GetOffsetIndex(countBE, count, i, objs);
                //Because of opengls stupid documentation this draw call is suppose to take in the offset in indices by bytes then take in the offset in vertices instead of the offset in indices
                // and its not the indices that are stored it wants the offsets as the indcies are already in a buffer which is what draw elements is using
                //
                //    indices
                //        Specifies a pointer to the location where the indices are stored.
                //    basevertex
                //        Specifies a constant that should be added to each element of indices when chosing elements from the enabled vertex arrays. 
                //
                //This naming is so fucking bad and has caused me multiple hours in trying to find what the hell the problem is

                //use the object shader
                gl.UseProgram(objs[i].shader.shader);
                if(objs[i].texture != null)
                    objs[i].texture.Bind();
                else
                    Renderer.defaultTexture.Bind();
                
                gl.DrawElementsBaseVertex(GLEnum.Triangles, (uint)objs[i].mesh.indices.Length, GLEnum.UnsignedInt, (void*)meshOffsets[index].offsetByte, meshOffsets[index].offset);
                drawCallCount++;
                count++;
            }
            gl.BindVertexArray(0);
        }*/
        
        /// <summary>
        /// Draws the RenderSet, using a normal draw call for each object
        /// slower, but should be fine on any driver
        /// </summary>
        /// <param name="objs">The objects to draw</param>
        /// <param name="countBE">The index to start from</param>
        /// <param name="countTO">The index to end to</param>
        /// <param name="shader">The shader to use</param>
        /// <param name="view">View matrix</param>
        /// <param name="proj">Projection matrix</param>
        /// <param name="camPos">Camera position</param>
        /*public unsafe void DrawSetObject(in List<SpatialObject> objs, int countBE, int countTO, ref Shader shader, in Matrix4x4 view, in Matrix4x4 proj, in Vector3 camPos)
        {
            gl.BindVertexArray(vao);
            modelMatrixes.Bind();
            int count = 0;
            for (int i = countBE; i < countTO; i++)
            {
                if(!objs[i].enabled)
                    continue;
                
                int index = count;
                if (count >= meshOffsets.Count)
                    index = GetOffsetIndex(countBE, count, i, objs);
                //Because of opengls stupid documentation this draw call is suppose to take in the offset in indices by bytes then take in the offset in vertices instead of the offset in indices
                // and its not the indices that are stored it wants the offsets as the indcies are already in a buffer which is what draw elements is using
                //
                //    indices
                //        Specifies a pointer to the location where the indices are stored.
                //    basevertex
                //        Specifies a constant that should be added to each element of indices when chosing elements from the enabled vertex arrays. 
                //
                //This naming is so fucking bad and has caused me multiple hours in trying to find what the hell the problem is

                //use the object shader
                shader.setMat4("view", view);
                shader.setMat4("projection", proj);
                shader.setVec3("viewPos", camPos);
                shader.setBool("meshDraw", true);
                shader.setInt("modelIndex", count);
                if(objs[i].texture != null)
                    objs[i].texture.Bind();
                else
                    Renderer.defaultTexture.Bind();

                gl.DrawElementsBaseVertex(GLEnum.Triangles, (uint)objs[i].mesh.indices.Length, GLEnum.UnsignedInt, (void*)meshOffsets[index].offsetByte, meshOffsets[index].offset);
                drawCallCount++;
                count++;
            }
            gl.BindVertexArray(0);
        }*/

        /// <summary>
        /// Draws but does not bind any shader inside the method
        /// </summary>
        /// <param name="objs">The objects to draw</param>
        /// <param name="countBE">The index to start from</param>
        /// <param name="countTO">The index to end to</param>
        /*public unsafe void DrawSetNoAssign(in List<SpatialObject> objs, int countBE, int countTO)
        {
            //return if scene is empty as will crash because obarray and others are empty since countBE - countTO is 0
            if (objs.Count == 0)
                return;

            gl.BindVertexArray(vao);
            modelMatrixes.Bind();
            int count = 0;
            uint[] indCounts = new uint[countTO - countBE];
            int[] offsetBytes = new int[countTO - countBE];
            int[] offsets = new int[countTO - countBE];
            for (int i = countBE; i < countTO; i++)
            {
                if(!objs[i].enabled)
                    continue;
                
                int index = count;
                if (count >= meshOffsets.Count)
                    index = GetOffsetIndex(countBE, count, i, objs);
                indCounts[count] = (uint)objs[i].mesh.indices.Length;
                offsetBytes[count] = meshOffsets[index].offsetByte;
                offsets[count] = meshOffsets[index].offset;
                count++;
            }

            //indices paramater needed a array of void* and this allows for it as it creates pointers to each value and creates a pointer array with it
            int*[] obArray = new int*[countTO - countBE];

            for (int i = 0; i < offsetBytes.Length; i++)
            {
                obArray[i] = (int*)offsetBytes[i];
            }

            fixed (void* ptr = &obArray[0])
                gl.MultiDrawElementsBaseVertex(GLEnum.Triangles, indCounts, GLEnum.UnsignedInt, (void**)ptr, offsets);
            drawCallCount++;
            gl.BindVertexArray(0);
        }*/
        
    }
}