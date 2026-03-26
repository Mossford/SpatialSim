#version 450

layout(location = 0) out vec4 outColor;

layout(set = 2) uniform UniformBufferObject
{
    mat4 proj;
    mat4 view;
    vec3 camPos;
    float time;
    vec3 res;
    uint rayMarchCount;
} ubo;

float RaySphereIntersection(vec3 center, float radius, vec3 rayDir)
{
    vec3 oc = ubo.camPos.xyz - center;

    float a = dot(rayDir, rayDir);
    float b = 2.0f * dot(oc, rayDir);
    float c = dot(oc, oc) - radius * radius;

    float discriminant = b * b - 4.0f * a * c;

    if (discriminant < 0.0f)
        return -1.0f;

    return (-b - sqrt(discriminant)) / (2.0f * a);
}

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

float DensityAtPoint(vec3 point, vec3 boxMin, vec3 boxMax)
{
    vec3 center = (boxMin + boxMax) * 0.5f;
    vec3 extents = (boxMax - boxMin) * 0.5f;
    vec3 rel = abs(point - center) / extents;
    
    float density = abs(sin(ubo.time) - 0.5f) / length(rel);
    density = 1.0f - density;
    return clamp(density, 0.0f, 1.0f);
}

vec3 RayMarchVolume(vec3 rayStart, vec3 rayEnd, vec3 boxMin, vec3 boxMax)
{
    float density = 0.0f;
    vec3 step = (rayEnd - rayStart) / ubo.rayMarchCount;
    vec3 pos = rayStart;

    for (uint i = 0; i < ubo.rayMarchCount; i++)
    {
        density += DensityAtPoint(pos, boxMin, boxMax) / float(ubo.rayMarchCount);
        pos += step;
    }
    
    return mix(vec3(0.2f), vec3(1, 1, 0), density) * density;
}

void main()
{
    vec2 fragPos = gl_FragCoord.xy / ubo.res.xy * 2.0f - 1.0f;
    vec4 target = inverse(ubo.proj) * vec4(fragPos, 1.0f, 1.0f);
    target /= target.w;
    vec3 rayDir = normalize((inverse(ubo.view) * vec4(target.xyz, 0.0f)).xyz);

    vec3 boxMax = vec3(2.5) + vec3(0, 0, 20);
    vec3 boxMin = vec3(-2.5) + vec3(0, 0, 20);
    vec2 intersection = RayAabbIntersect(ubo.camPos.xyz, rayDir, boxMin, boxMax);

    if (intersection.x <= intersection.y && intersection.y > 0.0f)
    {
        vec3 rayStart = ubo.camPos.xyz + rayDir * max(intersection.x, 0.0f);
        vec3 rayEnd = ubo.camPos.xyz + rayDir * intersection.y;
        
        outColor = vec4(RayMarchVolume(rayStart, rayEnd, boxMin, boxMax), 1.0f);
    }
    else
    {
        discard;
    }
    
    float gamma = 2.2;
    outColor.rgb = pow(outColor.rgb, vec3(1.0 / gamma));
}