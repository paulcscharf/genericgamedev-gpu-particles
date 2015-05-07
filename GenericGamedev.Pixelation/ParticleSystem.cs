using amulware.Graphics;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.Math;
using OpenTK;

namespace GenericGamedev.Pixelation
{
    sealed class ParticleSystem : IDeletable
    {
        public const int Count = 1000;

        private readonly BatchedVertexSurface<ParticleVertex> surface;
        private readonly BatchedVertexSurface<ParticleVertex>.Batch particles;
        private readonly FloatUniform timeUniform;

        private readonly float birthtime;
        private readonly float deathTime;

        public ParticleSystem(float time, BatchedVertexSurface<ParticleVertex> surface)
        {
            this.surface = surface;
            this.particles = surface.GetEmptyVertexBuffer();

            this.birthtime = time;

            const float minLifeTime = 1f;
            const float maxLifeTime = 2f;

            this.deathTime = time + maxLifeTime;

            ushort offset;
            var vertices = this.particles.VertexBuffer.WriteVerticesDirectly(Count, out offset);

            for (int i = 0; i < Count; i++)
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
                
                vertices[offset + i] = new ParticleVertex(Vector3.Zero, v * speed, lifetime);
            }

            this.particles.MarkAsDirty();

            this.particles.AddSetting(
                this.timeUniform = new FloatUniform("time")
                );
        }

        public void Update(float time)
        {
            if (time >= this.deathTime)
                this.delete();

            this.timeUniform.Float = time - this.birthtime;
        }

        private void delete()
        {
            this.Deleted = true;
            this.surface.DeleteVertexBuffer(this.particles);
        }

        public bool Deleted { get; private set; }
    }
}
