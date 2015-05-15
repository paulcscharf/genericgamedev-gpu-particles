#version 150

uniform vec3 acceleration;

in vec3 position0;
in vec3 velocity0;
in float lifetime;
in float time;

out Particle
{
	vec3 position;
	float alpha;
} particle;

void main()
{
	particle.position = position0
				+ velocity0 * time
				+ acceleration * (time * time * 0.5);

	particle.alpha = min(1, (time - lifetime) * -2);
}