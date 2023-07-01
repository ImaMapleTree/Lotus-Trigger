using System.Collections.Generic;
using HarmonyLib;
using LotusTrigger.Options;
using LotusTrigger.Options.General;
using VentLib.Logging;

namespace LotusTrigger.Patches.Systems;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.AddTasksFromList))]
class TaskAssignPatch
{
    public static void Prefix(ShipStatus __instance, [HarmonyArgument(4)] Il2CppSystem.Collections.Generic.List<NormalPlayerTask> unusedTasks)
    {
        if (!GameplayOptions.Instance.DisableTasks) return;
        List<NormalPlayerTask> disabledTasks = new();
        for (int i = 0; i < unusedTasks.Count; i++)
        {
            var task = unusedTasks[i];
            switch (task.TaskType)
            {
                case TaskTypes.SwipeCard when GameplayOptions.Instance.DisabledTaskFlag.HasFlag(DisabledTask.CardSwipe):
                case TaskTypes.SubmitScan when GameplayOptions.Instance.DisabledTaskFlag.HasFlag(DisabledTask.MedScan):
                case TaskTypes.UnlockSafe when GameplayOptions.Instance.DisabledTaskFlag.HasFlag(DisabledTask.UnlockSafe):
                case TaskTypes.UploadData when GameplayOptions.Instance.DisabledTaskFlag.HasFlag(DisabledTask.UploadData):
                case TaskTypes.StartReactor when GameplayOptions.Instance.DisabledTaskFlag.HasFlag(DisabledTask.StartReactor):
                case TaskTypes.ResetBreakers when GameplayOptions.Instance.DisabledTaskFlag.HasFlag(DisabledTask.ResetBreaker):
                    disabledTasks.Add(task);
                    break;
                case TaskTypes.FixWiring when GameplayOptions.Instance.DisabledTaskFlag.HasFlag(DisabledTask.FixWiring):
                    if (disabledTasks.Count + 1 < unusedTasks.Count) disabledTasks.Add(task);
                    break;
            }
        }
        foreach (var task in disabledTasks)
        {
            VentLogger.Debug("Disabling Task: " + task.TaskType, "DisableTasks");
            unusedTasks.Remove(task);
        }
    }
}