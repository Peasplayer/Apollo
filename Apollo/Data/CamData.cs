namespace Apollo.Data
{
    public class CamData
    {
        public string Name { get; set; }
        public string ObjectName { get; set; }
        public Vector2 Offset { get; set; } = new Vector2(0, 0);
        public bool Flip { get; set; }
    }
}