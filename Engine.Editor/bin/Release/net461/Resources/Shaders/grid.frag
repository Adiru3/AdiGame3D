#version 330 core

in vec3 worldPos;
out vec4 fragColor;

uniform vec3 cameraPos;
uniform float far;

// ?????? ????? ????? ?? XZ ????? fract
vec4 gridLine(vec2 coord, float scale)
{
    vec2 scaled    = coord * scale;
    vec2 grid      = abs(fract(scaled - 0.5) - 0.5);
    // ???????????? ????? ???????????
    vec2 dxdy      = fwidth(scaled);
    vec2 line      = grid / max(dxdy, vec2(0.0001));
    float alpha    = 1.0 - clamp(min(line.x, line.y), 0.0, 1.0);
    return vec4(0.40, 0.40, 0.44, alpha);
}

void main()
{
    float dist   = length(worldPos.xz - cameraPos.xz);
    float fading = 1.0 - clamp(dist / (far * 0.55), 0.0, 1.0);

    // ?????? ????? (??? 1) + ??????? (??? 10)
    vec4 small = gridLine(worldPos.xz, 1.0);
    vec4 large = gridLine(worldPos.xz, 0.1);
    large.rgb  = vec3(0.30, 0.30, 0.34);  // ??????? ???? ????

    // ??? X (???????) ? ????? Z=0
    float axisW = max(fwidth(worldPos.z) * 2.0, 0.015);
    if (abs(worldPos.z) < axisW)
    {
        fragColor = vec4(0.85, 0.25, 0.25, fading);
        return;
    }
    // ??? Z (?????) ? ????? X=0
    axisW = max(fwidth(worldPos.x) * 2.0, 0.015);
    if (abs(worldPos.x) < axisW)
    {
        fragColor = vec4(0.25, 0.45, 0.90, fading);
        return;
    }

    vec4 col = small + large * (1.0 - small.a);
    col.a   *= fading;

    if (col.a < 0.01) discard;
    fragColor = col;
}
