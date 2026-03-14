#version 450

layout(location = 0) out vec4 outColor;

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aUv;

layout(set = 1) uniform sampler2D texSampler;

void main() 
{
    outColor = texture(texSampler, aUv);
}
