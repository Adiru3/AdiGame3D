#version 330 core

layout (location = 0) in vec3 aPosition;

out vec3 texCoords;

uniform mat4 view;
uniform mat4 projection;

void main()
{
    texCoords   = aPosition;
    vec4 pos    = projection * mat4(mat3(view)) * vec4(aPosition, 1.0);
    gl_Position = pos.xyww; // w=w ensures sky is always at max depth
}
