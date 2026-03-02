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
        
        const float ResizeDelay = 0.05f;
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

            for (int i = 0; i < 10; i++)
            {
                meshTest = EcsManager.AddEntity();
                meshTest.AddComponent(new MeshRenderer(
                    meshTest.AddComponent(new Mesh(MeshGeneration.CreateSpikerMesh(1, 0))),
                    meshTest.AddComponent(new Material())));
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
                Debug.Error("Failed to acquire swap chain image");
                throw new Exception("Failed to acquire swap chain image");
            }

            if (VkSwapChain.imagesInFlight[imageIndex].Handle != 0)
            {
                vk!.WaitForFences(VkDevices.device, 1, in VkSwapChain.imagesInFlight[imageIndex], true, ulong.MaxValue);
            }
            VkSwapChain.imagesInFlight[imageIndex] = VkSwapChain.inFlightFences[VkSwapChain.currentFrame];
            
            //render
            
            CommandBuffer vkcommandBuffer = ((VkCommandBuffer)VkSwapChain.commandBuffers[imageIndex].commandBuffer!).commandBuffer;
            VkSwapChain.commandBuffers[imageIndex].BeginCommandBuffer();
            
            VkSwapChain.commandBuffers[imageIndex].BeginRenderPass((int)imageIndex);
            VkSwapChain.commandBuffers[imageIndex].BindPipeLine(defaultPipeline);
            Shader vertexShader = ShaderManager.RetrieveShader(new ShaderSettings(ShaderType.Vertex, "base.vert"));
            vertexShader.AddMat4(Matrix4x4.Identity * Matrix4x4.CreateTranslation(new Vector3(0)));
            vertexShader.AddMat4(Matrix4x4.CreateLookAt(new Vector3(MathF.Sin((float)AppState.GetSeconds()), 4, MathF.Cos((float)AppState.GetSeconds())), new Vector3(0, 0, 0), new Vector3(0, 1, 0)));
            vertexShader.AddMat4(Matrix4x4.CreatePerspectiveFieldOfView(45 * MathF.PI / 180.0f, Window.size.X / Window.size.Y, 0.01f, 10.0f));
            defaultPipeline.UpdateUniforms(vertexShader, (int)imageIndex);
            VkSwapChain.commandBuffers[imageIndex].BindUniforms(defaultPipeline, (int)imageIndex);
            EcsManager.Render(VkSwapChain.commandBuffers[imageIndex]);
            VkSwapChain.commandBuffers[imageIndex].EndRenderPass();
            
            imGuiController.Render(vkcommandBuffer, VkSwapChain.swapChainFramebuffers[imageIndex], VkSwapChain.swapChainExtent);
            
            VkSwapChain.commandBuffers[imageIndex].EndCommandBuffer();
            
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

            if (vk.QueueSubmit(VkDevices.graphicsQueue, 1, in submitInfo, VkSwapChain.inFlightFences[VkSwapChain.currentFrame]) != Result.Success)
            {
                Debug.Error("Failed to submit draw command buffer");
                throw new Exception("Failed to submit draw command buffer");
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
                Debug.Error("Failed to present swap chain image");
                throw new Exception("Failed to present swap chain image");
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