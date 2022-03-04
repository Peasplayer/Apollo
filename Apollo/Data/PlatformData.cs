namespace Apollo.Data
{
    public class PlatformData
    {
        public string Name { get; set; }
        public string LeftUseObject { get; set; }
        public string RightUseObject { get; set; }
        public bool StartLeft { get; set; } = true;
        public bool ShowFan { get; set; }
    }
}