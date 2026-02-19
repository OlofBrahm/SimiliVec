namespace VectorDataBase.Datahandling
{
    public class UmapNode
    {
        public int Id { get; set; }
        public required string DocumentId { get; set; }
        public required string Content { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}

