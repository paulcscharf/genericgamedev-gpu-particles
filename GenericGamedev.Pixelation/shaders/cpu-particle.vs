#version 150

in vec3 position;
in float alpha;

out Particle
{
	vec3 position;
	float alpha;
} particle;

void main()
{
	particle.position = position;
	particle.alpha =  alpha;
}