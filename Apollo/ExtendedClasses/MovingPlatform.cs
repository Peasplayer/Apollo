using System.Collections.Generic;

namespace Apollo;

public class MovingPlatform
{
    public MovingPlatform(MovingPlatformBehaviour platformBehaviour, PlatformConsole console1, PlatformConsole console2)
    {
        ID = MovingPlatformHandler.Platforms.Count;
        PlatformBehaviour = platformBehaviour;
        Console1 = console1;
        Console2 = console2;
    }
    
    public int ID;
    public MovingPlatformBehaviour PlatformBehaviour;
    public PlatformConsole Console1;
    public PlatformConsole Console2;
}

public static class MovingPlatformHandler
{
    public static List<MovingPlatform> Platforms = new List<MovingPlatform>();

    public static MovingPlatform GetPlatform(int id)
    {
        return Platforms.Find(platform => platform.ID == id);
    }
}