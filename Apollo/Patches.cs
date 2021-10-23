using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Reactor;
using Reactor.Extensions;
using Reactor.Networking;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace Apollo
{
    [HarmonyPatch]
    public static class Patches
    {
        public static Dictionary<SystemTypes, string> CustomRoomNames = new();

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
        [HarmonyPostfix]
        public static void PrepareOldMap(ShipStatus __instance)
        {
            if (CustomMap.UseCustomMap)
            {
                SurvCamera camPrefab = Object.Instantiate(__instance.GetComponentInChildren<SurvCamera>());
                camPrefab.name = "camPrefab";
                camPrefab.NewName = StringNames.ExitButton;
                camPrefab.gameObject.SetActive(false);
                CustomMap.CamPrefab = camPrefab;

                foreach (var camera in __instance.GetComponentsInChildren<SurvCamera>())
                {
                    camera.Destroy();
                }

                __instance.AllCameras = new List<SurvCamera>().ToArray();

                Vent ventPrefab = Object.Instantiate(__instance.GetComponentInChildren<Vent>());
                ventPrefab.name = "ventPrefab";
                ventPrefab.Left = null;
                ventPrefab.Right = null;
                ventPrefab.Center = null;
                ventPrefab.gameObject.SetActive(false);
                CustomMap.VentPrefab = ventPrefab;

                foreach (var vent in __instance.GetComponentsInChildren<Vent>())
                {
                    vent.Destroy();
                }

                __instance.AllVents = new List<Vent>().ToArray();

                foreach (var console in __instance.GetComponentsInChildren<Console>())
                {
                    console.Destroy();
                }

                __instance.AllConsoles = new List<Console>().ToArray();

                foreach (var room in __instance.GetComponentsInChildren<PlainShipRoom>())
                {
                    room.Destroy();
                }

                __instance.AllRooms = new List<PlainShipRoom>().ToArray();
            }
        }


        // Custom room name text
       
        [HarmonyPatch(typeof(RoomTracker._CoSlideIn_d__11), nameof(RoomTracker._CoSlideIn_d__11.MoveNext))]
        public static class RoomTrackerTextPatch
        {
            public static void Prefix(RoomTracker._CoSlideIn_d__11 __instance)
            {
                var customRoomName = "";
                foreach(var room in CustomRoomNames)
                {
                    if(room.Key == __instance.newRoom)
                    {
                        customRoomName = room.Value;
                    }
                }

                if (customRoomName != "")
                {
                    __instance.__4__this.text.text = customRoomName;
                }
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
        [HarmonyPrefix]
        public static void InitializeMap(ShipStatus __instance)
        {
            if (CustomMap.UseCustomMap)
            {
                __instance.transform.gameObject.SetActive(false);

                var map = Object.Instantiate(CustomMap.MapPrefab);
                map.SetActive(true);
                map.transform.position = __instance.transform.position;

                CustomMap.Map = map;
                CustomMap.SetupMap(__instance);

                if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    CustomMap.UseCustomMap = false;
            }
        }

        [HarmonyPatch(typeof(FreeplayPopover), nameof(FreeplayPopover.Show))]
        public static class AddMapToFreeplay
        {
            private static Transform customMapButton;

            public static void Prefix(FreeplayPopover __instance)
            {
                if (customMapButton == null)
                {
                    customMapButton =
                        Object.Instantiate(__instance.transform.FindChild("Content").FindChild("PlanetButton"),
                            __instance.transform.FindChild("Content"));
                    customMapButton.name = "CustomMapButton";
                    customMapButton.transform.position += new Vector3(0f, 2.1f);
                    customMapButton.GetComponent<SpriteRenderer>().sprite = CustomMap.MapLogo;

                    var button = customMapButton.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    button.OnClick.AddListener((UnityAction)listener);

                    void listener()
                    {
                        CustomMap.UseCustomMap = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
        [HarmonyPostfix]
        public static void SendCustomHandshake(AmongUsClient __instance)
        {
            Coroutines.Start(CheckIfUseCustomMap());
        }

        public static IEnumerator CheckIfUseCustomMap()
        {
            if (!AmongUsClient.Instance.AmHost)
            {
                yield return new WaitForSeconds(1f);
                Rpc<CustomRpc.RpcSendHandshake>.Instance.Send(
                    new CustomRpc.RpcSendHandshake.Data(PlayerControl.LocalPlayer.PlayerId));
            }
        }

        [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Show))]
        public static class AddMapToOnline
        {
            private static Transform customMapButton;

            public static void Prefix(CreateGameOptions __instance)
            {
                if (customMapButton == null)
                {
                    var mapSection = __instance.Content.transform.FindChild("Map");

                    mapSection.position = new Vector3(-2.5f, -0.12f, -20f);

                    var mapButtons = __instance.Content.transform.FindChild("Map")
                        .GetComponentsInChildren<PassiveButton>();
                    for (int i = 0; i < mapButtons.Count; i++)
                    {
                        var _child = mapButtons[i];
                        var child = _child.gameObject;
                        child.transform.localScale /= 1.25f;
                        child.transform.SetX(child.transform.position.x - 0.3f * (i + 1));
                        child.GetComponentInChildren<SpriteRenderer>().enabled = true;
                        _child.OnClick.AddListener((UnityAction)listener1);
                    }

                    customMapButton =
                        Object.Instantiate(__instance.Content.transform.FindChild("Map").FindChild("2"),
                            __instance.Content.transform.FindChild("Map"));
                    customMapButton.name = "CustomMapButton";
                    customMapButton.transform.position += new Vector3(
                        mapButtons.Last().transform.position.x + mapButtons[1].transform.position.x -
                        mapButtons[0].transform.position.x - 0.26f, 0f);
                    customMapButton.transform.FindChild("MapIcon2").GetComponent<SpriteRenderer>().sprite =
                        CustomMap.MapLogo;
                    customMapButton.GetComponent<SpriteRenderer>().enabled = false;

                    var button = customMapButton.GetComponent<ButtonBehavior>();
                    button.OnClick = new ButtonClickedEvent();
                    button.OnClick.AddListener((UnityAction)listener2);

                    mapButtons[2].OnClick.AddListener((UnityAction)listener3);

                    void listener1()
                    {
                        CustomMap.UseCustomMap = false;
                        customMapButton.GetComponent<SpriteRenderer>().enabled = false;
                    }

                    void listener2()
                    {
                        CustomMap.UseCustomMap = true;
                        customMapButton.GetComponent<SpriteRenderer>().enabled = true;
                        mapButtons[2].GetComponent<SpriteRenderer>().enabled = false;
                    }

                    void listener3()
                    {
                        mapButtons[2].GetComponent<SpriteRenderer>().enabled = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        [HarmonyPrefix]
        public static void AddMapToLocal(GameOptionsMenu __instance)
        {
            if (AmongUsClient.Instance.GameMode == GameModes.LocalGame)
            {
                var mapOption = Object.FindObjectOfType<KeyValueOption>();
                
                if (mapOption.Values.Count == 4)
                {
                    var item = mapOption.Values.ToArray()[2];
                    item.key = "CUSTOMAP";
                    mapOption.Values.Add(item);
                    
                    mapOption.OnValueChanged = new Action<OptionBehaviour>(_option =>
                    {
                        Logger<ApolloPlugin>.Info("mapid: " + mapOption.GetInt());
                    });
                }
            }
        }
    }
}