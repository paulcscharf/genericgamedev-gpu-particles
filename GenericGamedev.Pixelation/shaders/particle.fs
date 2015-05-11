#version 150

in Fragment
{
	vec2 uv;
	float alpha;
} fragment;

out vec4 fragColor;

void main()
{
	// colour the vertex with a simple procedural circular texture
	float d = dot(fragment.uv, fragment.uv);

	if(d > 1)
		discard;

	float a = 1 - d;

	fragColor = vec4(0, 0, 0, a * fragment.alpha * 0.1);
}