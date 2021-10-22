using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Extensions;
using UnityEngine;

namespace LevelCrewmate
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class LevelCrewmatePlugin : BasePlugin
    {
        public const string Id = "tk.peasplayer.amongus.levelcrewmate";

        public Harmony Harmony { get; } = new Harmony(Id);

        public static AssetBundle Bundle;

        public override void Load()
        {
            try
            {
                Bundle = AssetBundle.LoadFromFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\map");

                Logger<LevelCrewmatePlugin>.Info("Loading main AssetBundle...");

                var objects = 0;
                foreach (var obj in Bundle.LoadAllAssets())
                {
                    objects++;
                    Logger<LevelCrewmatePlugin>.Info("Found object: " + obj.name);
                }

                Logger<LevelCrewmatePlugin>.Info(
                    $"Finished loading main AssetBundle. Found {objects} objects in AssetBundle");
            }
            catch (Exception e)
            {
                Logger<LevelCrewmatePlugin>.Error("Failed to load main AssetBundle - " + e);
            }

            CustomMap.MapPrefab = Bundle.LoadAsset<GameObject>("Map.prefab").DontUnload();
            CustomMap.MapLogo = Bundle.LoadAsset<Sprite>("logo.png").DontUnload();

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