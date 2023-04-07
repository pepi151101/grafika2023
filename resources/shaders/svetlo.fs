
// Fragment shader
#version 330 core

struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
};

struct Light {
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

in vec3 FragPos;
in vec3 Normal;

out vec4 fragColor;

uniform Material material;
uniform Light light;

void main()
{
    // Ambient lighting
    vec3 ambient = light.ambient * material.ambient;

    // Diffuse lighting
    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(Normal, lightDir), 0.0);
    vec3 diffuse = light.diffuse * material.diffuse * diff;

    // Specular lighting
    vec3 viewDir = normalize(-FragPos);
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(Normal, halfwayDir), 0.0), material.shininess);
    vec3 specular = light.specular * material.specular * spec;

    fragColor = vec4(ambient + diffuse + specular, 1.0);
}
