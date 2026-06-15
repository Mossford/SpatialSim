#version 450

layout(set = 0) uniform UniformBufferObject
{
    mat4 view;
    mat4 proj;
    mat4 model;
} ubo;

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec3 aTangent;
layout(location = 3) in vec3 aBiTangent;
layout(location = 4) in vec2 aUv;

layout(location = 0) out vec3 aPosO;
layout(location = 1) out vec2 aUvO;
layout(location = 2) out vec3 aNormalO;

void main()
{
    aPosO = vec3(ubo.model * vec4(aPos, 1.0f));
    aUvO = vec2(1.0f - aUv.x, aUv.y);
    aNormalO = transpose(inverse(mat3(ubo.model))) * aNormal;
    gl_Position = ubo.proj * ubo.view * vec4(aPosO, 1.0);
}