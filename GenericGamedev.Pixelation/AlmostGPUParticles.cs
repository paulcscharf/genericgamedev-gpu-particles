using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.Utilities;
using Bearded.Utilities.Math;
using OpenTK;

namespace GenericGamedev.Pixelation
{
    sealed class AlmostGPUParticles : IParticleSystem
    {
        struct Particle
        {
            private readonly Vector3 position;
            private readonly Vector3 velocity;
            private readonly float birthTime;
            private readonly float deathTime;

            public Particle(Vector3 position, Vector3 velocity, float time, float lifeTime)
            {
                this.position = position;
                this.velocity = velocity;
                this.birthTime = time;
                this.deathTime = time + lifeTime;
            }

            public bool IsDead(float time)
            {
                return time > this.deathTime;
            }

            public void Draw(AlmostGPUVertex[] vertices, int offset, float time)
            {
                vertices[offset] = new AlmostGPUVertex(this.position, this.velocity,
                    this.deathTime - this.birthTime, time - this.birthTime);
            }
        }

        private readonly List<Particle> particles = new List<Particle>();

        private readonly VertexSurface<AlmostGPUVertex> surface;
        private float time;

        public AlmostGPUParticles(VertexSurface<AlmostGPUVertex> surface, float time)
        {
            this.surface = surface;
            this.time = time;
        }

        public string Name { get { return "Almost GPU Particles"; } }

        public int AliveParticles { get { return this.particles.Count; } }

        public void SpawnParticleGroup(int size)
        {
            const float minLifeTime = 1f;
            const float maxLifeTime = 2f;

            for (int i = 0; i < size; i++)
            {
                var angle1 = StaticRandom.Float(GameMath.TwoPi);
                var angle2 = StaticRandom.Float(1f, GameMath.Pi - 1f);

                float cos = GameMath.Cos(angle2);

                var v = new Vector3(
                    GameMath.Cos(angle1) * cos,
                    GameMath.Sin(angle1) * cos,
                    GameMath.Sin(angle2)
                    );

                var speed = StaticRandom.Float(80, 120);

                var lifetime = StaticRandom.Float(minLifeTime, maxLifeTime);

                this.particles.Add(new Particle(Vector3.Zero, v * speed, this.time, lifetime));
            }
        }

        public void Update(float time)
        {
            this.time = time;
        }

        public void Draw()
        {
            const int maxBatchSize = 16383;


            this.particles.RemoveAll(p => p.IsDead(this.time));

            var particlesLeft = this.particles.Count;

            var particlesWritten = 0;
            while (particlesLeft > 0)
            {
                var batchSize = Math.Min(particlesLeft, maxBatchSize);

                ushort offset;
                var vertices = this.surface.WriteVerticesDirectly(batchSize, out offset);

                for (int j = 0; j < batchSize; j++)
                {
                    this.particles[particlesWritten + j]
                        .Draw(vertices, offset + j, this.time);
                }

                this.surface.Render();

                particlesWritten += batchSize;
                particlesLeft -= batchSize;
            }

        }

        public void Dispose()
        {
            this.particles.Clear();
        }
    }
}
