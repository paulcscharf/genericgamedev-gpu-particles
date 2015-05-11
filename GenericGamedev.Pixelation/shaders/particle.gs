#version 150

// we take single points as input
layout (points) in;
// we emit a triangle strip with up to 4 vertices per shader instance
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
	// we discard particles that have already faded out
	if (particle[0].alpha <= 0)
		return;

	// assign alpha to output
	fragment.alpha = particle[0].alpha;

	// transform the center of the particle into camera space
	vec4 center = modelviewMatrix * vec4(particle[0].position, 1);

	// emit the four vertices
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

	// emitting the primitive excplicitely is optional,
	// but I like to include it to make it clear when
	// geometry is created
	EndPrimitive();
}