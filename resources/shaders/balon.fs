#version 330 core
out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
    vec3 TangentLightPos[4];
    vec3 TangentLightDirs[3];
    vec3 TangentViewPos;
    vec3 TangentFragPos;
} fs_in;

struct PointLight {
    vec3 position;

    vec3 specular;
    vec3 diffuse;
    vec3 ambient;

    float constant;
    float linear;
    float quadratic;
};

struct SpotLight {
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;

    float constant;
    float linear;
    float quadratic;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct Material {
    sampler2D texture_diffuse1;
    sampler2D texture_specular1;
    sampler2D normalMap;

    float shininess;
};

uniform Material material;
uniform PointLight pointLight;
uniform SpotLight spotlights[3];

uniform vec3 viewPosition;

// calculates the color when using a point light.
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir, vec3 TangentLightPos) {
    vec3 lightDir = normalize(TangentLightPos - fs_in.TangentFragPos);

    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), material.shininess);
    // attenuation
    float distance = length(light.position - fs_in.TangentFragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    // combine results
    vec3 ambient = light.ambient * vec3(texture(material.texture_diffuse1, fs_in.TexCoords));
    vec3 diffuse = light.diffuse * diff * vec3(texture(material.texture_diffuse1, fs_in.TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(material.texture_specular1, fs_in.TexCoords));
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir, vec3 TangentLightPos, vec3 TangentLightDir) {
    vec3 pos;
    vec3 dir;
    vec3 lightDir;
    float distance;
    pos = TangentLightPos;
    dir = TangentLightDir;
    lightDir = normalize(pos - fs_in.TangentFragPos);
    distance = length(pos - fs_in.TangentFragPos);

    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(halfwayDir, normal), 0.0), material.shininess);
    // attenuation
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    // spotlight intensity
    float theta = dot(lightDir, normalize(-dir));
    float epsilon = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    // combine results
    vec3 ambient = light.ambient * vec3(texture(material.texture_diffuse1, fs_in.TexCoords));
    vec3 diffuse = light.diffuse * diff * vec3(texture(material.texture_diffuse1, fs_in.TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(material.texture_specular1, fs_in.TexCoords));
    ambient *= attenuation * intensity;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;
    return (ambient + diffuse + specular);
}

void main() {
    vec3 normal = texture(material.normalMap, fs_in.TexCoords).rgb;
    normal = normalize(normal * 2.0 - 1.0);
    vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);

    vec3 result = CalcPointLight(pointLight, normal, viewDir, fs_in.TangentLightPos[0]);
        for(int i = 0; i < 3; i++) {
            result += CalcSpotLight(spotlights[i], normal, viewDir, fs_in.TangentLightPos[i+1], fs_in.TangentLightDirs[i]);
        }
   //vec3 result = texture(material.texture_diffuse1, TexCoords).rgb;
    FragColor = vec4(result, 1.0);
}

