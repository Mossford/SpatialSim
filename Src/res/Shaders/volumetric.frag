#version 450

layout(location = 0) out vec4 outColor;

layout(set = 2) uniform UniformBufferObject
{
    mat4 proj;
    mat4 view;
    vec3 camPos;
    float time;
    vec4 res;
} ubo;

float RaySphereIntersection(vec3 center, float radius, vec3 rayDir)
{
    vec3 oc = ubo.camPos.xyz - center;

    float a = dot(rayDir, rayDir);
    float b = 2.0 * dot(oc, rayDir);
    float c = dot(oc, oc) - radius * radius;

    float discriminant = b * b - 4.0 * a * c;

    if (discriminant < 0.0)
    return -1.0;

    return (-b - sqrt(discriminant)) / (2.0 * a);
}

void main()
{
    vec2 fragPos = gl_FragCoord.xy / ubo.res.xy * 2.0 - 1.0;
    vec4 target = inverse(ubo.proj) * vec4(fragPos, 1.0, 1.0);
    target /= target.w;
    vec3 rayDir = normalize((inverse(ubo.view) * vec4(target.xyz, 0.0)).xyz);

    float t = RaySphereIntersection(vec3(20 * sin(ubo.time), 0.0, 20), 0.5, rayDir);

    if (t > 0.0)
        outColor = vec4(vec3(t * 0.02), 1.0);
    else
         discard;
    
    float gamma = 2.2;
    outColor.rgb = pow(outColor.rgb, vec3(1.0 / gamma));
}