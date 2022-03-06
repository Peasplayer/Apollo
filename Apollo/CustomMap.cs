using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Apollo.Data;
using Apollo.ExtendedClasses;
using BepInEx.IL2CPP.Utils.Collections;
using HarmonyLib;
using Reactor;
using Reactor.Extensions;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;
using Vector2 = UnityEngine.Vector2;

namespace Apollo
{
    public class CustomMap
    {
        public static bool UseCustomMap;

        public static GameObject Map;
        public static GameObject MapPrefab;
        public static Sprite MapLogo;
        public static MapData MapData;

        public static Dictionary<SimpleObjectType, GameObject> SimpleObjectPrefabs =
            new Dictionary<SimpleObjectType, GameObject>();

        public static Vent SkeldVentPrefab;
        public static Vent PolusVentPrefab;
        public static SurvCamera SkeldCamPrefab;
        public static SurvCamera PolusCamPrefab;
        public static GameObject ShortLadderPrefab;
        public static GameObject LongLadderPrefab;
        public static MovingPlatformBehaviour PlatformPrefab;
        public static PlatformConsole PlatformConsolePrefab;

        public static Dictionary<TaskObjects.TaskType, GameObject> TaskPrefabs =
            new Dictionary<TaskObjects.TaskType, GameObject>();
        public static int CurrentConsoleId;
        public static List<NormalPlayerTask> AllCommonTasks = new List<NormalPlayerTask>();
        public static List<NormalPlayerTask> AllLongTasks = new List<NormalPlayerTask>();
        public static List<NormalPlayerTask> AllNormalTasks = new List<NormalPlayerTask>();
        public static List<NormalPlayerTask> CommonTasks = new List<NormalPlayerTask>();
        public static List<NormalPlayerTask> LongTasks = new List<NormalPlayerTask>();
        public static List<NormalPlayerTask> NormalTasks = new List<NormalPlayerTask>();

        public static List<Ladder> AllLadders = new List<Ladder>();
        public static byte CurrentLadderId;

        public static IEnumerator CoSetupMap(ShipStatus ship)
        {
            yield return Reset();
            ship.InitialSpawnCenter =
                ship.MeetingSpawnCenter =
                    ship.MeetingSpawnCenter2 =
                        Map.transform.FindChild("[SPAWN]").transform.position;
            Map.transform.FindChild("[SPAWN]").gameObject.Destroy();
            Map.transform.SetZ(2);

            if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                Coroutines.Start(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black, 0f).WrapToManaged());

            if (Map.transform.FindChild("Background") == null)
            {
                var background = new GameObject("Background");
                background.transform.SetParent(Map.transform);
                background.transform.position = new Vector3(0f, 0f, 100f);
                var texture = Texture2D.grayTexture;
                var rend = background.AddComponent<SpriteRenderer>();
                rend.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), 0.01f);
            }

            yield return CoCreatePrefabs();

            foreach (var roomData in MapData.Rooms)
            {
                var roomObject = Map.transform.FindChild(roomData.Value.ObjectName).gameObject;
                var roomGround = roomObject.transform.FindChild("Ground");
                roomObject.name = roomData.Key;
                roomObject.transform.SetZ(1);
                roomGround.SetZ(0.999f);
                roomObject.transform.FindChild("Room").SetZ(0.999f);

                var room = roomObject.transform.FindChild("Room").gameObject.AddComponent<PlainShipRoom>();
                room.roomArea = room.transform.FindChild("AreaCollider").GetComponent<PolygonCollider2D>();
                room.RoomId = (SystemTypes)ship.AllRooms.Count + 1;

                Patches.RegisterCustomRoomName(room.RoomId, roomData.Key);

                ship.AllRooms = ship.AllRooms.Add(room);
                ship.FastRooms.Add(room.RoomId, room);

                if (roomData.Value.SimpleObjects != null)
                    roomData.Value.SimpleObjects.Do(data => CreateSimpleObject(roomGround, data));

                if (roomData.Value.Vents != null)
                    roomData.Value.Vents.Do(data => CreateVent(roomGround, data));

                if (roomData.Value.Cams != null)
                    roomData.Value.Cams.Do(data => CreateCam(roomGround, data));

                if (roomData.Value.Ladders != null)
                    roomData.Value.Ladders.Do(data => CreateLadder(roomGround, data));

                if (roomData.Value.Platforms != null)
                    roomData.Value.Platforms.Do(data => CreatePlatform(roomGround, data));
                
                if (roomData.Value.Tasks != null)
                    roomData.Value.Tasks.Do(data => CreateTask(roomGround, room, data));

                Logger<ApolloPlugin>.Info("ADDED " + roomData.Key);
            }
            
            ship.CommonTasks = CommonTasks.ToArray();
            ship.LongTasks = LongTasks.ToArray();
            ship.NormalTasks = NormalTasks.ToArray();
            try
            {
                ship.Begin();
            }
            catch(Exception e)
            {
                Logger<ApolloPlugin>.Error("Couldn't assign tasks: " + e);
            }

            Patches.ShipStatusAwakeCount = Patches.ShipStatusStartCount = 0;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                ship.SpawnPlayer(player, GameData.Instance.PlayerCount,
                    AmongUsClient.Instance.GameMode != GameModes.FreePlay);
            }

            for (int i = 0; i < ship.transform.childCount; i++)
            {
                var child = ship.transform.GetChild(i);
                child.gameObject.SetActive(false);
            }

            if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                yield return HudManager.Instance.CoFadeFullScreen(Color.black, Color.clear, 0f);

            yield return DestroyPrefabs();
        }

        public static IEnumerator CoCreatePrefabs()
        {
            var ship = ShipStatus.Instance;

            var polusCamPrefab = Object.Instantiate(ship.GetComponentInChildren<SurvCamera>());
            polusCamPrefab.name = "PolusCamPrefab";
            polusCamPrefab.NewName = StringNames.ExitButton;
            polusCamPrefab.gameObject.SetActive(false);
            PolusCamPrefab = polusCamPrefab;

            var polusVentPrefab = Object.Instantiate(ship.GetComponentInChildren<Vent>());
            polusVentPrefab.name = "PolusVentPrefab";
            polusVentPrefab.Left = null;
            polusVentPrefab.Right = null;
            polusVentPrefab.Center = null;
            polusVentPrefab.gameObject.SetActive(false);
            PolusVentPrefab = polusVentPrefab;

            var emergencyButtonPrefab = ship.transform.FindChild("Office").FindChild("caftable")
                .FindChild("EmergencyButton").gameObject;
            emergencyButtonPrefab.name = "EmergencyButtonPrefab";
            emergencyButtonPrefab.SetActive(false);
            SimpleObjectPrefabs.Add(SimpleObjectType.EmergencyButton, emergencyButtonPrefab);

            var laptopPrefab = ship.transform.FindChild("Office").FindChild("caftable").FindChild("TaskAddConsole");
            if (laptopPrefab != null)
            {
                laptopPrefab.name = "CustomizeLaptopPrefab";
                laptopPrefab.gameObject.SetActive(false);
                SimpleObjectPrefabs.Add(SimpleObjectType.TaskLaptop, laptopPrefab.gameObject);
            }

            var securityCamsPanelPrefab = ship.transform.FindChild("Electrical").FindChild("Surv_Panel").gameObject;
            securityCamsPanelPrefab.name = "SecurityCamsPanelPrefab";
            securityCamsPanelPrefab.SetActive(false);
            SimpleObjectPrefabs.Add(SimpleObjectType.SecurityCamsPanel, securityCamsPanelPrefab);

            var vitalsPrefab = ship.transform.FindChild("Office").FindChild("panel_vitals").gameObject;
            vitalsPrefab.name = "VitalsPrefab";
            vitalsPrefab.SetActive(false);
            SimpleObjectPrefabs.Add(SimpleObjectType.Vitals, vitalsPrefab);
            
            AllCommonTasks.AddRange(ship.CommonTasks);
            AllLongTasks.AddRange(ship.LongTasks);
            AllNormalTasks.AddRange(ship.NormalTasks);

            AsyncOperationHandle<GameObject> skeldPrefabOperation =
                AmongUsClient.Instance.ShipPrefabs.ToArray()[0].InstantiateAsync(null, false);
            while (!skeldPrefabOperation.IsDone) yield return null;

            AsyncOperationHandle<GameObject> miraPrefabOperation =
                AmongUsClient.Instance.ShipPrefabs.ToArray()[1].InstantiateAsync(null, false);
            while (!miraPrefabOperation.IsDone) yield return null;

            AsyncOperationHandle<GameObject> airshipPrefabOperation =
                AmongUsClient.Instance.ShipPrefabs.ToArray()[4].InstantiateAsync(null, false);
            while (!airshipPrefabOperation.IsDone) yield return null;

            foreach (var pair in TaskObjects.TaskNames)
            {
                var task = GameObject.Find(pair.Value);
                if (task == null)
                    continue;
                var prefab = Object.Instantiate(task);
                prefab.SetActive(false);
                TaskPrefabs.Add(pair.Key, prefab);
            }
            
            var skeldPrefab = skeldPrefabOperation.Result;

            var skeldCamPrefab = Object.Instantiate(skeldPrefab.GetComponentInChildren<SurvCamera>());
            skeldCamPrefab.name = "SkeldCamPrefab";
            skeldCamPrefab.NewName = StringNames.ExitButton;
            skeldCamPrefab.gameObject.SetActive(false);
            SkeldCamPrefab = skeldCamPrefab;

            var skeldVentPrefab = Object.Instantiate(skeldPrefab.GetComponentInChildren<Vent>());
            skeldVentPrefab.name = "SkeldVentPrefab";
            skeldVentPrefab.Left = null;
            skeldVentPrefab.Right = null;
            skeldVentPrefab.Center = null;
            skeldVentPrefab.gameObject.SetActive(false);
            SkeldVentPrefab = skeldVentPrefab;
            
            AllCommonTasks.AddRange(skeldPrefab.GetComponent<ShipStatus>().CommonTasks);
            AllLongTasks.AddRange(skeldPrefab.GetComponent<ShipStatus>().LongTasks);
            AllNormalTasks.AddRange(skeldPrefab.GetComponent<ShipStatus>().NormalTasks);

            var miraPrefab = miraPrefabOperation.Result;

            var miraEmergencyButtonPrefab = Object.Instantiate(miraPrefab.transform.FindChild("Cafe").FindChild("Table")
                .FindChild("EmergencyConsole").gameObject);
            miraEmergencyButtonPrefab.name = "MiraEmergencyButtonPrefab";
            miraEmergencyButtonPrefab.SetActive(false);
            SimpleObjectPrefabs.Add(SimpleObjectType.MiraEmergencyButton, miraEmergencyButtonPrefab);
            
            AllCommonTasks.AddRange(miraPrefab.GetComponent<ShipStatus>().CommonTasks);
            AllLongTasks.AddRange(miraPrefab.GetComponent<ShipStatus>().LongTasks);
            AllNormalTasks.AddRange(miraPrefab.GetComponent<ShipStatus>().NormalTasks);

            var airshipPrefab = airshipPrefabOperation.Result;

            var shortLadderPrefab =
                Object.Instantiate(
                    airshipPrefab.GetComponentInChildren<Ladder>().transform.parent.gameObject);
            var shortLadderTop = shortLadderPrefab.GetComponentsInChildren<Ladder>()
                .FirstOrDefault(ladder => ladder.IsTop);
            var shortLadderBottom = shortLadderPrefab.GetComponentsInChildren<Ladder>()
                .FirstOrDefault(ladder => !ladder.IsTop);

            shortLadderPrefab.name = "LadderPrefab";
            shortLadderTop.name = "LadderTop";
            shortLadderTop.Destination = shortLadderBottom;
            shortLadderBottom.name = "LadderBottom";
            shortLadderBottom.Destination = shortLadderTop;

            var pos = new Vector3(0f, 0f);
            shortLadderPrefab.transform.position = pos - new Vector3(0f, 1f);
            shortLadderBottom.transform.position = pos - new Vector3(0f, 2.78f);
            shortLadderTop.transform.position = pos;
            shortLadderPrefab.SetActive(false);
            ShortLadderPrefab = shortLadderPrefab;

            var longLadderPrefab =
                Object.Instantiate(
                    airshipPrefab.transform.FindChild("MeetingRoom").FindChild("ladder_meeting").gameObject);
            var longLadderTop = longLadderPrefab.GetComponentsInChildren<Ladder>()
                .FirstOrDefault(ladder => ladder.IsTop);
            var longLadderBottom = longLadderPrefab.GetComponentsInChildren<Ladder>()
                .FirstOrDefault(ladder => !ladder.IsTop);

            longLadderPrefab.name = "LadderPrefab";
            longLadderTop.name = "LadderTop";
            longLadderTop.Destination = longLadderBottom;
            longLadderBottom.name = "LadderBottom";
            longLadderBottom.Destination = longLadderTop;

            longLadderPrefab.transform.position = pos - new Vector3(0f, 2.9f);
            longLadderBottom.transform.position = pos - new Vector3(0f, 6.6f);
            longLadderTop.transform.position = pos;
            longLadderPrefab.SetActive(false);
            LongLadderPrefab = longLadderPrefab;

            var platformPrefab =
                Object.Instantiate(airshipPrefab.GetComponentInChildren<MovingPlatformBehaviour>());
            platformPrefab.name = "PlatformPrefab";
            platformPrefab.gameObject.SetActive(false);
            PlatformPrefab = platformPrefab;

            var platformConsolePrefab = Object.Instantiate(airshipPrefab.GetComponentInChildren<PlatformConsole>());
            platformConsolePrefab.name = "PlatformConsolePrefab";
            platformConsolePrefab.gameObject.SetActive(false);
            PlatformConsolePrefab = platformConsolePrefab;

            AllCommonTasks.AddRange(airshipPrefab.GetComponent<ShipStatus>().CommonTasks);
            AllLongTasks.AddRange(airshipPrefab.GetComponent<ShipStatus>().LongTasks);
            AllNormalTasks.AddRange(airshipPrefab.GetComponent<ShipStatus>().NormalTasks);
            
            skeldPrefab.Destroy();
            miraPrefab.Destroy();
            airshipPrefab.Destroy();

            yield break;
        }

        public static void CreateSimpleObject(Transform room, SimpleObjectData objectData)
        {
            try
            {
                var originalObjectPrefab = Enum.TryParse(objectData.Type, out SimpleObjectType type)
                    ? SimpleObjectPrefabs[type]
                    : GameObject.Find(objectData.Type);
                if (originalObjectPrefab == null)
                {
                    Logger<ApolloPlugin>.Error(
                        $"Failed to create simple object {objectData.Name} of type {objectData.Type} in room {room.gameObject.name}: Type not found");
                    return;
                }

                var originalSimpleObject = room.FindChild(objectData.ObjectName);
                var simpleObject = Object.Instantiate(originalObjectPrefab, room);
                simpleObject.name = objectData.Name;
                simpleObject.transform.position = originalSimpleObject.position;
                simpleObject.transform.SetLocalZ(-0.001f);

                var rend = simpleObject.GetComponent<SpriteRenderer>();
                rend.flipX = objectData.FlipX;
                rend.flipY = objectData.FlipY;

                var collider = simpleObject.GetComponent<Collider2D>();
                if (collider != null && objectData.DisableCollider)
                {
                    collider.isTrigger = true;
                }

                simpleObject.SetActive(true);
                originalSimpleObject.gameObject.Destroy();
            }
            catch (Exception e)
            {
                Logger<ApolloPlugin>.Error(
                    $"Failed to create simple object {objectData.Name} of type {objectData.Type} in room {room.gameObject.name}:\n" +
                    e);
            }
        }

        public static void CreateVent(Transform room, VentData ventData)
        {
            try
            {
                var ship = ShipStatus.Instance;
                var allVents = ship.AllVents.ToList();

                var ventObject = room.FindChild(ventData.ObjectName).gameObject;

                var vent = Object.Instantiate(ventData.SkeldVent() ? SkeldVentPrefab : PolusVentPrefab, room.transform);
                vent.transform.position = ventObject.transform.position;
                vent.name = ventData.Name;
                vent.Id = allVents.Count + 1;
                vent.gameObject.SetActive(true);

                if (allVents.Count != 0)
                {
                    allVents.Last().Right = vent;
                    vent.Left = allVents.Last();
                }

                ventObject.Destroy();

                ship.AllVents = ship.AllVents.Add(vent);
            }
            catch (Exception e)
            {
                Logger<ApolloPlugin>.Error(
                    $"Failed to create vent {ventData.Name} in room {room.gameObject.name}:\n" +
                    e);
            }
        }

        public static void CreateCam(Transform room, CamData camData)
        {
            try
            {
                var ship = ShipStatus.Instance;
                var camObject = room.FindChild(camData.ObjectName).gameObject;

                var camera = Object.Instantiate(camData.SkeldCam() ? SkeldCamPrefab : PolusCamPrefab, room.transform);
                camera.transform.position = camObject.transform.position;
                camera.name = camData.Name;
                camera.CamName = camData.Name;
                camera.GetComponent<SpriteRenderer>().flipX = camData.Flip;
                camera.Offset = camData.Offset;
                camera.gameObject.SetActive(true);

                camObject.Destroy();

                ship.AllCameras = ship.AllCameras.Add(camera);
            }
            catch (Exception e)
            {
                Logger<ApolloPlugin>.Error(
                    $"Failed to create cam {camData.Name} in room {room.gameObject.name}:\n" +
                    e);
            }
        }

        public static void CreateLadder(Transform room, LadderData ladderData)
        {
            try
            {
                var ladderObject = room.FindChild(ladderData.ObjectName).gameObject;
                var ladderObjectPos = ladderObject.transform.position;
                var ladderParent =
                    Object.Instantiate(ladderData.Short ? ShortLadderPrefab : LongLadderPrefab, room.transform);
                var ladderTop = ladderParent.GetComponentsInChildren<Ladder>()
                    .FirstOrDefault(ladder => ladder.IsTop);
                var ladderBottom = ladderParent.GetComponentsInChildren<Ladder>()
                    .FirstOrDefault(ladder => !ladder.IsTop);

                ladderParent.name = ladderData.Name;
                ladderParent.gameObject.SetActive(true);

                ladderTop.name = "LadderTop";
                ladderTop.Destination = ladderBottom;
                ladderTop.Id = CurrentLadderId;
                CurrentLadderId += 1;
                AllLadders.Add(ladderTop);

                ladderBottom.name = "LadderBottom";
                ladderBottom.Destination = ladderTop;
                ladderBottom.Id = CurrentLadderId;
                CurrentLadderId += 1;
                AllLadders.Add(ladderBottom);

                ladderParent.transform.position = ladderObjectPos;

                ladderObject.Destroy();
            }
            catch (Exception e)
            {
                Logger<ApolloPlugin>.Error(
                    $"Failed to create ladder {ladderData.Name} in room {room.gameObject.name}:\n" +
                    e);
            }
        }

        public static void CreatePlatform(Transform room, PlatformData platformData)
        {
            try
            {
                var left = room.FindChild(platformData.LeftUseObject);
                var right = room.FindChild(platformData.RightUseObject);

                var platformParent = new GameObject(platformData.Name);
                platformParent.transform.parent = room;

                var platform = Object.Instantiate(PlatformPrefab, platformParent.transform);
                platform.name = "Platform";
                platform.transform.position = platformData.StartLeft ? left.position : right.position;
                platform.LeftUsePosition = left.position;
                platform.LeftPosition = left.position + new Vector3(1.5f, 0f);
                platform.IsLeft = platformData.StartLeft;
                platform.RightPosition = right.position - new Vector3(1.5f, 0f);
                platform.RightUsePosition = right.position;
                platform.gameObject.SetActive(true);
                platform.transform.FindChild("Fan").gameObject.SetActive(platformData.ShowFan);

                var console1 = Object.Instantiate(PlatformConsolePrefab, platformParent.transform);
                console1.name = "LeftConsole";
                console1.transform.position = left.position;
                console1.Image = platform.GetComponent<SpriteRenderer>();
                console1.gameObject.SetActive(true);

                var console2 = Object.Instantiate(PlatformConsolePrefab, platformParent.transform);
                console2.name = "RightConsole";
                console2.transform.position = right.position;
                console2.Image = platform.GetComponent<SpriteRenderer>();
                console2.gameObject.SetActive(true);

                console1.Platform = console2.Platform = platform;

                MovingPlatformHandler.Platforms.Add(
                    new MovingPlatformHandler.MovingPlatform(platform, console1, console2));

                left.gameObject.Destroy();
                right.gameObject.Destroy();
            }
            catch (Exception e)
            {
                Logger<ApolloPlugin>.Error(
                    $"Failed to create platform {platformData.Name} in room {room.gameObject.name}:\n" +
                    e);
            }
        }

        public static void CreateTask(Transform roomGround, PlainShipRoom room, TaskData taskData)
        {
            try
            {
                if (!Enum.TryParse(taskData.Type, out TaskObjects.TaskType type))
                {
                    Logger<ApolloPlugin>.Error(
                        $"Failed to create task {taskData.Name} of type {taskData.Type} in room {roomGround.gameObject.name}: Type not found");
                    return;
                }
                
                var originalTaskPrefab = TaskPrefabs[type];

                var taskObject = roomGround.FindChild(taskData.ObjectName);
                var task = Object.Instantiate(originalTaskPrefab, roomGround);
                task.name = taskData.Name;
                task.transform.position = taskObject.position;
                task.transform.SetLocalZ(-0.001f);

                var console = task.GetComponent<Console>();
                console.ConsoleId = CurrentConsoleId;
                CurrentConsoleId++;
                console.Room = room.RoomId;

                if (task.GetComponent<MedScannerBehaviour>() != null)
                {
                    if (ShipStatus.Instance.MedScanner == null)
                    {
                        ShipStatus.Instance.MedScanner = task.GetComponent<MedScannerBehaviour>();
                    }
                    else
                    {
                        task.Destroy();
                        taskObject.gameObject.Destroy();
                        Logger<ApolloPlugin>.Error(
                            $"Failed to create task {taskData.Name} of type {taskData.Type} in room {roomGround.gameObject.name}: MedScanner already exists");
                        return;
                    }
                }
                
                var rend = task.GetComponent<SpriteRenderer>();
                rend.flipX = taskData.FlipX;
                rend.flipY = taskData.FlipY;

                var collider = task.GetComponent<Collider2D>();
                if (collider != null && taskData.DisableCollider)
                {
                    collider.isTrigger = true;
                }

                var tasks = AllCommonTasks.Where(task => task.name == type.ToString()).ToArray();
                if (tasks.Length != 0)
                {
                    Logger<ApolloPlugin>.Info("Common");
                    var playerTask = tasks[0];
                    playerTask.StartAt = room.RoomId;
                    CommonTasks.Add(playerTask);
                }
                tasks = AllLongTasks.Where(task => task.name == type.ToString()).ToArray();
                if (tasks.Length != 0)
                {
                    Logger<ApolloPlugin>.Info("Long");
                    var playerTask = tasks[0];
                    playerTask.StartAt = room.RoomId;
                    LongTasks.Add(playerTask);
                }
                tasks = AllNormalTasks.Where(task => task.name == type.ToString()).ToArray();
                if (tasks.Length != 0)
                {
                    Logger<ApolloPlugin>.Info("Normal");
                    var playerTask = tasks[0];
                    playerTask.StartAt = room.RoomId;
                    NormalTasks.Add(playerTask);
                }

                task.SetActive(true);
                taskObject.gameObject.Destroy();
            }
            catch (Exception e)
            {
                Logger<ApolloPlugin>.Error(
                    $"Failed to create task {taskData.Name} of type {taskData.Type} in room {roomGround.gameObject.name}:\n" +
                    e);
            }
        }

        public static IEnumerator Reset()
        {
            foreach (var ladder in AllLadders)
            {
                ladder.Destroy();
            }
            AllLadders = new List<Ladder>();
            CurrentLadderId = 0;
            
            CurrentConsoleId = 0;
            CommonTasks = new List<NormalPlayerTask>();
            LongTasks = new List<NormalPlayerTask>();
            NormalTasks = new List<NormalPlayerTask>();
            yield break;
        }
        
        public static IEnumerator DestroyPrefabs()
        {
            foreach (var pair in SimpleObjectPrefabs)
            {
                pair.Value.Destroy();
            }
            SimpleObjectPrefabs = new Dictionary<SimpleObjectType, GameObject>();
            
            SkeldVentPrefab.gameObject.Destroy();
            SkeldVentPrefab = null;
            PolusVentPrefab.gameObject.Destroy();
            PolusVentPrefab = null;
            SkeldCamPrefab.gameObject.Destroy();
            SkeldCamPrefab = null;
            PolusCamPrefab.gameObject.Destroy();
            PolusCamPrefab = null;
            ShortLadderPrefab.Destroy();
            ShortLadderPrefab = null;
            LongLadderPrefab.Destroy();
            LongLadderPrefab = null;
            PlatformPrefab.gameObject.Destroy();
            PlatformPrefab = null;
            PlatformConsolePrefab.gameObject.Destroy();
            PlatformConsolePrefab = null;
            
            foreach (var pair in TaskPrefabs)
            {
                pair.Value.Destroy();
            }
            TaskPrefabs = new Dictionary<TaskObjects.TaskType, GameObject>();
            yield break;
        }
    }
}