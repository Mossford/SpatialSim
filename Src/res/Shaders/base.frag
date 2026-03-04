#version 450

layout(location = 0) out vec4 outColor;

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec3 aUv;

void main() 
{
    outColor = vec4(aNormal, 1.0);
}
