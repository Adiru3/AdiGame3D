#version 330 core

in vec3 fragNormal;
in vec3 fragWorldPos;
in vec2 fragTexCoords;

out vec4 fragColor;

uniform vec3  objectColor;
uniform vec3  lightDir;
uniform vec3  lightColor;
uniform float ambientStrength;
uniform vec3  fogColor;
uniform float fogStart;
uniform float fogEnd;
uniform vec3  cameraPos;

uniform sampler2D textureMap;
uniform bool      useTexture;

void main()
{
    vec3 norm = normalize(fragNormal);

    // Texture or Flat color
    vec4 texColor = useTexture ? texture(textureMap, fragTexCoords) : vec4(objectColor, 1.0);
    vec3 baseColor = texColor.rgb;

    // Ambient
    vec3 ambient = ambientStrength * lightColor;

    // Diffuse
    float diff   = max(dot(norm, normalize(-lightDir)), 0.0);
    vec3  diffuse = diff * lightColor;

    // Rim light
    float rim = max(dot(norm, vec3(0.0, -1.0, 0.0)), 0.0) * 0.12;

    vec3 result = (ambient + diffuse + rim) * baseColor;

    // Fog
    float dist      = length(fragWorldPos - cameraPos);
    float fogFactor = clamp((fogEnd - dist) / (fogEnd - fogStart), 0.0, 1.0);
    result = mix(fogColor, result, fogFactor);

    float a = useTexture ? texColor.a : 1.0;

    fragColor = vec4(result, a);
}
