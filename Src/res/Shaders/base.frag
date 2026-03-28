#version 450

layout(location = 0) out vec4 outColor;

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aUv;
layout(location = 2) in mat3 TBN;

layout(set = 1, binding = 0) uniform sampler2D texSampler;
layout(set = 1, binding = 1) uniform sampler2D normalSampler;

layout(set = 2) uniform UniformBufferObject
{
    vec4 diffuse;
    vec4 ambient;
    vec3 specular;
    float specularExp;
    vec4 viewPos;
    vec4 lightpos;
} ubo;


void main() 
{
    vec3 viewDir = normalize(ubo.viewPos.xyz - aPos);
    
    vec3 color = vec3(ubo.diffuse) * texture(texSampler, aUv).rgb;
    vec3 ambient = vec3(0.0001f) * ubo.ambient.xyz * color;

    vec3 normal = texture(normalSampler, aUv).rgb;
    normal = normal * 2.0f - 1.0f;
    normal = normalize(TBN * normal);
    
    vec3 lightDir = normalize(ubo.lightpos.xyz - aPos);
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = diff * color;
    
    vec3 halfDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfDir), 0.0), ubo.specularExp);
    vec3 specular = vec3(ubo.specular) * spec;

    vec3 result = ambient + diffuse + specular;
    float gamma = 2.2;
    outColor.rgb = pow(result, vec3(1.0/gamma));
}
