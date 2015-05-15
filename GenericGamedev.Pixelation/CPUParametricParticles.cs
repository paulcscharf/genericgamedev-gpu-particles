using System;
using System.Linq.Expressions;
using amulware.Graphics;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.Math;
using OpenTK;

namespace GenericGamedev.Pixelation
{
    sealed class CPUParametricParticles : IParticleSystem
    {
        class Particle : IDeletable
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

            private void delete()
            {
                this.Deleted = true;
            }

            public bool Deleted { get; private set; }

            public void Draw(IndexedSurface<CPUQuadVertex> surface, float time, Vector3 acceleration)
            {
                if (this.deathTime <= time)
                {
                    this.delete();
                    return;
                }

                var aliveTime = time - this.birthTime;

                var p = this.position
                        + this.velocity * aliveTime
                        + acceleration * (aliveTime.Squared() * 0.5f);

                var timeLeft = this.deathTime - time;

                var alpha = Math.Min(1, timeLeft * 2);
                surface.AddQuad(
                    new CPUQuadVertex(p, new Vector2(-1, -1), alpha),
                    new CPUQuadVertex(p, new Vector2(1, -1), alpha),
                    new CPUQuadVertex(p, new Vector2(1, 1), alpha),
                    new CPUQuadVertex(p, new Vector2(-1, 1), alpha)
                    );
            }
        }

        private readonly DeletableObjectList<Particle> particles =
            new DeletableObjectList<Particle>();

        private readonly IndexedSurface<CPUQuadVertex> surface;
        private float time;
        private readonly Vector3 acceleration;

        public CPUParametricParticles(IndexedSurface<CPUQuadVertex> surface, float time, Vector3 acceleration)
        {
            this.surface = surface;
            this.time = time;
            this.acceleration = acceleration;
        }

        public string Name { get { return "CPU Parametric"; } }

        public int AliveParticles { get { return this.particles.ApproximateCount; } }

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

            var i = 0;

            foreach (var particle in this.particles)
            {
                particle.Draw(this.surface, this.time, this.acceleration);
                i++;
                if (i == maxBatchSize)
                {
                    this.surface.Render();
                    i = 0;
                }
            }
            
            this.surface.Render();
        }

        public void Dispose()
        {
            this.particles.Clear();
        }
    }
}
