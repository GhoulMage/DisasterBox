using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using GhoulMage.LethalCompany;
using UnityEngine;

namespace DisasterBoxMod
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInProcess("Lethal Company.exe")]
    [BepInDependency(LC_API.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class DisasterBoxMod_Plugin : GhoulMagePlugin
    {
        public const string GUID = "ghoulmage.funny.disasterbox";
        public const string NAME = "Disaster Box";
        public const string VERSION = "1.1.0";

        const string ConfigName = "DisasterBox";

        internal static ManualLogSource Log;
        internal static ConfigFile configFile;

        internal static ConfigEntry<bool> LoopThemeIfBoxIsOpen;
        internal static ConfigEntry<float> LoopAudioRange;
        internal static ConfigEntry<float> LoopVolume;

        internal static AudioClip DisasterBox_Theme_Flat;
        internal static AudioClip DisasterBox_Theme_PopUp;
        internal static AudioClip DisasterBox_Theme_Loop;

        protected override LethalGameVersions GameCompatibility => new LethalGameVersions("v40", "v45", "v47");

        protected override Assembly AssemblyToPatch => Assembly.GetExecutingAssembly();

        private static void LoadFromAssetBundle()
        {
            Log.LogInfo("Loading Disaster Box music...");

            if (LC_Info.HasLoadedMod("LC_API"))
            {
                System.Version lcVersion = Chainloader.PluginInfos["LC_API"].Metadata.Version;
                switch (lcVersion.Major)
                {
                    case 3:
                        BundleLoadVer3OrHigher();
                        break;
                    default:
                        BundleLoadVersionUnder3();
                        break;
                }
            }
            else
            {
                BundleLoadVer3OrHigher();
            }

            Log.LogInfo("Disaster Box music loaded..!");
        }

        private static void BundleLoadVersionUnder3()
        {
            DisasterBox_Theme_Flat = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<AudioClip>("Assets/letha/disasterbox_1.ogg");
            if (DisasterBox_Theme_Flat == null)
            {
                Log.LogError("Failed to load Disaster Box FLAT!");
                return;
            }
            DisasterBox_Theme_Flat.LoadAudioData();

            DisasterBox_Theme_PopUp = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<AudioClip>("Assets/letha/disasterbox_2_popup.ogg");
            if (DisasterBox_Theme_PopUp == null)
            {
                Log.LogError("Failed to load Disaster Box Popup!");
                return;
            }
            DisasterBox_Theme_PopUp.LoadAudioData();

            DisasterBox_Theme_Loop = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<AudioClip>("Assets/letha/disasterbox_2_loop.ogg");
            if (DisasterBox_Theme_Loop == null)
            {
                Log.LogError("Failed to load Disaster Box Loop!");

                if (LoopThemeIfBoxIsOpen.Value)
                    return;

                Log.LogInfo("But looping is deactivated anyways so continuing...");
                DisasterBox_Theme_Loop.LoadAudioData();
            }
        }

        private static void BundleLoadVer3OrHigher()
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(Paths.PluginPath + "\\GhoulMage\\funny\\disasterbox");
            DisasterBox_Theme_Flat = bundle.LoadAsset<AudioClip>("Assets/letha/disasterbox_1.ogg");
            if (DisasterBox_Theme_Flat == null)
            {
                Log.LogError("Failed to load Disaster Box FLAT!");
                return;
            }
            DisasterBox_Theme_Flat.LoadAudioData();

            DisasterBox_Theme_PopUp = bundle.LoadAsset<AudioClip>("Assets/letha/disasterbox_2_popup.ogg");
            if (DisasterBox_Theme_PopUp == null)
            {
                Log.LogError("Failed to load Disaster Box Popup!");
                return;
            }
            DisasterBox_Theme_PopUp.LoadAudioData();

            DisasterBox_Theme_Loop = bundle.LoadAsset<AudioClip>("Assets/letha/disasterbox_2_loop.ogg");
            if (DisasterBox_Theme_Loop == null)
            {
                Log.LogError("Failed to load Disaster Box Loop!");

                if (LoopThemeIfBoxIsOpen.Value)
                    return;

                Log.LogInfo("But looping is deactivated anyways so continuing...");
                DisasterBox_Theme_Loop.LoadAudioData();
            }
        }

        private static void GetConfig()
        {
            LoopThemeIfBoxIsOpen = configFile.Bind(ConfigName, "Loop Theme", true, "Loops the metal part of the song as long as the box is open?");
            LoopAudioRange = configFile.Bind(ConfigName, "Loop Audio Range", 12.5f, "Audible range of the looping part.");
            LoopVolume = configFile.Bind(ConfigName, "Loop Volume", 0.65f, "Volume of the looping part, if enabled. Between 0 and 1.");
        }

        protected override void Initialize()
        {
            Log = Logger;
            configFile = Config;
            Startup(GUID, NAME, VERSION, OnSuccesfulLoad);
        }

        private static void OnSuccesfulLoad()
        {
            GetConfig();
            LoadFromAssetBundle();
        }
    }

    /// <summary>
    /// Added in a child GameObject when looping is enabled
    /// </summary>
    public class DisasterBoxLoopBehaviour : MonoBehaviour
    {
        AudioSource _audioSource;

        private void Awake()
        {
#if DEBUG
            DisasterBoxMod_Plugin.Log.LogInfo("Creating AudioSource for DisasterBox Loop...");
#endif
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.Stop();
            _audioSource.clip = DisasterBoxMod_Plugin.DisasterBox_Theme_Loop;
            _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

            float baseAudibleDistance = 0.5f + DisasterBoxMod_Plugin.LoopAudioRange.Value;
            _audioSource.maxDistance = baseAudibleDistance;
            _audioSource.minDistance = baseAudibleDistance * 0.25f;
        }

        public void Play()
        {
#if DEBUG
            DisasterBoxMod_Plugin.Log.LogInfo("Playing DisasterBox Loop!");
#endif
            _audioSource.Stop();
            _audioSource.clip = DisasterBoxMod_Plugin.DisasterBox_Theme_Loop;
            _audioSource.volume = Mathf.Clamp01(DisasterBoxMod_Plugin.LoopVolume.Value);
            _audioSource.loop = true;
            _audioSource.Play();
        }
        public void Stop()
        {
            _audioSource.Stop();
        }
    }
}
