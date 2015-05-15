namespace GenericGamedev.Pixelation
{
    interface IParticleSystem
    {
        string Name { get; }
        int AliveParticles { get; }

        void SpawnParticleGroup(int size);
        void Update(float time);
        void Draw();

        void Dispose();
    }

    class DummyParticleSystem : IParticleSystem
    {
        public string Name { get { return "N/A";  } }
        public int AliveParticles { get { return 0; } }
        public void SpawnParticleGroup(int size)
        {
        }

        public void Update(float deltaTime)
        {
        }

        public void Draw()
        {
        }

        public void Dispose()
        {
        }
    }
}
