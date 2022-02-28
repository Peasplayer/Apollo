using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;

namespace Apollo
{
    public static class Extensions
    {
        public static Il2CppReferenceArray<T> Add<T>(this Il2CppReferenceArray<T> arr, T value) where T : Il2CppObjectBase
        {
            List<T> list = arr.ToList();
            list.Add(value);
            return list.ToArray();
        }
        
        public static Il2CppStringArray Add(this Il2CppStringArray arr, string value)
        {
            List<string> list = arr.ToList();
            list.Add(value);
            return list.ToArray();
        }
        
        public static Il2CppStringArray Remove(this Il2CppStringArray arr, string value)
        {
            List<string> list = arr.ToList();
            list.Remove(value);
            return list.ToArray();
        }

        public static void Clear(this ShipStatus shipStatus)
        {
            shipStatus.AllCameras = new List<SurvCamera>().ToArray();
            shipStatus.AllDoors = new List<PlainDoor>().ToArray();
            shipStatus.AllConsoles = new List<Console>().ToArray();
            shipStatus.AllRooms = new List<PlainShipRoom>().ToArray();
            shipStatus.AllStepWatchers = new List<IStepWatcher>().ToArray();
            shipStatus.AllVents = new List<Vent>().ToArray();
            shipStatus.DummyLocations = new List<Transform>().ToArray();
            shipStatus.SpecialTasks = new List<PlayerTask>().ToArray();
            //shipStatus.CommonTasks = new List<NormalPlayerTask>().ToArray();
            //shipStatus.LongTasks = new List<NormalPlayerTask>().ToArray();
            //shipStatus.NormalTasks = new List<NormalPlayerTask>().ToArray();
            shipStatus.FastRooms = new Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, PlainShipRoom>();
            shipStatus.SystemNames = new List<StringNames>().ToArray();
            //shipStatus.Systems = new Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, ISystemType>();
            //shipStatus.Systems.Add(SystemTypes.Electrical, new SwitchSystem().Cast<ISystemType>());
            //shipStatus.Systems.Add(SystemTypes.Comms, new HudOverrideSystemType().Cast<ISystemType>());
            //shipStatus.Systems.Add(SystemTypes.Laboratory, new ReactorSystemType(60f, SystemTypes.Laboratory).Cast<ISystemType>());
            //shipStatus.Systems.Add(SystemTypes.Ventilation, new VentilationSystem().Cast<ISystemType>());
            //shipStatus.Systems.Add(SystemTypes.Doors, new DoorsSystemType().Cast<ISystemType>());
            //shipStatus.Systems.Add(SystemTypes.Sabotage, new SabotageSystemType(new IActivatable[] {
                //shipStatus.Systems[SystemTypes.Electrical].Cast<IActivatable>(),
                //shipStatus.Systems[SystemTypes.Comms].Cast<IActivatable>(),
                //shipStatus.Systems[SystemTypes.Laboratory].Cast<IActivatable>()
            //}).Cast<ISystemType>());
            //shipStatus.Systems.Add(SystemTypes.Security, new SecurityCameraSystemType().Cast<ISystemType>());
            //shipStatus.Systems.Add(SystemTypes.MedBay, new MedScanSystem().Cast<ISystemType>());
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

        public static int GetID(this MovingPlatformBehaviour platform)
        {
            if (MovingPlatformHandler.Platforms.Find(item => item.PlatformBehaviour == platform) != null)
                return MovingPlatformHandler.Platforms.Find(item => item.PlatformBehaviour == platform).ID;
            return -1;
        }
    }
}