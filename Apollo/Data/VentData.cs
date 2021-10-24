namespace Apollo.Data
{
    public class VentData
    {
        public string Name { get; set; }
        public string ObjectName { get; set; }
        public string Type { get; set; }
        
        public bool SkeldVent()
        {
            if (Type.Equals("Skeld"))
                return true;
            return false;
        }
    }
}