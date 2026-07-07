#version 330 core

// Infinite grid: draw a large flat quad following the camera on XZ
layout (location = 0) in vec3 aPosition;

uniform mat4 view;
uniform mat4 projection;
uniform vec3 cameraPos;

out vec3 worldPos;

void main()
{
    // Move quad with camera (XZ only, Y=0 is fixed)
    vec3 pos = aPosition + vec3(cameraPos.x, 0.0, cameraPos.z);
    worldPos    = pos;
    gl_Position = projection * view * vec4(pos, 1.0);
}
