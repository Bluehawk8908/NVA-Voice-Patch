using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using MelonLoader;
using GHPC.Crew;
using GHPC.Effects.Voices;
using GHPC.State;
using GHPC.Vehicle;
using NVA_Voices;

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
            Vehicle[] vicArray = GameObject.FindObjectsByType<Vehicle>(FindObjectsSortMode.None);
            
            foreach (var vic in vicArray)
            {
                if (vic.CrewVoiceHandler == null) continue; 
                CrewVoiceHandler cvh = vic.CrewVoiceHandler;
                if (cvh.VoiceProtocolLineData.name == "US_84_VoiceProtocol") continue;
                bool isNVA = false;
                if (cvh.VoiceProtocolLineData.name == "DE_84_VoiceProtocol") isNVA = true;
                cvh._ammoCallout = CrewVoiceHandler.AmmoCallout.DesiredAmmo;
                
                if (vic.gameObject.GetComponent<VoxConverted>() != null) continue;
                vic.gameObject.AddComponent<VoxConverted>(); 
                MelonLogger.Msg("Commander ammo designation changed for " + vic);

                if (!isNVA) continue; //ensures we correct Soviet TC ammo designations, but skip the German-specific voiceline manipulations
                
                var DictCrewActors = cvh._actorSelections;                
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
                            } else
                            {
                                onTheWay.CombatClipKeys = LeonSchussCombat;
                                onTheWay.PanickedClipKeys = LeonSchussPanic;
                            }
                            MelonLogger.Msg("Gunner's 'achtung' replaced with 'Schuss'");

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
                            MelonLogger.Msg("Subtitles updated");
                        }
                    }
                }

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
                            MelonLogger.Msg("Commander's 'Schuss' removed");
                        }
                    }
                }
            }
            activeScene = false;
            yield break;
        }
    }
}
