#version 450
#extension GL_EXT_nonuniform_qualifier : require

layout(location = 0) out vec4 outColor;

layout(set = 1, binding = 0) uniform sampler3D textures[];

layout(set = 2) uniform UniformBufferObject
{
    mat4 proj;
    mat4 view;
    vec3 camPos;
    float time;
    //resolution x, y, and fov in z
    vec3 resFov;
    uint rayMarchCount;
    vec4 aabb;
    uint sampleTex;
    uint sampleGradientTex;
} ubo;

vec2 RayAabbIntersect(vec3 rayOrigin, vec3 rayDir, vec3 boxMin, vec3 boxMax)
{
    vec3 tMin = (boxMin - rayOrigin) / rayDir;
    vec3 tMax = (boxMax - rayOrigin) / rayDir;
    vec3 t1 = min(tMin, tMax);
    vec3 t2 = max(tMin, tMax);
    float tNear = max(max(t1.x, t1.y), t1.z);
    float tFar = min(min(t2.x, t2.y), t2.z);
    return vec2(tNear, tFar);
}

vec4 RayMarchVolume(vec3 rayStart, vec3 rayEnd, vec3 boxMin, vec3 boxMax)
{
    vec3 color = vec3(0.0f);
    float density = 0.0f;
    vec3 step = (rayEnd - rayStart) / ubo.rayMarchCount;
    vec3 pos = rayStart;
    const float cutoff = 0.4f;

    for (uint i = 0; i < ubo.rayMarchCount; i++)
    {
        vec3 pointRel = (pos - boxMin) / (boxMax - boxMin);
        float texDensity = texture(nonuniformEXT(textures[ubo.sampleTex]), pointRel).r;
        if(texDensity >= cutoff)
        {
            vec3 gradient = texture(nonuniformEXT(textures[ubo.sampleGradientTex]), pointRel).rgb;
            vec3 normal = normalize(gradient * 2.0f - 1.0f);
            float dens = texDensity / float(ubo.rayMarchCount);
            color += dens * normal;
            density += dens;
        }

        pos += step;
    }

    return vec4(color, density);
}

void main()
{
    vec2 fragPos = gl_FragCoord.xy / ubo.resFov.xy * 2.0f - 1.0f;
    vec4 target = inverse(ubo.proj) * vec4(fragPos, 1.0f, 1.0f);
    target /= target.w;
    vec3 rayDir = normalize((inverse(ubo.view) * vec4(target.xyz, 0.0f)).xyz);

    vec3 boxMax = ubo.aabb.w + ubo.aabb.xyz;
    vec3 boxMin = -ubo.aabb.w + ubo.aabb.xyz;
    vec2 intersection = RayAabbIntersect(ubo.camPos.xyz, rayDir, boxMin, boxMax);
    
    if (intersection.x <= intersection.y && intersection.y > 0.0f)
    {
        vec3 rayStart = ubo.camPos.xyz + rayDir * max(intersection.x, 0.0f);
        vec3 rayEnd = ubo.camPos.xyz + rayDir * intersection.y;
        
        outColor = RayMarchVolume(rayStart, rayEnd, boxMin, boxMax);
    }
    else
    {
        discard;
    }
}