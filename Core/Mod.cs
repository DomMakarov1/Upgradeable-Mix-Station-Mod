using System;
using System.Collections;
using MelonLoader;
using UpgradeableMixStationMod.Data;
using UpgradeableMixStationMod.Items;
using UpgradeableMixStationMod.Shop;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace UpgradeableMixStationMod
{
    public class Mod : MelonMod
    {
        public static Mod Instance { get; private set; }
        public static UpgradeManager Upgrades { get; private set; }

        private bool _injected;

        public override void OnInitializeMelon()
        {
            Instance = this;
            Upgrades = new UpgradeManager();
            LoggerInstance.Msg("UpgradeableMixStationMod loaded.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Main" || sceneName == "Tutorial")
            {
                _injected = false;
                HookSaveSystem();
            }
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (_injected) return;
            if (sceneName != "Main" && sceneName != "Tutorial") return;

            try
            {
                UpgradeParts.Register();

                MelonCoroutines.Start(DelayedVendorSpawn());

                MelonCoroutines.Start(InitSpawnerDelayed());

                _injected = true;
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"Injection failed: {ex}");
            }
        }

        private System.Collections.IEnumerator InitSpawnerDelayed()
        {
            yield return new WaitForSeconds(4f);
            PartSpawner.Initialize();
        }

        public override void OnUpdate()
        {
            //if (Input.GetKeyDown(KeyCode.F8))
            //{
            //    try
            //    {
            //        var player = PlayerSingleton<PlayerMovement>.Instance;
            //        if (player != null)
            //        {
            //            var pos = player.transform.position;
            //            LoggerInstance.Msg($"[F8] Player position: new Vector3({pos.x:F2}f, {pos.y:F2}f, {pos.z:F2}f)");
            //        }
            //        else
            //        {
            //            LoggerInstance.Msg("[F8] Player not found.");
            //        }
            //    }
            //    catch { }
            //}
        }

        private void HookSaveSystem()
        {
            try
            {
                var loadMgr = Singleton<LoadManager>.Instance;
                if (loadMgr != null)
                    loadMgr.onLoadComplete.AddListener(OnGameLoaded);

                var saveMgr = Singleton<SaveManager>.Instance;
                if (saveMgr != null)
                    saveMgr.onSaveStart.AddListener(OnGameSaving);
            }
            catch
            {
                LoggerInstance.Warning("Could not hook save system yet.");
            }
        }

        private void OnGameLoaded()
        {
            var path = Singleton<LoadManager>.Instance?.LoadedGameFolderPath;
            if (!string.IsNullOrEmpty(path))
                Upgrades.Load(path);
        }

        private void OnGameSaving()
        {
            var path = Singleton<LoadManager>.Instance?.LoadedGameFolderPath;
            if (!string.IsNullOrEmpty(path))
                Upgrades.Save(path);
        }

        public override void OnApplicationQuit()
        {
            try
            {
                var path = Singleton<LoadManager>.Instance?.LoadedGameFolderPath;
                if (!string.IsNullOrEmpty(path))
                    Upgrades?.Save(path);
            }
            catch { }
        }

        private IEnumerator DelayedVendorSpawn()
        {
            while (ScheduleOne.NPCs.NPCManager.NPCRegistry.Count == 0)
                yield return new WaitForSeconds(1f);
            
            yield return new WaitForSeconds(2f);
            yield return PartsVendor.SpawnVendor();
        }
    }
}