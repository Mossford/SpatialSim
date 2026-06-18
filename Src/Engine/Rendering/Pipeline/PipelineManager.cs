using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public static class PipelineManager
    {
        public static Dictionary<string, int> pipelineToIndex;
        public static List<Pipeline> pipelines;
        
        public static void Init()
        {
            pipelineToIndex = new Dictionary<string, int>();
            pipelines = new List<Pipeline>();
            pipelines.Add(new NormalMapPipeline() {
                settings = new PipelineSettings()
                {
                    blendColor = false,
                    depthTest = true,
                    format = TextureFormat.R8G8B8A8Unorm
                }
            });
            pipelineToIndex.Add(DefaultPipelines.BaseNormalMap, pipelines.Count - 1);
            pipelines[^1].Create(ShaderManager.RetrieveShader(
                    new ShaderSettings(
                        ShaderType.Vertex,
                        [
                            new ShaderDescriptorDef(RendererSettings.VertexUniformSet, [0],
                                ShaderDescriptorUsage.Uniform, ShaderType.Vertex)
                        ],
                        "basenormalmap.vert")),
                ShaderManager.RetrieveShader(
                    new ShaderSettings(ShaderType.Fragment,
                        [
                            new ShaderDescriptorDef(RendererSettings.FragmentSamplerSet, [0],
                                ShaderDescriptorUsage.Sampler, ShaderType.Fragment),
                            new ShaderDescriptorDef(RendererSettings.FragmentUniformSet, [0],
                                ShaderDescriptorUsage.Uniform, ShaderType.Fragment)
                        ],
                        "basenormalmap.frag")));
            
            pipelines.Add(new ColorMapPipeline() {
                settings = new PipelineSettings()
                {
                    blendColor = false,
                    depthTest = true,
                    format = TextureFormat.R8G8B8A8Unorm
                }
            });
            pipelineToIndex.Add(DefaultPipelines.BaseColor, pipelines.Count - 1);
            pipelines[^1].Create(ShaderManager.RetrieveShader(
                    new ShaderSettings(
                        ShaderType.Vertex,
                        [
                            new ShaderDescriptorDef(RendererSettings.VertexUniformSet, [0],
                                ShaderDescriptorUsage.Uniform, ShaderType.Vertex)
                        ],
                        "basecolormap.vert")),
                ShaderManager.RetrieveShader(
                    new ShaderSettings(ShaderType.Fragment,
                        [
                            new ShaderDescriptorDef(RendererSettings.FragmentSamplerSet, [0],
                                ShaderDescriptorUsage.Sampler, ShaderType.Fragment),
                            new ShaderDescriptorDef(RendererSettings.FragmentUniformSet, [0],
                                ShaderDescriptorUsage.Uniform, ShaderType.Fragment)
                        ],
                        "basecolormap.frag")));
            
            Debug.LogInfo("Successful pipeline manager creation");
        }

        public static bool IsPipelineStored(string pipelineName)
        {
            return pipelineToIndex.ContainsKey(pipelineName);
        }

        public static bool LoadPipeline(Pipeline pipeline, Shader[] shaders)
        {
            //TODO remove this check
            if (shaders.Length != 2)
            {
                Debug.Error("Tried to load pipeline at an incorrect amount of shaders");
                return false;
            }
            
            if (pipelineToIndex.TryAdd(pipeline.name, pipelines.Count))
            {
                pipelines.Add(pipeline);
                //TODO make the pipeline accept an array of shaders and a usage and infer what to then do
                pipelines[^1].Create(shaders[0], shaders[1]);
                return true;
            }

            Debug.Warning($"Could not add pipeline {pipeline.name} possible duplicate");
            return false;
        }

        public static Pipeline RetrievePipeline(string pipelineName)
        {
            if (pipelineToIndex.TryGetValue(pipelineName, out int index))
            {
                return pipelines[index];
            }
            
            Debug.Warning($"Tried to get a pipeline that does not exist returning default {DefaultPipelines.BaseColor}");
            return pipelines[pipelineToIndex[DefaultPipelines.BaseColor]];
        }
        
        public static Pipeline RetrievePipeline(int pipeline)
        {
            if (pipeline >= 0 && pipeline < pipelines.Count)
            {
                return pipelines[pipeline];
            }
            
            Debug.Warning($"Tried to get a pipeline that does not exist returning default {DefaultPipelines.BaseColor}");
            return pipelines[pipelineToIndex[DefaultPipelines.BaseColor]];
        }

        public static void ResetPipelines(CommandBuffer commandBuffer)
        {
            for (int i = 0; i < pipelines.Count; i++)
            {
                commandBuffer.ResetPipeLine(pipelines[i]);
            }
        }

        public static void Clean()
        {
            for (int i = 0; i < pipelines.Count; i++)
            {
                pipelines[i].Clean();
            }
            
            Debug.LogInfo("Cleaned up pipeline manager");
        }
    }
}