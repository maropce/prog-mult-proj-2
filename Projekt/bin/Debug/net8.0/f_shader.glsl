#version 330 core
in vec3 fragPos;
in vec3 norm;
in vec2 uv;

uniform vec3 lightPos, lightColor;
uniform sampler2D tex0;
uniform vec3 viewPos;

out vec4 FragColor;

void main(){
    vec3 N = normalize(norm);
    vec3 L = normalize(lightPos - fragPos);
    vec3 V = normalize(-fragPos);
    vec3 R = reflect(-L, N);

    float diff = max(dot(N, L), 0.0);
    float spec = pow(max(dot(R, V),0.0), 32.0);

    vec3 matColor = texture(tex0, uv).rgb;
    vec3 color = (0.1 + diff + 0.5 * spec) * matColor * lightColor;
    FragColor = vec4(color,1.0);
}
