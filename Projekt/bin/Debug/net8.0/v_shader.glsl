#version 330 core
layout(location=0) in vec3 aPos;
layout(location=1) in vec3 aNorm;
layout(location=2) in vec2 aUV;

uniform mat4 P, V, M;
out vec3 fragPos;
out vec3 norm;
out vec2 uv;

void main(){
    vec4 world = M * vec4(aPos,1.0);
    fragPos = world.xyz;
    norm = mat3(transpose(inverse(M))) * aNorm;
    uv = aUV;
    gl_Position = P * V * world;
}
