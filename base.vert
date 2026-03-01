#version 450

layout(location = 0) in vec3 Apos;
layout(location = 1) in vec3 Anormal;
layout(location = 2) in vec3 Auv;

void main()
{
    gl_Position = vec4(Apos.xy, 0.0, 1.0);
}