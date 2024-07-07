#version 330 core

in vec2 fs_texCoord;

uniform sampler2D fs_texture;

out vec4 color;

void main()
{
	color = texture(fs_texture, fs_texCoord);
}