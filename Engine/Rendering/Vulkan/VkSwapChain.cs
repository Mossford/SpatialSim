using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public static class VkSwapChain
    {
        public struct SwapChainSupportDetails
        {
            public SurfaceCapabilitiesKHR Capabilities;
            public SurfaceFormatKHR[] Formats;
            public PresentModeKHR[] PresentModes;
        }
        
        public static KhrSwapchain? khrSwapChain;
        public static SwapchainKHR swapChain;
        public static Image[] swapChainImages;
        public static Format swapChainImageFormat;
        public static Extent2D swapChainExtent;
        public static ImageView[] swapChainImageViews;
        public static Framebuffer[] swapChainFramebuffers;
        public static PresentModeKHR presentMode;
        
        public static Silk.NET.Vulkan.Semaphore[] imageAvailableSemaphores;
        public static Silk.NET.Vulkan.Semaphore[] renderFinishedSemaphores;
        public static Fence[] inFlightFences;
        public static Fence[] imagesInFlight;
        public static int currentFrame = 0;

        static CommandBuffer commandPool;
        public static CommandBuffer[] commandBuffers;
        
        public const int MAX_FRAMES_IN_FLIGHT = 2;

        public static unsafe void CreateSwapChain()
        {
            SwapChainSupportDetails swapChainSupport = QuerySwapChainSupport(VkDevices.physicalDevice);

            SurfaceFormatKHR surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
            presentMode = ChoosePresentMode(swapChainSupport.PresentModes);
            Extent2D extent = ChooseSwapExtent(swapChainSupport.Capabilities);

            uint imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
            if (swapChainSupport.Capabilities.MaxImageCount > 0 && imageCount > swapChainSupport.Capabilities.MaxImageCount)
            {
                imageCount = swapChainSupport.Capabilities.MaxImageCount;
            }

            SwapchainCreateInfoKHR createInfo = new()
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = VkSurface.surface,

                MinImageCount = imageCount,
                ImageFormat = surfaceFormat.Format,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageExtent = extent,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            };

            VkDevices.QueueFamilyIndices indices = VkDevices.FindQueueFamilies(VkDevices.physicalDevice);
            uint* queueFamilyIndices = stackalloc[]
            {
                indices.GraphicsFamily!.Value, 
                indices.PresentFamily!.Value
            };

            if (indices.GraphicsFamily != indices.PresentFamily)
            {
                createInfo = createInfo with
                {
                    ImageSharingMode = SharingMode.Concurrent,
                    QueueFamilyIndexCount = 2,
                    PQueueFamilyIndices = queueFamilyIndices,
                };
            }
            else
            {
                createInfo.ImageSharingMode = SharingMode.Exclusive;
            }

            createInfo = createInfo with
            {
                PreTransform = swapChainSupport.Capabilities.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
                PresentMode = presentMode,
                Clipped = true,

                OldSwapchain = default
            };
            
            if (khrSwapChain is null)
            {
                if (!AppState.appContext.GetContext<VkContext>().vk.TryGetDeviceExtension(AppState.appContext.GetContext<VkContext>().instance, VkDevices.device, out khrSwapChain))
                {
                    Debug.Error("VK_KHR_swapchain extension not found");
                    throw new NotSupportedException("VK_KHR_swapchain extension not found");
                }
            }

            if (khrSwapChain!.CreateSwapchain(VkDevices.device, in createInfo, null, out swapChain) != Result.Success)
            {
                Debug.Error("Failed to create swap chain");
                throw new Exception("Failed to create swap chain");
            }

            khrSwapChain.GetSwapchainImages(VkDevices.device, swapChain, ref imageCount, null);
            swapChainImages = new Image[imageCount];
            fixed (Image* swapChainImagesPtr = swapChainImages)
            {
                khrSwapChain.GetSwapchainImages(VkDevices.device, swapChain, ref imageCount, swapChainImagesPtr);
            }

            swapChainImageFormat = surfaceFormat.Format;
            swapChainExtent = extent;
            
            Debug.LogInfo("Successful swapchain creation");
        }
        
        public static unsafe void CreateSyncObjects()
        {
            imageAvailableSemaphores = new Silk.NET.Vulkan.Semaphore[MAX_FRAMES_IN_FLIGHT];
            renderFinishedSemaphores = new Silk.NET.Vulkan.Semaphore[swapChainImages.Length];
            inFlightFences = new Fence[MAX_FRAMES_IN_FLIGHT];
            imagesInFlight = new Fence[swapChainImages.Length];

            SemaphoreCreateInfo semaphoreInfo = new()
            {
                SType = StructureType.SemaphoreCreateInfo,
            };

            FenceCreateInfo fenceInfo = new()
            {
                SType = StructureType.FenceCreateInfo,
                Flags = FenceCreateFlags.SignaledBit,
            };

            for (int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
            {
                if (AppState.appContext.GetContext<VkContext>().vk.CreateSemaphore(VkDevices.device, in semaphoreInfo, null, out imageAvailableSemaphores[i]) != Result.Success ||
                    AppState.appContext.GetContext<VkContext>().vk.CreateFence(VkDevices.device, in fenceInfo, null, out inFlightFences[i]) != Result.Success)
                {
                    Debug.Error("Failed to create synchronization objects for a frame");
                    throw new Exception("Failed to create synchronization objects for a frame");
                }
            }
            
            for (var i = 0; i < swapChainImages.Length; i++)
            {
                if (AppState.appContext.GetContext<VkContext>().vk.CreateSemaphore(VkDevices.device, in semaphoreInfo, null, out renderFinishedSemaphores[i]) != Result.Success)
                {
                    Debug.Error("Failed to create synchronization objects for a frame");
                    throw new Exception("Failed to create synchronization objects for a frame");
                }
            }
            
            Debug.LogInfo("Successful sync object creation");
        }

        public static void CreateSwapChainCommandBuffers()
        {
            commandPool = new CommandBuffer();
            commandPool.commandBuffer = new VkCommandBuffer();
            ((VkCommandBuffer)commandPool.commandBuffer!).CreateCommandPool();
            commandBuffers = new CommandBuffer[swapChainImages.Length];
            for (int i = 0; i < commandBuffers.Length; i++)
            {
                commandBuffers[i] = new CommandBuffer();
                commandBuffers[i].commandBuffer = new VkCommandBuffer();
                ((VkCommandBuffer)commandBuffers[i].commandBuffer!).Create(((VkCommandBuffer)commandPool.commandBuffer!).commandPool);
            }
        }
        
        public static unsafe void RecreateSwapChain()
        {
            Debug.LogInfo("Start Swapchain recreation");
            
            while (Window.size.X == 0 || Window.size.Y == 0)
            {
                Window.size = (Vector2)AppState.window.FramebufferSize;
                Window.windowScale = Window.size / (Vector2)AppState.window.Size;
                Window.scaleFromBase = Window.size / AppState.WindowStartSize;
                AppState.window.DoEvents();
            }
            
            AppState.appContext.GetContext<VkContext>().vk.DeviceWaitIdle(VkDevices.device);

            foreach (Framebuffer framebuffer in swapChainFramebuffers!)
            {
                AppState.appContext.GetContext<VkContext>().vk.DestroyFramebuffer(VkDevices.device, framebuffer, null);
            }

            for (int i = 0; i < commandBuffers.Length; i++)
            {
                AppState.appContext.GetContext<VkContext>().vk.FreeCommandBuffers(
                    VkDevices.device, 
                    ((VkCommandBuffer)commandPool.commandBuffer!).commandPool, 1, 
                    ref ((VkCommandBuffer)commandBuffers[i].commandBuffer!).commandBuffer);
            }

            AppState.appContext.defaultPipeline.Clean();
            AppState.appContext.renderPass.Clean();

            foreach (ImageView imageView in swapChainImageViews!)
            {
                AppState.appContext.GetContext<VkContext>().vk.DestroyImageView(VkDevices.device, imageView, null);
            }

            khrSwapChain!.DestroySwapchain(VkDevices.device, swapChain, null);

            CreateSwapChain();
            CreateImageViews();
            AppState.appContext.renderPass = new VkRenderPass();
            AppState.appContext.renderPass.Create();
            
            AppState.appContext.defaultPipeline.Create(
                ShaderManager.RetrieveShader(new ShaderSettings(ShaderType.Vertex, "base.vert")), 
                ShaderManager.RetrieveShader(new ShaderSettings(ShaderType.Fragment, "base.frag")));
            
            CreateFramebuffers();
            
            for (int i = 0; i < commandBuffers.Length; i++)
            {
                ((VkCommandBuffer)commandBuffers[i].commandBuffer!).Create(((VkCommandBuffer)commandPool.commandBuffer!).commandPool);
            }
            
            imagesInFlight = new Fence[swapChainImages.Length];
            
            Debug.LogInfo("Successful swapchain recreation");
        }

        public static unsafe void CleanSwapChain()
        {
            foreach (Framebuffer framebuffer in swapChainFramebuffers!)
            {
                AppState.appContext.GetContext<VkContext>().vk.DestroyFramebuffer(VkDevices.device, framebuffer, null);
            }

            AppState.appContext.defaultPipeline.Clean();
            AppState.appContext.renderPass.Clean();

            foreach (ImageView imageView in swapChainImageViews!)
            {
                AppState.appContext.GetContext<VkContext>().vk.DestroyImageView(VkDevices.device, imageView, null);
            }

            khrSwapChain!.DestroySwapchain(VkDevices.device, swapChain, null);
            
            for (int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
            {
                AppState.appContext.GetContext<VkContext>().vk.DestroySemaphore(VkDevices.device, imageAvailableSemaphores[i], null);
                AppState.appContext.GetContext<VkContext>().vk.DestroyFence(VkDevices.device, inFlightFences[i], null);
            }
            
            AppState.appContext.GetContext<VkContext>().vk.DestroyCommandPool(
                VkDevices.device, 
                ((VkCommandBuffer)commandPool.commandBuffer!).commandPool, 
                null);
            
            for (int i = 0; i < swapChainImages.Length; i++)
            {
                AppState.appContext.GetContext<VkContext>().vk.DestroySemaphore(VkDevices.device, renderFinishedSemaphores[i], null);
            }
            
            Debug.LogInfo("Cleaned up SwapChain");
        }
        
        public static unsafe void CreateFramebuffers()
        {
            swapChainFramebuffers = new Framebuffer[swapChainImageViews.Length];

            for (int i = 0; i < swapChainImageViews.Length; i++)
            {
                var attachment = swapChainImageViews[i];

                FramebufferCreateInfo framebufferInfo = new()
                {
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = ((VkRenderPass)AppState.appContext.renderPass).renderPass,
                    AttachmentCount = 1,
                    PAttachments = &attachment,
                    Width = swapChainExtent.Width,
                    Height = swapChainExtent.Height,
                    Layers = 1,
                };

                if (AppState.appContext.GetContext<VkContext>().vk.CreateFramebuffer(VkDevices.device, in framebufferInfo, null, out swapChainFramebuffers[i]) != Result.Success)
                {
                    Debug.Error("Failed to create framebuffer");
                    throw new Exception("Failed to create framebuffer");
                }
            }
            
            Debug.LogInfo("Successful swapchain framebuffers creation");
        }

        static SurfaceFormatKHR ChooseSwapSurfaceFormat(IReadOnlyList<SurfaceFormatKHR> availableFormats)
        {
            foreach (var availableFormat in availableFormats)
            {
                //Imgui uses B8G8R8A8Unorm
                if (availableFormat.Format == Format.B8G8R8A8Unorm && availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
                {
                    return availableFormat;
                }
            }

            return availableFormats[0];
        }

        static PresentModeKHR ChoosePresentMode(IReadOnlyList<PresentModeKHR> availablePresentModes)
        {
            foreach (var availablePresentMode in availablePresentModes)
            {
                if (availablePresentMode == PresentModeKHR.ImmediateKhr)
                {
                    return availablePresentMode;
                }
            }

            return PresentModeKHR.FifoKhr;
        }

        static Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
        {
            if (capabilities.CurrentExtent.Width != uint.MaxValue)
            {
                return capabilities.CurrentExtent;
            }
            else
            {
                Vector2D<int> framebufferSize = AppState.window.FramebufferSize;

                Extent2D actualExtent = new()
                {
                    Width = (uint)framebufferSize.X,
                    Height = (uint)framebufferSize.Y
                };

                actualExtent.Width = Math.Clamp(actualExtent.Width, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width);
                actualExtent.Height = Math.Clamp(actualExtent.Height, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height);

                return actualExtent;
            }
        }

        public static unsafe SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice physicalDevice)
        {
            SwapChainSupportDetails details = new SwapChainSupportDetails();

            VkSurface.khrSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, VkSurface.surface, out details.Capabilities);

            uint formatCount = 0;
            VkSurface.khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, VkSurface.surface, ref formatCount, null);

            if (formatCount != 0)
            {
                details.Formats = new SurfaceFormatKHR[formatCount];
                fixed (SurfaceFormatKHR* formatsPtr = details.Formats)
                {
                    VkSurface.khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, VkSurface.surface, ref formatCount, formatsPtr);
                }
            }
            else
            {
                details.Formats = Array.Empty<SurfaceFormatKHR>();
            }

            uint presentModeCount = 0;
            VkSurface.khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, VkSurface.surface, ref presentModeCount, null);

            if (presentModeCount != 0)
            {
                details.PresentModes = new PresentModeKHR[presentModeCount];
                fixed (PresentModeKHR* formatsPtr = details.PresentModes)
                {
                    VkSurface.khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, VkSurface.surface, ref presentModeCount, formatsPtr);
                }

            }
            else
            {
                details.PresentModes = Array.Empty<PresentModeKHR>();
            }

            return details;
        }
        
        public static unsafe void CreateImageViews()
        {
            swapChainImageViews = new ImageView[swapChainImages.Length];

            for (int i = 0; i < swapChainImages.Length; i++)
            {
                ImageViewCreateInfo createInfo = new()
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = swapChainImages[i],
                    ViewType = ImageViewType.Type2D,
                    Format = swapChainImageFormat,
                    Components =
                    {
                        R = ComponentSwizzle.Identity,
                        G = ComponentSwizzle.Identity,
                        B = ComponentSwizzle.Identity,
                        A = ComponentSwizzle.Identity,
                    },
                    SubresourceRange =
                    {
                        AspectMask = ImageAspectFlags.ColorBit,
                        BaseMipLevel = 0,
                        LevelCount = 1,
                        BaseArrayLayer = 0,
                        LayerCount = 1,
                    }

                };

                if (AppState.appContext.GetContext<VkContext>().vk.CreateImageView(VkDevices.device, in createInfo, null, out swapChainImageViews[i]) != Result.Success)
                {
                    Debug.Error("Failed to create image views");
                    throw new Exception("Failed to create image views");
                }
            }
            
            Debug.LogInfo("Successful image view creation");
        }
    }
}