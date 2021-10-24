namespace Apollo.Data
{
    public class CamData
    {
        public string Name { get; set; }
        public string ObjectName { get; set; }
        public string Type { get; set; }
        public Vector2 Offset { get; set; } = new (0, 0);
        public bool Flip { get; set; }
        
        public bool SkeldCam()
        {
            if (Type.Equals("Skeld"))
                return true;
            return false;
        }
    }
}