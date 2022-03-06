using System.Collections.Generic;

namespace Apollo;

public class TaskObjects
{
    public static Dictionary<TaskType, string> TaskNames = new Dictionary<TaskType, string>()
    {
        {
            TaskType.OfficeSwipeCardTask, "PolusShip(Clone)/Office/panel_scanID"
        },
        {
            TaskType.MedScanTask, "PolusShip(Clone)/Science/panel_medplatform"
        }
    };

    public enum TaskType
    {
        OfficeSwipeCardTask,
        MedScanTask
    }
}