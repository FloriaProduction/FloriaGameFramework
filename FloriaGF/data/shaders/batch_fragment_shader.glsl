#version 330 core

in vec2 fs_texCoord;

uniform sampler2D fs_texture;

out vec4 color;

void main()
{
	//color = vec4(0, 0, 1, 1);

	color = texture(fs_texture, fs_texCoord);
}