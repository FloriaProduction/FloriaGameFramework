#version 330 core


layout (location = 0) in vec3 vs_position;
layout (location = 1) in vec2 vs_texCoord;

uniform vec3 camera_position;
uniform vec3 camera_scale;

out vec2 fs_texCoord;


void main()
{
    fs_texCoord = vs_texCoord;

    gl_Position = vec4(
        (vs_position.xyz - vec3(0.0, (vs_position.z * 0.5 - camera_position.z * 0.5) * -1.0, 0.0) - camera_position.xyz) * camera_scale / vec3(0.5, -0.5, -1000.0),
        1.0
    );
}