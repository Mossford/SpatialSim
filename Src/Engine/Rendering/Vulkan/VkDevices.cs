using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public static class VkDevices
    {
        public static PhysicalDevice physicalDevice;
        public static Device device;
        //meant for graphics commands
        public static Queue graphicsQueue;
        public static uint graphicsFamilyIndex;
        public static PhysicalDeviceProperties properties;
        public static PhysicalDeviceProperties2 properties2;

        //this needs to exist because the vk class will segfault on commands for dynamic rendering as they dont have the methods loaded
        public static KhrDynamicRendering dynamicRendering;
        public static KhrAccelerationStructure accelerationStructure;
        
        public struct QueueFamilyIndices
        {
            public uint? GraphicsFamily { get; set; }
            public uint? PresentFamily { get; set; }

            public bool IsComplete()
            {
                return GraphicsFamily.HasValue && PresentFamily.HasValue;
            }
        }
        
        public static unsafe void PickPhysicalDevice()
        {
            IReadOnlyCollection<PhysicalDevice> devices = AppState.appContext.GetContext<VkContext>().vk.GetPhysicalDevices(AppState.appContext.GetContext<VkContext>().instance);
            
            foreach (PhysicalDevice device in devices)
            {
                if (IsDeviceSuitable(device))
                {
                    physicalDevice = device;
                    break;
                }
            }

            if (physicalDevice.Handle == 0)
            {
                Debug.Error("Failed to find a suitable GPU");
                throw new Exception("Failed to find a suitable GPU");
            }
            
            AppState.appContext.GetContext<VkContext>().vk.GetPhysicalDeviceProperties(physicalDevice, out properties);
            AppState.appContext.GetContext<VkContext>().vk.GetPhysicalDeviceProperties2(physicalDevice, out properties2);
            fixed(PhysicalDeviceProperties* ptr = &properties)
                AppState.gpuDeviceName = Encoding.UTF8.GetString(ptr->DeviceName, (int)Vk.MaxPhysicalDeviceNameSize);
            
            Debug.LogInfo("Successful physical device creation");
        }
        
        public static unsafe void CreateLogicalDevice()
        {
            QueueFamilyIndices indices = FindQueueFamilies(physicalDevice);

            uint[] uniqueQueueFamilies = new[]
            {
                indices.GraphicsFamily!.Value, 
                indices.PresentFamily!.Value
            };
            
            uniqueQueueFamilies = uniqueQueueFamilies.Distinct().ToArray();
            graphicsFamilyIndex = indices.GraphicsFamily.Value;

            using GlobalMemory? mem = GlobalMemory.Allocate(uniqueQueueFamilies.Length * sizeof(DeviceQueueCreateInfo));
            DeviceQueueCreateInfo* queueCreateInfos = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref mem.GetPinnableReference());

            float queuePriority = 1.0f;
            for (int i = 0; i < uniqueQueueFamilies.Length; i++)
            {
                queueCreateInfos[i] = new()
                {
                    SType = StructureType.DeviceQueueCreateInfo,
                    QueueFamilyIndex = uniqueQueueFamilies[i],
                    QueueCount = 1,
                    PQueuePriorities = &queuePriority
                };
            }

            DeviceCreateInfo createInfo;

            PhysicalDeviceVulkan13Features features = new PhysicalDeviceVulkan13Features()
            {
                SType = StructureType.PhysicalDeviceVulkan13Features,
                DynamicRendering = true,
            };

            fixed (PhysicalDeviceFeatures* deviceFeatures = &VkSettings.physicalDeviceFeatures)
            {
                createInfo = new()
                {
                    SType = StructureType.DeviceCreateInfo,
                    QueueCreateInfoCount = (uint)uniqueQueueFamilies.Length,
                    PQueueCreateInfos = queueCreateInfos,

                    PEnabledFeatures = deviceFeatures,
                    PNext = &features,

                    EnabledExtensionCount = (uint)VkSettings.deviceExtensions.Length,
                    PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(VkSettings.deviceExtensions),
                };
            }

            if (AppState.EnableVkValidationLayers)
            {
                createInfo.EnabledLayerCount = (uint)VkSettings.validationLayers.Length;
                createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(VkSettings.validationLayers);
            }
            else
            {
                createInfo.EnabledLayerCount = 0;
            }

            Result result = AppState.appContext.GetContext<VkContext>().vk
                .CreateDevice(physicalDevice, in createInfo, null, out device);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to create logical device {result}");
                throw new Exception($"Failed to create logical device {result}");
            }

            AppState.appContext.GetContext<VkContext>().vk.GetDeviceQueue(device, indices.GraphicsFamily!.Value, 0, out graphicsQueue);
            AppState.appContext.GetContext<VkContext>().vk.GetDeviceQueue(device, indices.PresentFamily!.Value, 0, out VkSurface.presentQueue);

            if (AppState.EnableVkValidationLayers)
            {
                SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
            }

            AppState.appContext.GetContext<VkContext>().vk
                .TryGetDeviceExtension(AppState.appContext.GetContext<VkContext>().instance, device,
                    out dynamicRendering);
            AppState.appContext.GetContext<VkContext>().vk
                .TryGetDeviceExtension(AppState.appContext.GetContext<VkContext>().instance, device,
                    out accelerationStructure);
            
            Debug.LogInfo("Successful logical device creation");
        }

        static bool IsDeviceSuitable(PhysicalDevice device)
        {
            QueueFamilyIndices indices = FindQueueFamilies(device);

            bool extensionsSupported = CheckDeviceExtensionsSupport(device);

            bool swapChainAdequate = false;
            if (extensionsSupported)
            {
                VkSwapChain.SwapChainSupportDetails swapChainSupport = VkSwapChain.QuerySwapChainSupport(device);
                swapChainAdequate = swapChainSupport.Formats.Any() && swapChainSupport.PresentModes.Any();
            }

            return indices.IsComplete() && extensionsSupported && swapChainAdequate && CheckDeviceFeatureSupport(device);
        }
        
        public static uint FindMemoryType(uint typeFilter, MemoryPropertyFlags selProperties)
        {
            AppState.appContext.GetContext<VkContext>().vk.GetPhysicalDeviceMemoryProperties(physicalDevice, out PhysicalDeviceMemoryProperties memProperties);

            for (int i = 0; i < memProperties.MemoryTypeCount; i++)
            {
                if ((typeFilter & (1 << i)) != 0 && (memProperties.MemoryTypes[i].PropertyFlags & selProperties) == selProperties)
                {
                    return (uint)i;
                }
            }

            Debug.Error($"Failed to find suitable memory type {selProperties}");
            throw new Exception($"Failed to find suitable memory type {selProperties}");
        }

        
        public static Format FindSupportedFormat(IEnumerable<Format> candidates, ImageTiling tiling, FormatFeatureFlags features)
        {
            foreach (Format format in candidates)
            {
                AppState.appContext.GetContext<VkContext>().vk.GetPhysicalDeviceFormatProperties(physicalDevice, format, out FormatProperties props);

                if (tiling == ImageTiling.Linear && (props.LinearTilingFeatures & features) == features)
                {
                    return format;
                }
                else if (tiling == ImageTiling.Optimal && (props.OptimalTilingFeatures & features) == features)
                {
                    return format;
                }
            }

            foreach (Format format in candidates)
            {
                Debug.Error($"Failed to find supported format {format}");
                throw new Exception($"Failed to find supported format {format}");
            }
            
            throw new Exception($"Failed to find supported format");
        }
        
        /// <summary>
        /// Check if device has all extensions specified in the deviceExtensions array
        /// </summary>
        /// <param name="device">The physical device to check</param>
        /// <returns></returns>
        public static unsafe bool CheckDeviceExtensionsSupport(PhysicalDevice device)
        {
            uint extentionsCount = 0;
            AppState.appContext.GetContext<VkContext>().vk.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extentionsCount, null);

            ExtensionProperties[] availableExtensions = new ExtensionProperties[extentionsCount];
            fixed (ExtensionProperties* availableExtensionsPtr = availableExtensions)
            {
                AppState.appContext.GetContext<VkContext>().vk.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extentionsCount, availableExtensionsPtr);
            }

            HashSet<string?> availableExtensionNames = availableExtensions.Select(extension => Marshal.PtrToStringAnsi((IntPtr)extension.ExtensionName)).ToHashSet();

            return VkSettings.deviceExtensions.All(availableExtensionNames.Contains);
        }

        public static bool CheckDeviceFeatureSupport(PhysicalDevice device)
        {
            AppState.appContext.GetContext<VkContext>().vk
                .GetPhysicalDeviceFeatures(device, out PhysicalDeviceFeatures features);
            
            if (features.SamplerAnisotropy)
            {
                return true;
            }

            return false;
        }

        public static unsafe QueueFamilyIndices FindQueueFamilies(PhysicalDevice device)
        {
            QueueFamilyIndices indices = new QueueFamilyIndices();

            uint queueFamilityCount = 0;
            AppState.appContext.GetContext<VkContext>().vk.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, null);

            QueueFamilyProperties[] queueFamilies = new QueueFamilyProperties[queueFamilityCount];
            fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
            {
                AppState.appContext.GetContext<VkContext>().vk.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, queueFamiliesPtr);
            }
            
            for (uint i = 0; i < queueFamilies.Length; i++)
            {
                if (queueFamilies[i].QueueFlags.HasFlag(QueueFlags.GraphicsBit))
                {
                    indices.GraphicsFamily = i;
                }

                VkSurface.khrSurface.GetPhysicalDeviceSurfaceSupport(device, i, VkSurface.surface, out Bool32 presentSupport);

                if (presentSupport)
                {
                    indices.PresentFamily = i;
                }

                if (indices.IsComplete())
                {
                    break;
                }
                
            }

            return indices;
        }
    }
}