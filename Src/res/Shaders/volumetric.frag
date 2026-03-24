#version 450

layout(location = 0) out vec4 outColor;

in vec4 gl_FragCoord;

layout(set = 2) uniform UniformBufferObject
{
    vec3 resTime;
} ubo;

void main()
{
    vec2 fragPos = (gl_FragCoord.xy / ubo.resTime.xy) * 2.0 - 1.0;
    vec4 target = vec4(fragPos, 1.0, 1.0);
    vec3 rayDir = vec3(vec4(normalize(vec3(target) / target.w), 0.0));
    
    fragPos *= vec2(16.0 / 9.0, 1.0);
    if(fragPos.x * fragPos.x + fragPos.y * fragPos.y < 0.5 * cos(fragPos.x * 30 + ubo.resTime.z))
            discard;
    
    outColor = vec4(fragPos, 0.0, 0.0);
    float gamma = 2.2;
    outColor.rgb = pow(outColor.rgb, vec3(1.0/gamma));
}
