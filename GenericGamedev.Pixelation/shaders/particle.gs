#version 150

layout (points) in;
layout (triangle_strip, max_vertices = 4) out;

uniform mat4 modelviewMatrix;
uniform mat4 projectionMatrix;

in Particle
{
	vec3 position;
	float alpha;
} particle[];

out Fragment
{
	vec2 uv;
	float alpha;
} fragment;


void main()
{
	if (particle[0].alpha <= 0)
		return;

	fragment.alpha = particle[0].alpha;

	vec4 center = modelviewMatrix * vec4(particle[0].position, 1);

	vec2 uv = vec2(-1, -1);
	vec4 p = center;
	p.xy += uv;
	fragment.uv = uv;
	gl_Position = projectionMatrix * p;
	EmitVertex();

	uv = vec2(1, -1);
	p = center;
	p.xy += uv;
	fragment.uv = uv;
	gl_Position = projectionMatrix * p;
	EmitVertex();

	uv = vec2(-1, 1);
	p = center;
	p.xy += uv;
	fragment.uv = uv;
	gl_Position = projectionMatrix * p;
	EmitVertex();

	uv = vec2(1, 1);
	p = center;
	p.xy += uv;
	fragment.uv = uv;
	gl_Position = projectionMatrix * p;
	EmitVertex();

	EndPrimitive();
}