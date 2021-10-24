using System.Collections;
using System.Linq;
using Apollo.Data;
using HarmonyLib;
using Reactor;
using Reactor.Extensions;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Apollo
{
    public class CustomMap
    {
        public static bool UseCustomMap;

        public static GameObject Map;
        public static GameObject MapPrefab;
        public static Sprite MapLogo;
        public static MapData MapData;

        public static SurvCamera SkeldCamPrefab;
        public static SurvCamera PolusCamPrefab;
        public static Vent SkeldVentPrefab;
        public static Vent PolusVentPrefab;
        public static GameObject ShortLadderPrefab;
        public static GameObject LongLadderPrefab;

        public static AsyncOperationHandle<GameObject> SkeldPrefab;
        public static AsyncOperationHandle<GameObject> MiraPrefab;
        public static AsyncOperationHandle<GameObject> AirshipPrefab;

        public static IEnumerator CoSetupMap(ShipStatus ship)
        {
            ship.InitialSpawnCenter =
                ship.MeetingSpawnCenter =
                    Map.transform.FindChild("[SPAWN]").transform.position;
            Map.transform.FindChild("[SPAWN]").gameObject.Destroy();
            Map.transform.SetZ(2);

            var emergencyButtonPrefab = GameObject.Find("EmergencyButton");
            if (emergencyButtonPrefab != null)
            {
                var emergencyButton =
                    Object.Instantiate(
                        ship.transform.FindChild("Office").FindChild("caftable").FindChild("EmergencyButton"),
                        emergencyButtonPrefab.transform.parent);
                emergencyButton.position = emergencyButtonPrefab.transform.position;
                ship.MeetingSpawnCenter = emergencyButtonPrefab.transform.position;
                emergencyButtonPrefab.Destroy();
            }

            var securityPanelPrefab = GameObject.Find("SecurityPanel");
            if (securityPanelPrefab != null)
            {
                var securityPanel = Object.Instantiate(ship.transform.FindChild("Electrical").FindChild("Surv_Panel"),
                    securityPanelPrefab.transform.parent);
                securityPanel.name = "SecurityPanel";
                securityPanel.transform.position = securityPanelPrefab.transform.position + new Vector3(0f, -0.16f);
                securityPanelPrefab.Destroy();
            }

            var laptopPrefab = GameObject.Find("CustomizeLaptop");
            if (laptopPrefab != null)
            {
                var laptop =
                    Object.Instantiate(
                        ship.transform.FindChild("Office").FindChild("caftable").FindChild("TaskAddConsole"),
                        laptopPrefab.transform.parent);
                laptop.name = "CustomizeLaptop";
                laptop.transform.position = laptopPrefab.transform.position;
                laptopPrefab.Destroy();
            }

            yield return CoLoadAllMaps();
            yield return CoCreatePrefabs();

            foreach (var roomData in MapData.Rooms)
            {
                var roomObject = Map.transform.FindChild(roomData.Value.ObjectName).gameObject;
                var roomGround = roomObject.transform.FindChild("Ground");
                roomObject.transform.SetZ(1);
                roomGround.SetZ(0.999f);
                roomObject.transform.FindChild("Room").SetZ(0.999f);

                var room = roomObject.transform.FindChild("Room").gameObject.AddComponent<PlainShipRoom>();
                room.roomArea = room.transform.FindChild("AreaCollider").GetComponent<PolygonCollider2D>();
                room.RoomId = (SystemTypes)ship.AllRooms.Count + 1;

                Patches.RegisterCustomRoomName(room.RoomId, roomData.Key);

                ship.AllRooms = ship.AllRooms.Add(room);
                ship.FastRooms.Add(room.RoomId, room);

                if (roomData.Value.Vents != null)
                    roomData.Value.Vents.Do(data => CreateVent(roomGround, data));

                if (roomData.Value.Cams != null)
                    roomData.Value.Cams.Do(data => CreateCam(roomGround, data));

                if (roomData.Value.Ladders != null)
                    roomData.Value.Ladders.Do(data => CreateLadder(roomGround, data));

                Logger<ApolloPlugin>.Info("ADDED " + roomData.Key);
            }

            Patches.ShipStatusAwakeCount = Patches.ShipStatusStartCount = 0;
        }

        public static IEnumerator CoLoadAllMaps()
        {
            SkeldPrefab = AmongUsClient.Instance.ShipPrefabs.ToArray()[0].LoadAsset<GameObject>();
            while (!SkeldPrefab.IsDone) yield return null;

            MiraPrefab = AmongUsClient.Instance.ShipPrefabs.ToArray()[1].LoadAsset<GameObject>();
            while (!MiraPrefab.IsDone) yield return null;

            AirshipPrefab = AmongUsClient.Instance.ShipPrefabs.ToArray()[4].LoadAsset<GameObject>();
            while (!AirshipPrefab.IsDone) yield return null;
        }

        public static IEnumerator CoCreatePrefabs()
        {
            var ship = ShipStatus.Instance;

            var polusCamPrefab = Object.Instantiate(ship.GetComponentInChildren<SurvCamera>());
            polusCamPrefab.name = "PolusCamPrefab";
            polusCamPrefab.NewName = StringNames.ExitButton;
            polusCamPrefab.gameObject.SetActive(false);
            PolusCamPrefab = polusCamPrefab;

            Vent polusVentPrefab = Object.Instantiate(ship.GetComponentInChildren<Vent>());
            polusVentPrefab.name = "PolusVentPrefab";
            polusVentPrefab.Left = null;
            polusVentPrefab.Right = null;
            polusVentPrefab.Center = null;
            polusVentPrefab.gameObject.SetActive(false);
            PolusVentPrefab = polusVentPrefab;

            if (SkeldPrefab.IsDone)
            {
                var skeldPrefab = SkeldPrefab.Result;

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

                skeldPrefab.Destroy();
            }

            if (MiraPrefab.IsDone)
            {
                var miraPrefab = MiraPrefab.Result;
                miraPrefab.Destroy();
            }

            if (AirshipPrefab.IsDone)
            {
                var airshipPrefab = AirshipPrefab.Result;

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

                airshipPrefab.Destroy();
            }

            yield break;
        }

        public static void CreateVent(Transform room, VentData ventData)
        {
            var ship = ShipStatus.Instance;
            var allVents = ship.AllVents.ToList();

            var ventObject = room.FindChild(ventData.ObjectName).gameObject;

            var vent = Object.Instantiate(ventData.SkeldVent() ? SkeldVentPrefab : PolusVentPrefab, room.transform);
            vent.transform.position = ventObject.transform.position;
            vent.name = "vent_" + ventData.Name;
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

        public static void CreateCam(Transform room, CamData camData)
        {
            var ship = ShipStatus.Instance;
            var camObject = room.FindChild(camData.ObjectName).gameObject;

            var camera = Object.Instantiate(camData.SkeldCam() ? SkeldCamPrefab : PolusCamPrefab, room.transform);
            camera.transform.position = camObject.transform.position;
            camera.name = "cam_" + camData.Name;
            camera.CamName = camData.Name;
            camera.GetComponent<SpriteRenderer>().flipX = camData.Flip;
            camera.Offset = camData.Offset;
            camera.gameObject.SetActive(true);

            camObject.Destroy();

            ship.AllCameras = ship.AllCameras.Add(camera);
        }

        public static void CreateLadder(Transform room, LadderData ladderData)
        {
            var ladderObject = room.FindChild(ladderData.ObjectName).gameObject;
            var ladderObjectPos = ladderObject.transform.position;
            var ladderParent = Object.Instantiate(ladderData.Short ? ShortLadderPrefab : LongLadderPrefab, room.transform);
            var ladderTop = ladderParent.GetComponentsInChildren<Ladder>()
                .FirstOrDefault(ladder => ladder.IsTop);
            var ladderBottom = ladderParent.GetComponentsInChildren<Ladder>()
                .FirstOrDefault(ladder => !ladder.IsTop);

            ladderParent.name = ladderData.Name;
            ladderParent.gameObject.SetActive(true);
            ladderTop.name = "LadderTop";
            ladderTop.Destination = ladderBottom;
            ladderBottom.name = "LadderBottom";
            ladderBottom.Destination = ladderTop;
            ladderParent.transform.position = ladderObjectPos;
            
            ladderObject.Destroy();
        }
    }
}