using amulware.Graphics;
using OpenTK;

namespace GenericGamedev.Pixelation
{
    public struct CPUQuadVertex : IVertexData
    {
        private readonly Vector3 position;
        private readonly Vector2 uv;
        private readonly float alpha;

        public CPUQuadVertex(Vector3 position, Vector2 uv, float alpha)
        {
            this.position = position;
            this.uv = uv;
            this.alpha = alpha;
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
                VertexData.MakeAttributeTemplate<Vector3>("position"),
                VertexData.MakeAttributeTemplate<Vector2>("uv"),
                VertexData.MakeAttributeTemplate<float>("alpha")
                );

        private static readonly int size = VertexData.SizeOf<CPUQuadVertex>();
    }
}
