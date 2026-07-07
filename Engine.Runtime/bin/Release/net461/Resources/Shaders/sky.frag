#version 330 core

in vec3 texCoords;
out vec4 fragColor;

uniform vec3 skyColorTop;
uniform vec3 skyColorHorizon;
uniform vec3 sunDir;

void main()
{
    vec3 dir = normalize(texCoords);

    // ???????? ?? ????????? ?? ??????
    float t = max(dir.y, 0.0);
    vec3  sky = mix(skyColorHorizon, skyColorTop, pow(t, 0.6));

    // ?????? (Phong-like highlight)
    float sunDot = max(dot(dir, normalize(-sunDir)), 0.0);
    float sun    = pow(sunDot, 128.0);
    float glare  = pow(sunDot, 12.0) * 0.4;
    sky += vec3(1.0, 0.9, 0.7) * (sun + glare);

    fragColor = vec4(sky, 1.0);
}
