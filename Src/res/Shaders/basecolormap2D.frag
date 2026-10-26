#version 450
#extension GL_EXT_nonuniform_qualifier : require

layout(location = 0) out vec4 outColor;

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aUv;

layout(set = 1, binding = 0) uniform sampler2D textures[];

layout(set = 2) uniform UniformBufferObject
{
    vec3 diffuse;
    uint colorTex;
} ubo;

void main() 
{
    vec3 color = vec3(ubo.diffuse) * texture(nonuniformEXT(textures[ubo.colorTex]), aUv).rgb;
    outColor = vec4(color, 1);
}
