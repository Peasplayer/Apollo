using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Reactor;
using Reactor.Extensions;
using Reactor.Networking;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace LevelCrewmate
{
    public class Patches
    {
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
        public static class ShipStatusAwakePatch
        {
            public static void Postfix(ShipStatus __instance)
            {
                CustomMap.CustomMapActive = CustomMap.UseCustomMap;
                
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
        }
        
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
        public static class ShipStatusStartPatch
        {
            public static void Prefix(ShipStatus __instance)
            {
                if (CustomMap.UseCustomMap)
                {
                    __instance.transform.gameObject.SetActive(false);

                    var map = Object.Instantiate(CustomMap.MapPrefab);
                    map.SetActive(true);
                    map.transform.position = __instance.transform.position;
                        //new Vector3(__instance.InitialSpawnCenter.x, __instance.InitialSpawnCenter.y);//, 1f //
                    CustomMap.Map = map;

                    CustomMap.StartMap(__instance);
                    
                    if (AmongUsClient.Instance.GameMode == GameModes.FreePlay) 
                        CustomMap.UseCustomMap = false;
                }
            }
        }
        
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.SpawnPlayer))]
        public static class ShipStatusSpawnPlayerPatch
        {
            public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] PlayerControl player)
            {
                if (CustomMap.CustomMapActive)
                {
                    player.transform.position = __instance.InitialSpawnCenter;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(FreeplayPopover), nameof(FreeplayPopover.Show))]
        public static class FreeplayPopoverShowPatch
        {
            private static Transform customMapButton;

            public static void Prefix(FreeplayPopover __instance)
            {
                if (customMapButton == null)
                {
                    customMapButton =
                        Object.Instantiate(__instance.transform.FindChild("Content").FindChild("PlanetButton"),
                            __instance.transform.FindChild("Content"));
                    customMapButton.transform.position += new Vector3(0f, 2.1f);

                    var button = customMapButton.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    button.OnClick.AddListener((UnityAction) listener);

                    void listener()
                    {
                        CustomMap.UseCustomMap = true;
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
        public static class AmongUsClientOnGameJoinedPatch
        {
            public static void Postfix(AmongUsClient __instance)
            {
                Coroutines.Start(CheckIfUseCustomMod());
            }

            public static IEnumerator CheckIfUseCustomMod()
            {
                if (!AmongUsClient.Instance.AmHost)
                {
                    yield return new WaitForSeconds(1f);
                    Rpc<CustomRpc.RpcSendHandshake>.Instance.Send(new CustomRpc.RpcSendHandshake.Data(PlayerControl.LocalPlayer.PlayerId));
                }
            }
        }
        
        [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Show))]
        public static class CreateGameOptionsShowPatch
        {
            private static Transform customMapButton;

            public static void Prefix(FreeplayPopover __instance)
            {
                if (customMapButton == null)
                {
                    var mapSection = __instance.transform.FindChild("Content").FindChild("Map");
                    mapSection.localScale = new Vector3(0.8f, 0.8f);
                    mapSection.position = new Vector3(-2.5f, -0.12f, -20f);
                    
                    foreach (var _child in __instance.transform.GetComponentsInChildren<PassiveButton>())
                    {
                        var child = _child.gameObject;
                        child.transform.localScale /= 5f;
                        _child.OnClick.AddListener((UnityAction) listener1);
                    }
                    
                    customMapButton =
                        Object.Instantiate(__instance.transform.FindChild("Content").FindChild("Map").FindChild("2"),
                            __instance.transform.FindChild("Content").FindChild("Map"));
                    customMapButton.transform.position += new Vector3(2.7f, 0f);
                    customMapButton.name = "customMapButton";

                    var button = customMapButton.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    button.OnClick.AddListener((UnityAction) listener2);

                    void listener1()
                    {
                        CustomMap.UseCustomMap = false;
                    }
                    
                    void listener2()
                    {
                        CustomMap.UseCustomMap = true;
                    }
                }
            }
        }
    }
}