#version 150

// the time since the creation of the particular particle system
uniform float time;
// constant acceleration (gravity) to apply to the particles' movement
uniform vec3 acceleration;

in vec3 position0;
in vec3 velocity0;
in float lifetime;

out Particle
{
	vec3 position;
	float alpha;
} particle;

void main()
{
	// calculate the corrent position of the particle
	particle.position = position0
				+ velocity0 * time
				+ acceleration * (time * time * 0.5);

	// calculate the opacity of the particle
	// (does not have to be clamped to 0 since we are discarding
	//  non-positive alpha particles in the geometry shader)
	particle.alpha = min(1, (time - lifetime) * -2);
}