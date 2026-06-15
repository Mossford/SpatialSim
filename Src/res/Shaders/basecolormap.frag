#version 450
#extension GL_EXT_nonuniform_qualifier : require

layout(location = 0) out vec4 outColor;

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aUv;
layout(location = 2) in vec3 aNormal;

layout(set = 1, binding = 0) uniform sampler2D textures[];

layout(set = 2) uniform UniformBufferObject
{
    vec4 diffuse;
    vec4 ambient;
    vec4 specular;
    vec3 viewPos;
    uint colorTex;
    vec4 lightpos;
} ubo;

void main() 
{
    vec3 viewDir = normalize(ubo.viewPos.xyz - aPos);
    
    vec3 color = vec3(ubo.diffuse) * texture(nonuniformEXT(textures[ubo.colorTex]), aUv).rgb;
    vec3 ambient = vec3(0.0001f) * ubo.ambient.xyz * color;
    
    vec3 lightDir = normalize(ubo.lightpos.xyz - aPos);
    float diff = max(dot(aNormal, lightDir), 0.0);
    vec3 diffuse = diff * color;
    
    vec3 halfDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(aNormal, halfDir), 0.0), ubo.specular.w);
    vec3 specular = vec3(ubo.specular) * spec;

    vec3 result = ambient + diffuse + specular;
    outColor.rgb = result;
}
