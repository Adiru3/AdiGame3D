#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat3 normalMatrix;

out vec3 fragNormal;
out vec3 fragWorldPos;
out vec2 fragTexCoords;

void main()
{
    vec4 worldPos   = model * vec4(aPosition, 1.0);
    fragWorldPos    = worldPos.xyz;
    fragNormal      = normalize(normalMatrix * aNormal);
    fragTexCoords   = aTexCoords;
    gl_Position     = projection * view * worldPos;
}
