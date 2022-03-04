using System.Collections.Generic;

namespace Apollo;

public class TaskObjects
{
    public static Dictionary<TaskType, string> TaskNames = new Dictionary<TaskType, string>()
    {
        {
            TaskType.PolusScanId, "PolusShip(Clone)/Office/panel_scanID"
        }
    };

    public enum TaskType
    {
        PolusScanId
    }
}