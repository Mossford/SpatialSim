#version 450

// Good reference https://www.shadertoy.com/view/fl2Bzd


layout(location = 0) out vec4 outColor;

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

vec2 GetAnglesFromVector(vec3 vector)
{
    return vec2(atan(vector.x, vector.y), atan(vector.x, vector.z));
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
    
    float density = length(rel);
    return clamp(1.0f - density, 0.0f, 1.0f);
}

vec4 RayMarchVolume(vec3 rayStart, vec3 rayEnd, vec3 boxMin, vec3 boxMax)
{
    float density = 0.0f;
    vec3 step = (rayEnd - rayStart) / ubo.rayMarchCount;
    vec3 pos = rayStart;

    for (uint i = 0; i < ubo.rayMarchCount; i++)
    {
        density += DensityAtPoint(pos, boxMin, boxMax) / float(ubo.rayMarchCount);
        pos += step;
    }
    
    return vec4(density, density, 0, density);
}

vec4 hash43x(vec3 p)
{
    uvec3 x = uvec3(ivec3(p));
    x = 1103515245U*((x.xyz >> 1U)^(x.yzx));
    uint h = 1103515245U*((x.x^x.z)^(x.y>>3U));
    uvec4 rz = uvec4(h, h*16807U, h*48271U, h*69621U); //see: http://random.mat.sbg.ac.at/results/karl/server/node4.html
    return vec4((rz >> 1) & uvec4(0x7fffffffU))/float(0x7fffffff);
}


//Taken and edited from https://www.shadertoy.com/view/fl2Bzd
vec3 stars(vec3 dir)
{
    vec3 color = vec3(0);
    //This is the possibly? radius of the "constellations"
    float rad = 0.087f * ubo.resFov.y;
    //density of the stars
    float dens = 0.15f;
    float id = 0.0f;
    float rz = 0.0f;
    float z = 1.0f;
    
    //how many constellations we have? seems to be 3 for 1
    for (int i = 0; i < 5; i++)
    {
        dir *= mat3(0.86564, -0.28535, 0.41140, 0.50033, 0.46255, -0.73193, 0.01856, 0.83942, 0.54317);
        
        vec3 absDir = abs(dir);
        vec3 p2 = dir / max(absDir.x, max(absDir.y, absDir.z));
        p2 *= rad;
        vec3 ip = floor(p2 + 1e-5);
        vec3 fp = fract(p2 + 1e-5);
        vec4 rand = hash43x(ip * 283.1f);
        vec3 q2 = abs(p2);
        vec3 pl = 1.0 - step(max(q2.x, max(q2.y, q2.z)), q2);
        vec3 pp = fp - ((rand.xyz - 0.5f) * 0.6f + 0.5f) * pl; //don't displace points away from the cube faces
        float pr = length(ip) - rad;
        if (rand.w > (dens - dens * pr * 0.035f))
            pp += 1e6;

        float d = dot(pp, pp);
        d /= pow(fract(rand.w * 172.1f), 32.0f) + 0.25f;
        float bri = dot(rand.xyz * (1.0f - pl), vec3(1)); //since one random value is unused to displace, we can reuse
        id = fract(rand.w * 101.0f);
        color += bri * z * 0.00009f / pow(d + 0.025f, 3.0f) * (mix(vec3(1.0f, 0.45f, 0.1f), vec3(0.45f, 0.55f, 1.0), id) * 0.6f + 0.4f);

        rad = floor(rad * 1.08f);
        dens *= 1.45;
        //decreases the stars brightness each time
        z *= 0.6;
        dir = dir.yxz;
    }

    return color;
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