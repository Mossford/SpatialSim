using System.Numerics;
using SpatialSim.Engine.Core;
using SpatialSim.Game.Math;

namespace SpatialSim.Engine.Rendering
{
    public class PostProcessManager
    {
        //TODO This might need copies of at least the textures store for the swapchain depending on how the sync works
        
        public static SortedList<int, PostProcessEffect> postProcesses;
        //TODO should remove this? and replace with quad in shader
        public static MeshData quad;
        public static Buffer<Vertex> vertexBuffer;
        public static Buffer<int> indexBuffer;
        
        public static void Init()
        {
            postProcesses = new SortedList<int, PostProcessEffect>();
            quad = MeshGeneration.Create2DQuad();
            vertexBuffer = new Buffer<Vertex>();
            vertexBuffer.Create(new Span<Vertex>(quad.GetVertexes()), BufferUsage.Vertex, BufferMemoryUsage.Cpu);
            indexBuffer = new Buffer<int>();
            indexBuffer.Create(new Span<int>(quad.indices), BufferUsage.Index, BufferMemoryUsage.Cpu);
            
            Debug.LogInfo("Successful post process manager creation");
        }

        /// <summary>
        /// needed when window resize as imagesize might change
        /// </summary>
        public static void RecreatePostProcesses()
        {
            for (int i = 0; i < postProcesses.Count; i++)
            {
                postProcesses.GetValueAtIndex(i).Clean();
            }
            
            for (int i = 0; i < postProcesses.Count; i++)
            {
                postProcesses.GetValueAtIndex(i).Create();
            }
        }

        public static bool LoadPostProcessEffect(PostProcessEffect effect, Shader fragShader)
        {
            Shader vertShader = ShaderManager.RetrieveShader(new ShaderSettings(
                ShaderType.Vertex,
                [
                    new ShaderDescriptorDef(RendererSettings.VertexUniformSet, [0],
                        ShaderDescriptorUsage.Uniform, ShaderType.Vertex)
                ],
                "postprocess.vert"));

            if (!PipelineManager.LoadPipeline(new Pipeline(effect.pipeline)
                {
                    settings = new PipelineSettings()
                    {
                        blendColor = false,
                        depthTest = false
                    }
                }, [vertShader, fragShader]))
            {
                Debug.Warning($"Could not add post process {effect.pipeline}, pipeline failed to load");
                return false;
            }
            
            if (postProcesses.TryAdd(postProcesses.Count, effect))
            {
                postProcesses[postProcesses.Count - 1].shader = fragShader.settings.file;
                postProcesses[postProcesses.Count - 1].layer = postProcesses.Count - 1;
                return true;
            }

            Debug.Warning($"Could not add post process {effect.pipeline} possible duplicate");
            return false;
        }

        public static void Clean()
        {
            vertexBuffer.Clean();
            indexBuffer.Clean();

            for (int i = 0; i < postProcesses.Count; i++)
            {
                postProcesses.GetValueAtIndex(i).Clean();
            }
            
            Debug.LogInfo("Cleaned up post process manager");
        }
    }
}