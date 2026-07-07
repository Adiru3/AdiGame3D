#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 fragNormal;
out vec3 fragWorldPos;

void main()
{
    vec4 worldPos   = model * vec4(aPosition, 1.0);
    fragWorldPos    = worldPos.xyz;
    fragNormal      = normalize(mat3(transpose(inverse(model))) * aNormal);
    gl_Position     = projection * view * worldPos;
}
