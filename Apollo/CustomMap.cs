using System.Linq;
using Apollo.Data;
using Reactor;
using Reactor.Extensions;
using UnityEngine;
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

        public static SurvCamera CamPrefab;
        public static Vent VentPrefab;

        public static void SetupMap(ShipStatus ship)
        {
            ship.InitialSpawnCenter =
                ship.MeetingSpawnCenter =
                    Map.transform.FindChild("[SPAWN]").transform.position; //new Vector2(12.7f, -3f);
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
                    foreach (var ventData in roomData.Value.Vents)
                    {
                        var allVents = ship.AllVents.ToList();

                        var ventObject = roomGround.FindChild(ventData.ObjectName).gameObject;

                        var vent = Object.Instantiate(VentPrefab, roomGround.transform);
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

                if (roomData.Value.Cams != null)
                    foreach (var camData in roomData.Value.Cams)
                    {
                        var camObject = roomGround.FindChild(camData.ObjectName).gameObject;

                        var camera = Object.Instantiate(CamPrefab, roomGround.transform);
                        camera.transform.position = camObject.transform.position;
                        camera.name = "cam_" + camData.Name;
                        camera.CamName = camData.Name;
                        camera.gameObject.SetActive(true);
                        camera.GetComponent<SpriteRenderer>().flipX = camData.Flip;
                        camera.Offset = camData.Offset.ToVector2;

                        camObject.Destroy();

                        ship.AllCameras = ship.AllCameras.Add(camera);
                    }

                Logger<ApolloPlugin>.Info("ADDED " + roomData.Key);
            }
        }
    }
}