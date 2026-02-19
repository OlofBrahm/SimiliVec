using System;

namespace VectorDataBase.Datahandling
{
    public class UmapNode
    {
        public int Id { get; set; }
        public required string DocumentId { get; set; }
        public required string Content { get; set; }
        public float[] ReducedVector { get; set; } = Array.Empty<float>();
    }
}

