using System.Collections.Generic;

namespace Apollo;

public class MovingPlatform
{
    public MovingPlatform(MovingPlatformBehaviour platformBehaviour)
    {
        ID = MovingPlatformHandler.Platforms.Count;
        PlatformBehaviour = platformBehaviour;
    }
    
    public int ID;
    public MovingPlatformBehaviour PlatformBehaviour;
}

public static class MovingPlatformHandler
{
    public static List<MovingPlatform> Platforms = new List<MovingPlatform>();

    public static MovingPlatform GetPlatform(int id)
    {
        return Platforms.Find(platform => platform.ID == id);
    }
}