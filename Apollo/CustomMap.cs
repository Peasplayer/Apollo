using System;
using System.Linq;
using HarmonyLib;
using Reactor;
using Reactor.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Apollo
{
    public class CustomMap
    {
        public static bool UseCustomMap;
        public static bool CustomMapActive;

        public static GameObject Map;
        public static GameObject MapPrefab;
        public static Sprite MapLogo;
        public static SurvCamera CamPrefab;
        public static Vent VentPrefab;

        public static void StartMap(ShipStatus ship)
        {
            foreach (var room in ship.FastRooms)
            {
                Logger<ApolloPlugin>.Info("FAST " + room.Key.ToString());
            }

            foreach (var room in ship.AllRooms)
            {
                Logger<ApolloPlugin>.Info("ALL " + room.RoomId.ToString());
            }

            ship.Clear(); // Clearing the ShipStatus and all its rooms before "remaking" it

            ship.InitialSpawnCenter = ship.MeetingSpawnCenter = Map.transform.FindChild("Spawn").transform.position;//new Vector2(12.7f, -3f);
            Map.transform.FindChild("Spawn").gameObject.Destroy();
            Map.transform.SetZ(2);

            var emergencyButtonPrefab = GameObject.Find("EmergencyButton");
            if (emergencyButtonPrefab != null)
            {
                var emergencyButton = Object.Instantiate(ship.transform.FindChild("Office").FindChild("caftable").FindChild("EmergencyButton"), emergencyButtonPrefab.transform.parent);
                ship.MeetingSpawnCenter = emergencyButtonPrefab.transform.position;
                emergencyButtonPrefab.Destroy();
            }

            var securityPanelPrefab = GameObject.Find("SecurityPanel");
            if (securityPanelPrefab != null)
            {
                var securityPanel = Object.Instantiate(ship.transform.FindChild("Electrical").FindChild("Surv_Panel"), securityPanelPrefab.transform.parent);
                securityPanel.name = "SecurityPanel";
                securityPanel.transform.position = securityPanelPrefab.transform.position + new Vector3(0f, -0.16f);
                securityPanelPrefab.Destroy();
            }

            var laptopPrefab = GameObject.Find("CustomizeLaptop");
            if (laptopPrefab != null)
            {
                var laptop = Object.Instantiate(ship.transform.FindChild("Office").FindChild("caftable").FindChild("TaskAddConsole"), laptopPrefab.transform.parent);
                laptop.name = "CustomizeLaptop";
                laptop.transform.position = laptopPrefab.transform.position;
                laptopPrefab.Destroy();
            }

            foreach (var roomPrefab in Object.FindObjectsOfType<GameObject>().Where(obj => obj.name.StartsWith("[ROOM]")))
            {
                roomPrefab.transform.SetZ(1);
                roomPrefab.transform.FindChild("Ground").SetZ(0.999f);

                var room = roomPrefab.transform.FindChild("Room").gameObject.AddComponent<PlainShipRoom>();
                room.roomArea = room.transform.FindChild("AreaCollider").GetComponent<PolygonCollider2D>();
                if (roomPrefab.name.Contains(";"))
                {
                    var parts = roomPrefab.name.Split(";");
                    roomPrefab.name = parts[0].Replace("[ROOM]", "");
                    room.RoomId = (SystemTypes)ship.AllRooms.Count + 1;
                }
                else
                    roomPrefab.name = roomPrefab.name.Replace("[ROOM]", "");


                Patches.CustomRoomNames.Add(room.RoomId, roomPrefab.name);

                ship.AllRooms = ship.AllRooms.Add(room);
                ship.FastRooms.Add((SystemTypes)room.RoomId, room);
                Logger<ApolloPlugin>.Info("ADDED " + room.RoomId.ToString());
            }

            foreach (var ventPrefab in Object.FindObjectsOfType<GameObject>().Where(obj => obj.name.StartsWith("[VENT]")))
            {
                var allVents = ship.AllVents.ToList();

                var vent = Object.Instantiate(VentPrefab, ventPrefab.transform.parent);
                vent.transform.position = ventPrefab.transform.position;
                vent.name = "vent_" + ventPrefab.name.Replace("[VENT]", "");
                vent.Id = allVents.Count + 1;
                vent.gameObject.SetActive(true);

                if (allVents.Count != 0)
                {
                    allVents.Last().Right = vent;
                    vent.Left = allVents.Last();
                }

                ventPrefab.Destroy();

                ship.AllVents = ship.AllVents.Add(vent);
            }

            foreach (var camPrefab in Object.FindObjectsOfType<GameObject>().Where(obj => obj.name.StartsWith("[CAMERA]")))
            {
                var parts = camPrefab.name.Split(";");

                var camera = Object.Instantiate(CamPrefab, camPrefab.transform.parent);
                camera.transform.position = camPrefab.transform.position;
                camera.name = "cam_" + parts[0].Replace("[CAMERA]", "");
                camera.CamName = parts[0].Replace("[CAMERA]", "");
                camera.gameObject.SetActive(true);
                camera.GetComponent<SpriteRenderer>().flipX = camPrefab.GetComponent<SpriteRenderer>().flipX;
                if (parts.Length == 3)
                    camera.Offset = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), 0f);

                camPrefab.Destroy();

                var allCams = ship.AllCameras.ToList();
                allCams.Add(camera);
                ship.AllCameras = allCams.ToArray();
            }

            Logger<ApolloPlugin>.Info("FINISHED BUILDING MAP!");

            foreach (var room in ship.FastRooms)
            {
                Logger<ApolloPlugin>.Info("FAST " + room.Key.ToString());
            }

            foreach (var room in ship.AllRooms)
            {
                Logger<ApolloPlugin>.Info("ALL " + room.RoomId.ToString());
            }
        }
    }
}