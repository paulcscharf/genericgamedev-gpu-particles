using amulware.Graphics;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.Math;
using OpenTK;

namespace GenericGamedev.Pixelation
{
    sealed class GPUParticles : IParticleSystem
    {
        sealed class Batch : IDeletable
        {
            private readonly BatchedVertexSurface<GPUVertex> surface;
            private readonly BatchedVertexSurface<GPUVertex>.Batch particles;
            private readonly FloatUniform timeUniform;

            private readonly float birthtime;
            private readonly float deathTime;

            public Batch(float time, BatchedVertexSurface<GPUVertex> surface, int count)
            {
                this.surface = surface;
                this.particles = surface.GetEmptyVertexBuffer();

                this.birthtime = time;

                const float minLifeTime = 1f;
                const float maxLifeTime = 2f;

                this.deathTime = time + maxLifeTime;

                ushort offset;
                var vertices = this.particles.VertexBuffer.WriteVerticesDirectly(count, out offset);

                for (int i = 0; i < count; i++)
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

                    vertices[offset + i] = new GPUVertex(Vector3.Zero, v * speed, lifetime);
                }

                this.particles.MarkAsDirty();

                this.particles.AddSetting(
                    this.timeUniform = new FloatUniform("time")
                    );
            }

            public void Update(float time)
            {
                if (time >= this.deathTime)
                    this.Delete();

                this.timeUniform.Float = time - this.birthtime;
            }

            public void Delete()
            {
                this.Deleted = true;
                this.surface.DeleteVertexBuffer(this.particles);
            }

            public bool Deleted { get; private set; }
        }

        private readonly DeletableObjectList<Batch> batches = new DeletableObjectList<Batch>();
        private readonly BatchedVertexSurface<GPUVertex> surface;
        private float time;
        private readonly int batchParticleCount;

        public GPUParticles(BatchedVertexSurface<GPUVertex> gpuSurface, float time, int batchParticleCount)
        {
            this.surface = gpuSurface;
            this.time = time;
            this.batchParticleCount = batchParticleCount;
        }

        public string Name { get { return "GPU"; } }

        public int AliveParticles { get { return this.batches.ApproximateCount * this.batchParticleCount; } }

        public void SpawnParticleGroup(int size)
        {
            this.batches.Add(new Batch(this.time, this.surface, size));
        }

        public void Update(float time)
        {
            this.time = time;
            foreach (var batch in this.batches)
            {
                batch.Update(time);
            }
        }

        public void Draw()
        {
            this.surface.Render();
        }

        public void Dispose()
        {
            foreach (var batch in this.batches)
            {
                batch.Delete();
            }
        }
    }
}
