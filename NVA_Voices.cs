using GHPC.Crew;
using GHPC.Effects.Voices;
using GHPC.State;
using GHPC.Vehicle;
using GHPC.Player;
using MelonLoader;
using NVA_Voices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

[assembly: MelonInfo(typeof(NVAVoiceClass), "DDR NVA Voice Patch", "1.0.0", "Bluehawk")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace NVA_Voices
{
    public class VoxConverted : MonoBehaviour
    {
        void Awake()
        {
            enabled = false;
        }
    }
    public class NVAVoiceClass : MelonMod
    {
        public static MelonPreferences_Entry<bool> mute_logger;

        static string[] ChrisSchussCombat = {"de84_TC_fire_ChristianPiontek_combat_05",
                                              "de84_TC_fire_ChristianPiontek_combat_06",
                                              "de84_TC_fire_ChristianPiontek_combat_07",
                                              "de84_TC_fire_ChristianPiontek_combat_08"};
        static string[] ChrisSchussPanic = {"de84_TC_fire_ChristianPiontek_panic_05",
                                             "de84_TC_fire_ChristianPiontek_panic_06",
                                             "de84_TC_fire_ChristianPiontek_panic_07",
                                             "de84_TC_fire_ChristianPiontek_panic_08"};
        static string[] LeonSchussCombat = {"de84_TC_fire_LeonBeilmann_combat_03",
                                             "de84_TC_fire_LeonBeilmann_combat_04"};
        static string[] LeonSchussPanic = {"de84_TC_fire_LeonBeilmann_panic_02",
                                            "de84_TC_fire_LeonBeilmann_panic_07"};

        static string[] ChrissFeuerCombat = {"de84_TC_fire_ChristianPiontek_combat_01",
                                             "de84_TC_fire_ChristianPiontek_combat_02",
                                             "de84_TC_fire_ChristianPiontek_combat_03",
                                             "de84_TC_fire_ChristianPiontek_combat_04"};
        static string[] ChrissFeuerPanic = {"de84_TC_fire_ChristianPiontek_panic_01",
                                            "de84_TC_fire_ChristianPiontek_panic_02",
                                            "de84_TC_fire_ChristianPiontek_panic_03",
                                            "de84_TC_fire_ChristianPiontek_panic_04"};
        static string[] LeonFeuerCombat = {"de84_TC_fire_LeonBeilmann_combat_01",
                                           "de84_TC_fire_LeonBeilmann_combat_02" };
        static string[] LeonFeuerPanic = { "de84_TC_fire_LeonBeilmann_panic_01",
                                           "de84_TC_fire_LeonBeilmann_panic_03",
                                           "de84_TC_fire_LeonBeilmann_panic_04",
                                           "de84_TC_fire_LeonBeilmann_panic_05",
                                           "de84_TC_fire_LeonBeilmann_panic_06",
                                           "de84_TC_fire_LeonBeilmann_panic_08",
                                           "de84_TC_fire_LeonBeilmann_panic_09"};

        static bool activeScene = false;
        public static GameObject gameManager;

        public override void OnInitializeMelon()
        {
            MelonPreferences_Category cfg = MelonPreferences.CreateCategory("NVA Voice Patch");
            mute_logger = cfg.CreateEntry<bool>("Mute log messages", false);
            mute_logger.Comment = "Mutes log messages in the MelonLoader console.";
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "MainMenu2_Scene" || sceneName == "t64_menu" || sceneName == "MainMenu2-1_Scene")
            {
                activeScene = false;
                return;
            }

            gameManager = GameObject.Find("_APP_GHPC_");
            if (gameManager == null) return;
            
            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(VoicePatch), GameStatePriority.Lowest);            
        }

        public IEnumerator VoicePatch(GameState _)
        {
            if (activeScene == true) { yield break; }
            activeScene = true;

            //When we modify voices in a scene, it will effect all vehicles at once, so we must determine if the player is NVA or Bundeswehr
            //We must also make the changes in a loop of vehicles, since the player can start in an NVA "vehicle" with no voice, like the SPG-9 launcher.            
            PlayerInput pi = gameManager.gameObject.GetComponent<PlayerInput>();
            Vehicle playerStart = (Vehicle)pi.CurrentPlayerUnit;
            if (playerStart == null) { MelonLogger.Error("Too soon to assign a unit to the player"); }
            bool playerNVA;
            switch (playerStart.UniqueName) //is there no better way to determine if a vehicle with German voices is NVA or Bundeswehr?
            {
                case ("URAL375D"):
                case ("STATIC_SPG9"):
                case ("STATIC_SPG9_TRENCH"):
                case ("STATIC_9K111"):
                case ("STATIC_9K111_TRENCH"):
                case ("BTR70"):
                case ("BTR60PB"):
                case ("BRDM2"):
                case ("T54A"):
                case ("T55A"):
                case ("BMP1"):
                case ("BMP1P"):
                case ("BMP2"):
                case ("T72"):
                case ("T72M"):
                case ("T72M1"):
                case ("T72GILLS"):
                case ("T72UV1"):
                case ("T72UV2"):
                case ("T72ULEM"):
                case ("PT76B"):
                case ("T3485"):
                    playerNVA = true;
                    break;
                default:
                    playerNVA = false;
                    break;
            }

            Vehicle[] vicArray = GameObject.FindObjectsByType<Vehicle>(FindObjectsSortMode.None);
            
            foreach (var vic in vicArray)
            {
                if (vic.CrewVoiceHandler == null) continue; 
                CrewVoiceHandler cvh = vic.CrewVoiceHandler;
                if (cvh.VoiceProtocolLineData.name == "US_84_VoiceProtocol") continue;
                bool isRUS = false;
                if (cvh.VoiceProtocolLineData.name == "USSR_84_VoiceProtocol") isRUS = true;
                cvh._ammoCallout = CrewVoiceHandler.AmmoCallout.DesiredAmmo;
                
                if (vic.gameObject.GetComponent<VoxConverted>() != null) continue;
                vic.gameObject.AddComponent<VoxConverted>();
                if (!mute_logger.Value) { MelonLogger.Msg("Commander ammo designation changed for " + vic); }

                if (isRUS) continue; //ensures we correct Soviet TC ammo designations above, but skip the German-specific voiceline manipulations
                                
                var DictCrewActors = cvh._actorSelections;
                ActorVoiceSetsScriptable cmdrAVSS = DictCrewActors[CrewPosition.Commander];
                string cmdrActor = cmdrAVSS.ActorName;
                List<VoiceSubset> cmdrList = cmdrAVSS.AllVoiceSubsets;
                foreach (VoiceSubset voice in cmdrList)
                {
                    if (voice.Name == "DE 84 TC")
                    {
                        VoiceSubset.VoiceLineDictionary cmdrVoxDict = voice.AllVoiceLines;
                        VoiceLine fire;
                        if (cmdrVoxDict.TryGetValue("fire", out fire))
                        {
                            if (cmdrActor == "Christian Piontek")
                            {
                                fire.CombatClipKeys = ChrissFeuerCombat;
                                fire.PanickedClipKeys = ChrissFeuerPanic;
                            } else
                            {
                                fire.CombatClipKeys = LeonFeuerCombat;
                                fire.PanickedClipKeys = LeonFeuerPanic;
                            }
                            if (!mute_logger.Value) { MelonLogger.Msg("Commander's 'Schuss' removed"); }
                        }
                    }
                }

                if (!playerNVA) continue;
                ActorVoiceSetsScriptable gunnerAVSS = DictCrewActors[CrewPosition.Gunner];
                String gunnerActor = gunnerAVSS.ActorName;
                List<VoiceSubset> gunnerList = gunnerAVSS.AllVoiceSubsets;
                foreach (VoiceSubset voice in gunnerList)
                {
                    if (voice.Name == "DE 84 Gunner/Loader")
                    {
                        VoiceSubset.VoiceLineDictionary gunnerVoxDict = voice.AllVoiceLines;
                        VoiceLine onTheWay;
                        if (gunnerVoxDict.TryGetValue("ontheway", out onTheWay))
                        {
                            if (gunnerActor == "Christian Piontek")
                            {
                                onTheWay.CombatClipKeys = ChrisSchussCombat;
                                onTheWay.PanickedClipKeys = ChrisSchussPanic;
                            }
                            else
                            {
                                onTheWay.CombatClipKeys = LeonSchussCombat;
                                onTheWay.PanickedClipKeys = LeonSchussPanic;
                            }
                            if (!mute_logger.Value) { MelonLogger.Msg("Gunner's 'achtung' replaced with 'Schuss'"); }

                            var table = LocalizationSettings.StringDatabase.GetTable("VoiceLocalizationTable");
                            for (int i = 0; i < 4; i++)
                            {
                                table.AddEntry(ChrisSchussCombat[i], "On the way!");
                                table.AddEntry(ChrisSchussPanic[i], "On the way!");
                            }
                            for (int i = 0; i < 2; i++)
                            {
                                table.AddEntry(LeonSchussCombat[i], "On the way!");
                                table.AddEntry(LeonSchussPanic[i], "On the way!");
                            }
                            if (!mute_logger.Value) { MelonLogger.Msg("Subtitles updated"); }
                        }
                    }
                }
            }
            activeScene = false;
            yield break;
        }
    }
}
