using amulware.Graphics;
using OpenTK;

namespace GenericGamedev.Pixelation
{
    struct ParticleVertex : IVertexData
    {
        private readonly Vector3 position0;
        private readonly Vector3 velocity0;
        private readonly float lifetime;

        public ParticleVertex(Vector3 position0, Vector3 velocity0, float lifetime)
        {
            this.position0 = position0;
            this.velocity0 = velocity0;
            this.lifetime = lifetime;
        }

        public VertexAttribute[] VertexAttributes()
        {
            return attributes;
        }

        public int Size()
        {
            return size;
        }

        private static readonly VertexAttribute[] attributes =
            VertexData.MakeAttributeArray(
                VertexData.MakeAttributeTemplate<Vector3>("position0"),
                VertexData.MakeAttributeTemplate<Vector3>("velocity0"),
                VertexData.MakeAttributeTemplate<float>("lifetime")
                );

        private static readonly int size = VertexData.SizeOf<ParticleVertex>();
    }
}
