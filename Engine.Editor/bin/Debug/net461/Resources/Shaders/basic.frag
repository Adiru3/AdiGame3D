#version 330 core

in vec3 fragNormal;
in vec3 fragWorldPos;
in vec2 fragTexCoords;

out vec4 fragColor;

uniform vec3  objectColor;
uniform vec3  lightDir;
uniform vec3  lightColor;
uniform float ambientStrength;
uniform bool  isSelected;
uniform bool  isPreview;
uniform float alpha;

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
    vec3  rimDir = normalize(vec3(-lightDir.x * 0.5, -1.0, -lightDir.z * 0.5));
    float rim    = max(dot(norm, rimDir), 0.0) * 0.15;

    vec3 result = (ambient + diffuse + rim) * baseColor;

    // Selection highlight
    if (isSelected) {
        result = mix(result, vec3(1.0, 0.85, 0.1), 0.4);
        result += vec3(0.15, 0.1, 0.0);
    }

    // Alpha / Preview opacity
    float a = isPreview ? 0.45 : (useTexture ? texColor.a * alpha : alpha);

    fragColor = vec4(result, a);
}
