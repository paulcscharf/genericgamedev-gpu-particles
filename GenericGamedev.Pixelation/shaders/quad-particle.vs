#version 150

uniform mat4 modelviewMatrix;
uniform mat4 projectionMatrix;

in vec3 position;
in vec2 uv;
in float alpha;

out Fragment
{
	vec2 uv;
	float alpha;
} fragment;

void main()
{
	vec4 p = modelviewMatrix * vec4(position, 1);

	p.xy += uv;

	gl_Position = projectionMatrix * p;

	fragment.uv = uv;
	fragment.alpha = alpha;
}