using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Reactor;
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
        private static Dictionary<SystemTypes, string> CustomRoomNames = new();

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
        [HarmonyPostfix]
        public static void PrepareOldMap(ShipStatus __instance)
        {
            if (CustomMap.UseCustomMap)
            {
                Coroutines.Start(CustomMap.CoLoadAllMaps());
                
                __instance.Clear();
            }
        }
        
        public static int ShipStatusAwakeCount;
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
        [HarmonyPrefix]
        public static bool CheckIfShouldExecuteShipStatusAwake(ShipStatus __instance)
        {
            if (ShipStatusAwakeCount == 1)
                return false;
            
            if (CustomMap.UseCustomMap)
            {
                ShipStatusAwakeCount++;
            }

            return true;
        }

        public static void RegisterCustomRoomName(SystemTypes type, string name)
        {
            if (CustomRoomNames.ContainsKey(type))
                CustomRoomNames[type] = name;
            else
                CustomRoomNames.Add(type, name);
        }
       
        [HarmonyPatch(typeof(RoomTracker._CoSlideIn_d__11), nameof(RoomTracker._CoSlideIn_d__11.MoveNext))]
        public static class RoomTrackerCustomRoomNames
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

        public static int ShipStatusStartCount;
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
        [HarmonyPrefix]
        public static bool InitializeMap(ShipStatus __instance)
        {
            if (ShipStatusStartCount == 1)
                return false;
            
            if (CustomMap.UseCustomMap)
            {
                ShipStatusStartCount++;
                __instance.transform.gameObject.SetActive(false);

                var map = Object.Instantiate(CustomMap.MapPrefab);
                map.SetActive(true);
                map.transform.position = __instance.transform.position;

                CustomMap.Map = map;
                Coroutines.Start(CustomMap.CoSetupMap(__instance));

                if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    CustomMap.UseCustomMap = false;
            }

            return true;
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
                    customMapButton.transform.position = new Vector3(3.29f, 0.06999993f, -10f);
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
            
            if (!GameOptionsData.MapNames.Contains(CustomMap.MapData.Name))
                GameOptionsData.MapNames = GameOptionsData.MapNames.Add(CustomMap.MapData.Name);
            
            if (AmongUsClient.Instance.ShipPrefabs.ToArray().Count != 5)
            {
                var map = AmongUsClient.Instance.ShipPrefabs.ToArray()[2];
                AmongUsClient.Instance.ShipPrefabs.Insert(5, map);
            }
        }

        public static IEnumerator CheckIfUseCustomMap()
        {
            if (!AmongUsClient.Instance.AmHost)
            {
                yield return new WaitForSeconds(1f);
                Rpc<CustomRpc.RpcSendHandshake>.Instance.Send(PlayerControl.LocalPlayer.PlayerId);
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

                    var button = customMapButton.GetComponent<PassiveButton>();
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
        
        private static Action<KeyValueOption> HandleMapOption = new Action<KeyValueOption>(option =>
        {
            if (option.GetInt() == 5 && !CustomMap.UseCustomMap)
            {
                CustomMap.UseCustomMap = true;
                Rpc<CustomRpc.RpcUseCustomMap>.Instance.Send(true);
            }
            else if (option.GetInt() != 5 && CustomMap.UseCustomMap)
            {
                CustomMap.UseCustomMap = false;
                Rpc<CustomRpc.RpcUseCustomMap>.Instance.Send(false);
            }
        });

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        [HarmonyPrefix]
        public static void AddMapToLocal(GameOptionsMenu __instance)
        {
            if (AmongUsClient.Instance.GameMode == GameModes.LocalGame)
            {
                var mapOption = Object.FindObjectOfType<KeyValueOption>();
                
                if (mapOption.Values.Count == 4)
                {
                    var item = mapOption.Values.ToArray().First();
                    item.key = CustomMap.MapData.Name;
                    item.value = 5;
                    mapOption.Values.Add(item);
                }
            }
        }

        [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.Increase))]
        [HarmonyPostfix]
        public static void KeyValueOptionIncreasePatch(KeyValueOption __instance)
        {
            if (__instance.OnValueChanged != null)
            {
                HandleMapOption.Invoke(__instance);
            }
        }
        
        [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.Decrease))]
        [HarmonyPostfix]
        public static void KeyValueOptionDecreasePatch(KeyValueOption __instance)
        {
            if (__instance.OnValueChanged != null)
            {
                HandleMapOption.Invoke(__instance);
            }
        }
        
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
        [HarmonyPrefix]
        public static void AmongUsClientStartGamePatch(AmongUsClient __instance)
        {
            if (PlayerControl.GameOptions.MapId == 5)
            {
                PlayerControl.GameOptions.MapId = 2;
                CustomMap.UseCustomMap = true;
            }
        }
        
        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
        public static class AmBannedPatch
        {
            public static void Postfix(out bool __result)
            {
                __result = false;
            }
        }
    }
}