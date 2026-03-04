#version 450

layout(binding = 0) uniform UniformBufferObject
{
    mat4 view;
    mat4 proj;
    mat4 model;
} ubo;

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec3 aUv;

layout(location = 0) out vec3 aPosO;
layout(location = 1) out vec3 aNormalO;
layout(location = 2) out vec3 aUvO;

void main()
{
    aPosO = aPos;
    aNormalO = mat3(transpose(inverse(ubo.model))) * aNormal;
    aUvO = aUv;
    gl_Position = ubo.proj * ubo.view * ubo.model * vec4(aPos, 1.0);
}