using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Apollo.Data;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Extensions;
using UnityEngine;

namespace Apollo
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class ApolloPlugin : BasePlugin
    {
        public const string Id = "gg.astral.apollo";

        public Harmony Harmony { get; } = new Harmony(Id);

        public static AssetBundle Bundle;
        public static GameObject TestObject;

        public static bool UseCustomMap;

        public override void Load()
        {
            try
            {
                Bundle = AssetBundle.LoadFromFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\map");

                Logger<ApolloPlugin>.Info("Loading main AssetBundle...");

                var objects = 0;
                foreach (var obj in Bundle.LoadAllAssets())
                {
                    objects++;
                    Logger<ApolloPlugin>.Info("Found object: " + obj.name);
                }

                Logger<ApolloPlugin>.Info(
                    $"Finished loading main AssetBundle. Found {objects} objects in AssetBundle");
            }
            catch (Exception e)
            {
                Logger<ApolloPlugin>.Error("Failed to load main AssetBundle - " + e);
            }

            CustomMap.MapPrefab = Bundle.LoadAsset<GameObject>("Map.prefab").DontUnload();
            CustomMap.MapLogo = Bundle.LoadAsset<Sprite>("logo.png").DontUnload();
            CustomMap.MapData = JsonSerializer.Deserialize<MapData>((Bundle.LoadAsset<TextAsset>("Data.json").DontUnload().text));

            Harmony.PatchAll();
        }

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public static class KeybindPatch
        {
            public static void Prefix(KeyboardJoystick __instance)
            {
                if (Input.GetKeyDown(KeyCode.F6))
                {
                    
                }
            }
        }
    }
}