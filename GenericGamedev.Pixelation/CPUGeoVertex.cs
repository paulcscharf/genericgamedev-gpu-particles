using amulware.Graphics;
using OpenTK;

namespace GenericGamedev.Pixelation
{
    public struct CPUGeoVertex : IVertexData
    {
        private readonly Vector3 position;
        private readonly float alpha;

        public CPUGeoVertex(Vector3 position, float alpha)
        {
            this.position = position;
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
                VertexData.MakeAttributeTemplate<float>("alpha")
                );

        private static readonly int size = VertexData.SizeOf<CPUGeoVertex>();
    }
}
