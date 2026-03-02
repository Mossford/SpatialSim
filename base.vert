#version 450

layout(binding = 0) uniform UniformBufferObject
{
    mat4 model;
    mat4 view;
    mat4 proj;
} ubo;

layout(location = 0) in vec3 Apos;
layout(location = 1) in vec3 Anormal;
layout(location = 2) in vec3 Auv;

void main()
{
    gl_Position = ubo.proj * ubo.view * ubo.model * vec4(Apos, 1.0);
}