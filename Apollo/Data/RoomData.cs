﻿namespace Apollo.Data
{
    public class RoomData
    {
        public string ObjectName { get; set; }
        public SimpleObjectData[] SimpleObjects { get; set; }
        public VentData[] Vents { get; set; }
        public CamData[] Cams { get; set; }
        public LadderData[] Ladders { get; set; }
        public PlatformData[] Platforms { get; set; }
        public TaskData[] Tasks { get; set; }
    }
}