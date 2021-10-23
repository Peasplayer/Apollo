using System.Collections.Generic;

namespace Apollo.Data
{
    public class MapData
    {
        public string Name { get; set; }
        public Dictionary<string, RoomData> Rooms { get; set; }
    }
}