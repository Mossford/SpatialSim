using System.Numerics;
using ImGuiNET;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using SpatialSim.Engine.Rendering;
using SpatialSim.Engine.Rendering.ImGui;
using SpatialSim.Engine.Rendering.Vulkan;
using CommandBuffer = SpatialSim.Engine.Rendering.CommandBuffer;
using Pipeline = SpatialSim.Engine.Rendering.Pipeline;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using VkCommandBuffer = SpatialSim.Engine.Rendering.Vulkan.VkCommandBuffer;

namespace SpatialSim.Engine.Core.Vulkan
{
    public class VkContext : AppContext
    {
        /// <summary>
        /// IMPORTANT This only supports, probably vulkan 1.0 commands
        /// </summary>
        public Vk vk;
        public Instance instance;
        
        public GraphicsAPI graphicsApi { get; set; }
        public IDeviceFactory DeviceFactory { get; set; }
        public RenderPass renderPass { get; set; }
        public VkImGuiController imGuiController;
        
        public static int imageIndex;
        public static CommandBuffer drawCmdBuf;
        
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
            
            VkDepthBuffer.CreateDepthBuffers();
            VkSwapChain.CreateSyncObjects();
            VkSwapChain.CreateSwapChainCommandBuffers();
            
            VkCreation.CreateImGui();
            
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
                    Ticks.swapchainRecreations.created++;

                    currentlyResizing = false;
                    swapChainRecreationCounter = 0f;
                    currentSwapChainRecreations = 0;
                }
            }
        }
        
        public unsafe void Render()
        {
            imageIndex = PrepareFrame();
            if (imageIndex == -1)
            {
                return;
            }
            //render
            
            drawCmdBuf = VkSwapChain.commandBuffers[imageIndex];
            VkSwapChain.commandBuffers[imageIndex].Begin();
            
            VkTexture.TransitionImageLayout(
                drawCmdBuf,
                VkDepthBuffer.texture.image,
                ImageLayout.Undefined,
                ImageLayout.DepthStencilAttachmentOptimal,
                AccessFlags.DepthStencilAttachmentWriteBit,
                AccessFlags.DepthStencilAttachmentWriteBit,
                PipelineStageFlags.EarlyFragmentTestsBit | PipelineStageFlags.LateFragmentTestsBit,
                PipelineStageFlags.EarlyFragmentTestsBit | PipelineStageFlags.LateFragmentTestsBit,
                ImageAspectFlags.DepthBit
            );

            // If we have no post processes to run then just write to swapchain
            if (PostProcessManager.postProcesses.Count == 0)
            {
                VkTexture.TransitionImageLayout(
                    drawCmdBuf,
                    VkSwapChain.swapChainImages[imageIndex],
                    ImageLayout.Undefined,
                    ImageLayout.ColorAttachmentOptimal,
                    AccessFlags.None,
                    AccessFlags.ColorAttachmentWriteBit,
                    PipelineStageFlags.ColorAttachmentOutputBit,
                    PipelineStageFlags.ColorAttachmentOutputBit,
                    ImageAspectFlags.ColorBit
                );
            
                EcsManager.Render(drawCmdBuf, imageIndex);
            
                imGuiController.Render(
                    ((VkCommandBuffer)drawCmdBuf.commandBuffer!).commandBuffer,
                    VkSwapChain.swapChainImageViews[imageIndex],
                    VkDepthBuffer.texture.imageView,
                    VkSwapChain.swapChainExtent,
                    Window.windowScale);
            }
            else
            {
                //render world to first texture
                PostProcessManager.postProcesses.GetValueAtIndex(0).EnableWrite(drawCmdBuf);
                
                EcsManager.Render(drawCmdBuf, PostProcessManager.postProcesses.GetValueAtIndex(0).texture);
                
                PostProcessManager.postProcesses.GetValueAtIndex(0).EnableRead(drawCmdBuf);
                
                //render all post processes 
                for (int i = 1; i < PostProcessManager.postProcesses.Count; i++)
                {
                    PostProcessManager.postProcesses.GetValueAtIndex(i).EnableWrite(drawCmdBuf);
                    PostProcessManager.postProcesses.GetValueAtIndex(i - 1).Render(drawCmdBuf, 
                        PostProcessManager.postProcesses.GetValueAtIndex(i).texture);
                    PostProcessManager.postProcesses.GetValueAtIndex(i).EnableRead(drawCmdBuf);
                }
                
                VkTexture.TransitionImageLayout(
                    drawCmdBuf,
                    VkSwapChain.swapChainImages[imageIndex],
                    ImageLayout.Undefined,
                    ImageLayout.ColorAttachmentOptimal,
                    AccessFlags.ColorAttachmentWriteBit,
                    AccessFlags.ColorAttachmentWriteBit,
                    PipelineStageFlags.ColorAttachmentOutputBit,
                    PipelineStageFlags.ColorAttachmentOutputBit,
                    ImageAspectFlags.ColorBit
                );
                
                PostProcessManager.postProcesses.GetValueAtIndex(PostProcessManager.postProcesses.Count - 1).Render(drawCmdBuf, imageIndex);
                
                imGuiController.Render(
                    ((VkCommandBuffer)drawCmdBuf.commandBuffer!).commandBuffer,
                    VkSwapChain.swapChainImageViews[imageIndex],
                    VkDepthBuffer.texture.imageView,
                    VkSwapChain.swapChainExtent,
                    Window.windowScale);
                
            }
            
            
            VkTexture.TransitionImageLayout(
                drawCmdBuf, 
                VkSwapChain.swapChainImages[imageIndex],
                ImageLayout.ColorAttachmentOptimal,
                ImageLayout.PresentSrcKhr,
                AccessFlags.ColorAttachmentWriteBit,
                AccessFlags.None,
                PipelineStageFlags.ColorAttachmentOutputBit,
                PipelineStageFlags.BottomOfPipeBit,
                ImageAspectFlags.ColorBit
            );
        }

        public void FinishRender()
        {
            drawCmdBuf.End();
            PipelineManager.ResetPipelines(drawCmdBuf);
            
            SubmitFrame();
        }

        unsafe int PrepareFrame()
        {
            //wait for the current frame to complete
            vk.WaitForFences(VkDevices.device, 
                1,
                in VkSwapChain.inFlightFences[VkSwapChain.currentFrame], 
                true,
                ulong.MaxValue);
            
            //grab an image to render to at the image index
            uint imageIndex = 0;
            Result result = VkSwapChain.khrSwapChain!.AcquireNextImage(VkDevices.device, 
                VkSwapChain.swapChain, 
                ulong.MaxValue, 
                VkSwapChain.imageAvailableSemaphores[VkSwapChain.currentFrame], 
                default, 
                ref imageIndex);

            if (result == Result.ErrorOutOfDateKhr)
            {
                VkSwapChain.RecreateSwapChain();
                return -1;
            }
            else if (result != Result.Success && result != Result.SuboptimalKhr)
            {
                Debug.Error($"Failed to acquire swap chain image {result}");
                throw new Exception($"Failed to acquire swap chain image {result}");
            }

            if (VkSwapChain.imagesInFlight[imageIndex].Handle != 0)
            {
                vk.WaitForFences(VkDevices.device, 
                    1,
                    in VkSwapChain.imagesInFlight[imageIndex],
                    true,
                    ulong.MaxValue);
            }
            VkSwapChain.imagesInFlight[imageIndex] = VkSwapChain.inFlightFences[VkSwapChain.currentFrame];

            return (int)imageIndex;
        }

        unsafe void SubmitFrame()
        {
            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
            };

            Semaphore* waitSemaphores = stackalloc[] { VkSwapChain.imageAvailableSemaphores[VkSwapChain.currentFrame] };
            PipelineStageFlags* waitStages = stackalloc[] { PipelineStageFlags.ColorAttachmentOutputBit };
            Semaphore* signalSemaphores = stackalloc[] { VkSwapChain.renderFinishedSemaphores[imageIndex] };

            fixed (Silk.NET.Vulkan.CommandBuffer* cmdBuf = &((VkCommandBuffer)drawCmdBuf.commandBuffer!).commandBuffer)
            {
                submitInfo = submitInfo with
                {
                    WaitSemaphoreCount = 1,
                    PWaitSemaphores = waitSemaphores,
                    PWaitDstStageMask = waitStages,

                    CommandBufferCount = 1,
                    PCommandBuffers = cmdBuf,
                
                    SignalSemaphoreCount = 1,
                    PSignalSemaphores = signalSemaphores,
                };
            }

            vk.ResetFences(VkDevices.device, 1, in VkSwapChain.inFlightFences[VkSwapChain.currentFrame]);
            Result result = vk.QueueSubmit(VkDevices.graphicsQueue, 1, in submitInfo,
                VkSwapChain.inFlightFences[VkSwapChain.currentFrame]);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to submit draw command buffer {result}");
                throw new Exception($"Failed to submit draw command buffer {result}");
            }

            SwapchainKHR* swapChains = stackalloc[] { VkSwapChain.swapChain };
            uint imageIndexVal = (uint)imageIndex;
            PresentInfoKHR presentInfo = new()
            {
                SType = StructureType.PresentInfoKhr,

                WaitSemaphoreCount = 1,
                PWaitSemaphores = signalSemaphores,

                SwapchainCount = 1,
                PSwapchains = swapChains,

                PImageIndices = &imageIndexVal
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

            VkSwapChain.currentFrame = (VkSwapChain.currentFrame + 1) % VkSettings.MAX_FRAMES_IN_FLIGHT;
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

            if (AppState.EnableValidationLayers)
            {
                VkValidationLayers.debugUtils.DestroyDebugUtilsMessenger(instance, VkValidationLayers.debugMessenger, null);
            }
            
            VkSurface.khrSurface.DestroySurface(instance, VkSurface.surface, null);
            
            vk.DestroyInstance(instance, null);
            vk.Dispose();
            
            Debug.LogInfo("Cleaned all vulkan instances");
        }

        public Texture GetRenderTexture()
        {
            Texture texture = new Texture()
            {
                data = new TextureData()
                {
                    info = new TextureInfo()
                    {
                        width = VkSwapChain.swapChainExtent.Width,
                        height = VkSwapChain.swapChainExtent.Height
                    }
                }
            };
            texture.texture = new VkTexture();
            ((VkTexture)texture.texture!).Create(
                VkSwapChain.swapChainExtent.Width,
                VkSwapChain.swapChainExtent.Height,
                1,
                VkSwapChain.swapChainImageFormat,
                ImageTiling.Optimal,
                ImageUsageFlags.SampledBit,
                MemoryPropertyFlags.HostVisibleBit,
                ImageType.Type2D);
            
            ((VkTexture)texture.texture!).TransitionImageLayout(ImageLayout.Undefined,
                ImageLayout.ShaderReadOnlyOptimal,
                0,
                AccessFlags.ShaderReadBit,
                PipelineStageFlags.TopOfPipeBit,
                PipelineStageFlags.FragmentShaderBit,
                ImageAspectFlags.ColorBit);
            
            ((VkTexture)texture.texture!).CreateImageView(ImageAspectFlags.ColorBit);

            ((VkTexture)texture.texture!).CopyImageToImage(
                VkSwapChain.swapChainImages[VkSwapChain.currentFrame],
                ImageLayout.TransferSrcOptimal,
                texture.data);
            
            return texture;
        }

        T AppContext.GetContext<T>()
        {
            if (typeof(T) != typeof(VkContext))
            {
                Debug.Error($"Type {typeof(T).Name} does not match contained type {nameof(VkContext)}");
                throw new Exception($"Type {typeof(T)} does not match contained type {nameof(VkContext)}");
            }
            
            return (T)(object)this;
        }
    }
}