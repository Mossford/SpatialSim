using System.Numerics;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using SpatialSim.Engine.Rendering;
using SpatialSim.Engine.Rendering.ImGui;
using SpatialSim.Engine.Rendering.Vulkan;
using CommandBuffer = Silk.NET.Vulkan.CommandBuffer;
using Pipeline = SpatialSim.Engine.Rendering.Pipeline;
using RenderPass = SpatialSim.Engine.Rendering.RenderPass;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using VkRenderPass = SpatialSim.Engine.Rendering.Vulkan.VkRenderPass;
using VkCommandBuffer = SpatialSim.Engine.Rendering.Vulkan.VkCommandBuffer;

namespace SpatialSim.Engine.Core.Vulkan
{
    public class VkContext : AppContext
    {
        public Vk vk;
        public Instance instance;
        
        public GraphicsAPI graphicsApi { get; set; }
        public IDeviceFactory DeviceFactory { get; set; }
        public Pipeline defaultPipeline { get; set; }
        public RenderPass renderPass { get; set; }
        public VkImGuiController imGuiController;

        public Entity meshTest;
        public Entity camera;
        
        const float ResizeDelay = 0.1f;
        int currentSwapChainRecreations;
        float swapChainRecreationCounter;
        bool currentlyResizing;

        public VkContext(GraphicsAPI graphicsAPI)
        {
            this.graphicsApi = graphicsAPI;
        }

        public void Init()
        {
            currentSwapChainRecreations = 0;
            swapChainRecreationCounter = 0f;
            currentlyResizing = false;
            
            DeviceFactory = new VkDeviceFactory();
            
            VkCreation.CreateInstance();
            VkValidationLayers.SetupDebugMessenger();
            VkSurface.CreateSurface();
            VkDevices.PickPhysicalDevice();
            VkDevices.CreateLogicalDevice();
            VkSwapChain.CreateSwapChain();
            VkSwapChain.CreateImageViews();
            
            renderPass = new VkRenderPass();
            renderPass.Create();
            
            defaultPipeline = new Pipeline();
            defaultPipeline.Create(
                ShaderManager.RetrieveShader(new ShaderSettings(ShaderType.Vertex, "base.vert")), 
                ShaderManager.RetrieveShader(new ShaderSettings(ShaderType.Fragment, "base.frag")));
            
            VkSwapChain.CreateFramebuffers();
            VkSwapChain.CreateSyncObjects();
            VkSwapChain.CreateSwapChainCommandBuffers();
            
            VkCreation.CreateImGui();

            camera = EcsManager.AddEntity();
            camera.AddComponent(new Camera(
                camera.AddComponent(new Transform(new Vector3(0f), Quaternion.Identity, new Vector3(1.0f))), 60));

            for (int i = -10; i <= 10; i++)
            {
                for (int j = -10; j <= 10; j++)
                {
                    meshTest = EcsManager.AddEntity();
                
                    EcsComponentRef mesh = meshTest.AddComponent(
                        new Mesh(
                            MeshGeneration.CreateSpikerMesh(1, 0), 
                            meshTest.AddComponent(new Transform(new Vector3(i * 0.5f, 0, j * 0.5f), Quaternion.Identity, new Vector3(0.2f)))));
                
                    meshTest.AddComponent(new MeshRenderer(mesh, meshTest.AddComponent(new Material())));
                }
            }
            
            Debug.LogInfo("Successful vulkan context creation");
        }

        public unsafe void Update(float delta)
        {
            imGuiController.Update(delta);
            
            if (currentlyResizing)
            {
                swapChainRecreationCounter += delta;
                
                if (swapChainRecreationCounter >= ResizeDelay)
                {
                    VkSwapChain.RecreateSwapChain();
                    Ticks.swapchainRecreations++;

                    currentlyResizing = false;
                    swapChainRecreationCounter = 0f;
                    currentSwapChainRecreations = 0;
                }
            }
        }

        public unsafe void Render()
        {
            //wait for the current frame to complete
            vk.WaitForFences(VkDevices.device, 1, in VkSwapChain.inFlightFences[VkSwapChain.currentFrame], true, ulong.MaxValue);
            
            //grab an image to render to at the image index
            uint imageIndex = 0;
            Result result = VkSwapChain.khrSwapChain!.AcquireNextImage(VkDevices.device, VkSwapChain.swapChain, ulong.MaxValue, VkSwapChain.imageAvailableSemaphores[VkSwapChain.currentFrame], default, ref imageIndex);

            if (result == Result.ErrorOutOfDateKhr)
            {
                VkSwapChain.RecreateSwapChain();
                return;
            }
            else if (result != Result.Success && result != Result.SuboptimalKhr)
            {
                Debug.Error($"Failed to acquire swap chain image {result}");
                throw new Exception($"Failed to acquire swap chain image {result}");
            }

            if (VkSwapChain.imagesInFlight[imageIndex].Handle != 0)
            {
                vk!.WaitForFences(VkDevices.device, 1, in VkSwapChain.imagesInFlight[imageIndex], true, ulong.MaxValue);
            }
            VkSwapChain.imagesInFlight[imageIndex] = VkSwapChain.inFlightFences[VkSwapChain.currentFrame];
            
            //render
            
            CommandBuffer vkcommandBuffer = ((VkCommandBuffer)VkSwapChain.commandBuffers[imageIndex].commandBuffer!).commandBuffer;
            VkSwapChain.commandBuffers[imageIndex].BeginCommandBuffer();
            
            ((Camera)camera.GetFirstComponentOfType(EcsComponentType.Camera)).GenerateTransforms();
            
            EcsManager.Render(VkSwapChain.commandBuffers[imageIndex], (int)imageIndex);
            
            imGuiController.Render(vkcommandBuffer, VkSwapChain.swapChainFramebuffers[imageIndex], VkSwapChain.swapChainExtent);

            VkSwapChain.commandBuffers[imageIndex].EndCommandBuffer();
            VkSwapChain.commandBuffers[imageIndex].ResetPipeLine(defaultPipeline);
            
            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
            };

            Semaphore* waitSemaphores = stackalloc[] { VkSwapChain.imageAvailableSemaphores[VkSwapChain.currentFrame] };
            PipelineStageFlags* waitStages = stackalloc[] { PipelineStageFlags.ColorAttachmentOutputBit };
            
            submitInfo = submitInfo with
            {
                WaitSemaphoreCount = 1,
                PWaitSemaphores = waitSemaphores,
                PWaitDstStageMask = waitStages,

                CommandBufferCount = 1,
                PCommandBuffers = &vkcommandBuffer
            };

            Semaphore* signalSemaphores = stackalloc[] { VkSwapChain.renderFinishedSemaphores[imageIndex] };
            submitInfo = submitInfo with
            {
                SignalSemaphoreCount = 1,
                PSignalSemaphores = signalSemaphores,
            };

            vk.ResetFences(VkDevices.device, 1, in VkSwapChain.inFlightFences[VkSwapChain.currentFrame]);
            result = vk.QueueSubmit(VkDevices.graphicsQueue, 1, in submitInfo,
                VkSwapChain.inFlightFences[VkSwapChain.currentFrame]);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to submit draw command buffer {result}");
                throw new Exception($"Failed to submit draw command buffer {result}");
            }

            SwapchainKHR* swapChains = stackalloc[] { VkSwapChain.swapChain };
            PresentInfoKHR presentInfo = new()
            {
                SType = StructureType.PresentInfoKhr,

                WaitSemaphoreCount = 1,
                PWaitSemaphores = signalSemaphores,

                SwapchainCount = 1,
                PSwapchains = swapChains,

                PImageIndices = &imageIndex
            };

            result = VkSwapChain.khrSwapChain.QueuePresent(VkSurface.presentQueue, in presentInfo);

            if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr)
            {
                VkSwapChain.RecreateSwapChain();
            }
            else if (result != Result.Success)
            {
                Debug.Error($"Failed to present swap chain image {result}");
                throw new Exception($"Failed to present swap chain image {result}");
            }

            VkSwapChain.currentFrame = (VkSwapChain.currentFrame + 1) % VkSwapChain.MAX_FRAMES_IN_FLIGHT;
        }

        public void WindowResize()
        {
            currentlyResizing = true;
            swapChainRecreationCounter = 0f;
        }

        public unsafe void CleanObjects()
        {
            vk.DeviceWaitIdle(VkDevices.device);
         
            imGuiController.Dispose();
            
            VkSwapChain.CleanSwapChain();
            
            Debug.LogInfo("Cleaned vulkan windowing resources");
        }

        /// <summary>
        /// Should be called after all objects have been cleaned
        /// </summary>
        public unsafe void CleanContext()
        {
            VkDescriptor.CleanPool();
            
            vk.DestroyDevice(VkDevices.device, null);

            if (AppState.EnableVkValidationLayers)
            {
                //DestroyDebugUtilsMessenger equivilant to method DestroyDebugUtilsMessengerEXT from original tutorial.
                VkValidationLayers.debugUtils.DestroyDebugUtilsMessenger(instance, VkValidationLayers.debugMessenger, null);
            }
            
            VkSurface.khrSurface.DestroySurface(instance, VkSurface.surface, null);
            
            vk.DestroyInstance(instance, null);
            vk.Dispose();
            
            Debug.LogInfo("Cleaned all vulkan instances");
        }

        T AppContext.GetContext<T>()
        {
            if (typeof(T) != typeof(VkContext))
            {
                Debug.Error($"Type {typeof(T).Name} does not match contained type {nameof(VkContext)}");
                throw new Exception($"Type {typeof(T)} does not match contained type {nameof(VkContext)}");
            }
            
            return (T)this;
        }
    }
}