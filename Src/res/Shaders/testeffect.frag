#version 450
#extension GL_EXT_nonuniform_qualifier : require

layout(set = 1, binding = 0) uniform sampler2D textures[];

layout(location = 0) in vec2 aUvO;

layout(location = 0) out vec4 outColor;

void main()
{
    vec2 texelSize = 1.0 / textureSize(nonuniformEXT(textures[0]), 0);
    
    int radius = 5;
    vec3 color = vec3(0.0);
    
    for (int x = -radius; x <= radius; x++)
    {
        for (int y = -radius; y <= radius; y++)
        {
            vec2 offset = vec2(float(x), float(y)) * texelSize;
            color += texture(nonuniformEXT(textures[0]), aUvO + offset).rgb;
        }
    }
    
    color /= (2 * radius + 1) * (2 * radius + 1);
    outColor.rgb = color;
}
