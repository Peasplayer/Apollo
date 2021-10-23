using System.Collections.Generic;
using UnhollowerBaseLib;
using UnityEngine;

namespace Apollo
{
    public static class Extensions
    {
        public static Il2CppReferenceArray<T> Add<T>(this Il2CppReferenceArray<T> arr, T value) where T : Il2CppObjectBase
        {
            List<T> list = new List<T>(arr);
            list.Add(value);
            return list.ToArray();
        }

        public static void Clear(this ShipStatus shipStatus)
        {
            shipStatus.AllCameras = new UnhollowerBaseLib.Il2CppReferenceArray<SurvCamera>(0);
            shipStatus.AllDoors = new UnhollowerBaseLib.Il2CppReferenceArray<PlainDoor>(0);
            shipStatus.AllConsoles = new UnhollowerBaseLib.Il2CppReferenceArray<Console>(0);
            shipStatus.AllRooms = new UnhollowerBaseLib.Il2CppReferenceArray<PlainShipRoom>(0);
            shipStatus.AllStepWatchers = new UnhollowerBaseLib.Il2CppReferenceArray<IStepWatcher>(0);
            shipStatus.AllVents = new UnhollowerBaseLib.Il2CppReferenceArray<Vent>(0);
            shipStatus.DummyLocations = new UnhollowerBaseLib.Il2CppReferenceArray<Transform>(0);
            shipStatus.SpecialTasks = new UnhollowerBaseLib.Il2CppReferenceArray<PlayerTask>(0);
            shipStatus.CommonTasks = new UnhollowerBaseLib.Il2CppReferenceArray<NormalPlayerTask>(0);
            shipStatus.LongTasks = new UnhollowerBaseLib.Il2CppReferenceArray<NormalPlayerTask>(0);
            shipStatus.NormalTasks = new UnhollowerBaseLib.Il2CppReferenceArray<NormalPlayerTask>(0);
            shipStatus.FastRooms = new Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, PlainShipRoom>();
            shipStatus.SystemNames = new UnhollowerBaseLib.Il2CppStructArray<StringNames>(0);
            shipStatus.Systems = new Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, ISystemType>();
            shipStatus.Systems.Add(SystemTypes.Electrical, new SwitchSystem().Cast<ISystemType>());
            shipStatus.Systems.Add(SystemTypes.Comms, new HudOverrideSystemType().Cast<ISystemType>());
            shipStatus.Systems.Add(SystemTypes.Laboratory, new ReactorSystemType(60f, SystemTypes.Laboratory).Cast<ISystemType>());
            shipStatus.Systems.Add(SystemTypes.Doors, new DoorsSystemType().Cast<ISystemType>());
            shipStatus.Systems.Add(SystemTypes.Sabotage, new SabotageSystemType(new IActivatable[] {
                shipStatus.Systems[SystemTypes.Electrical].Cast<IActivatable>(),
                shipStatus.Systems[SystemTypes.Comms].Cast<IActivatable>(),
                shipStatus.Systems[SystemTypes.Laboratory].Cast<IActivatable>()
            }).Cast<ISystemType>());
            shipStatus.Systems.Add(SystemTypes.Security, new SecurityCameraSystemType().Cast<ISystemType>());
            shipStatus.Systems.Add(SystemTypes.MedBay, new MedScanSystem().Cast<ISystemType>());
        }
        public static Vector3 SetX(this Transform transform, float x)
        {
            var position = transform.position;
            var original = position;
            position = new Vector3(x, original.y, original.z);
            transform.position = position;
            return position;
        }

        public static Vector3 SetY(this Transform transform, float y)
        {
            var position = transform.position;
            var original = position;
            position = new Vector3(original.x, y, original.z);
            transform.position = position;
            return position;
        }

        public static Vector3 SetZ(this Transform transform, float z)
        {
            var position = transform.position;
            var original = position;
            position = new Vector3(original.x, original.y, z);
            transform.position = position;
            return position;
        }
    }
}