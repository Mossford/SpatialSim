using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public static class VkAccelerationStructure
    {
        public static void Create()
        {
            CreateBLAS();
            CreateTLAS();
        }

        unsafe static void CreateBLAS()
        {
            //go through all the mesh renderers in the ecs manager mesh pool and create a blas for each one
            //where we grab the already stored mesh data on the gpu
            for (int i = 0; i < EcsManager.componentPools[EcsComponentType.MeshRenderer.GetId()].components.ValueCount; i++)
            {
                MeshRenderer mesh = (MeshRenderer)EcsManager.componentPools[EcsComponentType.MeshRenderer.GetId()].components.Get(i);
                AccelerationStructureGeometryTrianglesDataKHR trianglesData = new()
                {
                    SType = StructureType.AccelerationStructureGeometryTrianglesDataKhr,
                    VertexFormat = Format.R32G32B32Sfloat,
                    VertexData = new DeviceOrHostAddressConstKHR(((VkBuffer<Vertex>)mesh.vertexBuffer.buffer!).GetBufferDeviceAddress()),
                    VertexStride = (ulong)sizeof(Vertex),
                    MaxVertex = (uint)(mesh.vertexBuffer.size / (ulong)sizeof(Vertex)),
                    IndexType = IndexType.Uint32,
                    IndexData = new DeviceOrHostAddressConstKHR(((VkBuffer<int>)mesh.indexBuffer.buffer!).GetBufferDeviceAddress())
                };

                AccelerationStructureGeometryDataKHR geometryData = new(trianglesData);

                AccelerationStructureGeometryKHR blasGeometry = new()
                {
                    SType = StructureType.AccelerationStructureGeometryKhr,
                    GeometryType = GeometryTypeKHR.TrianglesKhr,
                    Geometry = geometryData,
                    Flags = GeometryFlagsKHR.OpaqueBitKhr
                };
                
                AccelerationStructureBuildGeometryInfoKHR blasBuildGeometryInfo = new()
                {
                    SType = StructureType.AccelerationStructureBuildGeometryInfoKhr,
                    Type = AccelerationStructureTypeKHR.BottomLevelKhr,
                    Mode = BuildAccelerationStructureModeKHR.BuildKhr,
                    GeometryCount = 1,
                    PGeometries = &blasGeometry,
                };

                uint triCount = (uint)(mesh.indexBuffer.size / (ulong)sizeof(Vertex)) / 3;
                
                AccelerationStructureBuildSizesInfoKHR blasBuildSizes =
                    VkDevices.accelerationStructure.GetAccelerationStructureBuildSizes(
                        VkDevices.device,
                        AccelerationStructureBuildTypeKHR.DeviceKhr,
                        &blasBuildGeometryInfo, ref triCount);
                
                /*Buffer<>
                
                AccelerationStructureCreateInfoKHR blasCreateInfo = new()
                {
                    SType = AccelerationStructureCreateInfoKHR,
                    Buffer = blasBuffers[i],
                    Offset = 0,
                    Size = blasBuildSizes.accelerationStructureSize,
                    Type = vk::AccelerationStructureTypeKHR::eBottomLevel,
                };

                blasHandles.emplace_back(device.createAccelerationStructureKHR(blasCreateInfo));
                
                blasBuildGeometryInfo.dstAccelerationStructure = blasHandles[i];*/
                
            }
        }

        static void CreateTLAS()
        {
            
        }
    }
}