using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace Wolfheat.StartMenu
{
    public enum WallSoundType { Stone, Moss, Flesh, Sand } 
    public enum SoundName
    {
        MenuStep, 
        MenuClick, 
        MenuOver, 
        ButtonClick,
        PlaceCard,
        PlaceSwap,
        PlaceError,
        PickupCard,
        DrawCard,
        MergeCards
    }

    public enum MusicName {MenuMusic, OutDoorMusic, IndoorMusic, DeadMusic, CreditsMusic, BossMusic}

    [Serializable]
    public class Music : BaseSound
    {
        public MusicName name;
        public void SetSound(AudioSource source)
        {
            audioSource = source;
        }
    }

    [Serializable]
    public class Sound: BaseSound
    { 
        public SoundName name;
        public void SetSound(AudioSource source)
        {
            audioSource = source;
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;
        }
    }
    
    [Serializable]
    public class BaseSound
        {
        public AudioClip clip;
        [Range(0f,1f)]
        public float volume;
        [Range(0.8f, 1.2f)]
        public float pitch=1f;
        public bool loop=false;
        [HideInInspector] public AudioSource audioSource;

    }

    public class SoundMaster : MonoBehaviour
    {
        public static SoundMaster Instance { get; private set; }
        public const float MuteBoundary = 0.015f;
        public AudioMixer mixer;
        public AudioMixerGroup masterGroup;  
        public AudioMixerGroup musicMixerGroup;  
        public AudioMixerGroup SFXMixerGroup;  
        [SerializeField] private Sound[] sounds;
        [SerializeField] private Sound[] effects;
        [SerializeField] private Music[] musics;

        [SerializeField]private AudioClip[] coins;
        [SerializeField]private AudioClip[] getHit;
        [SerializeField]private AudioClip[] bossGetHit;
        [SerializeField]private AudioClip[] pickAxeHitStone;
        [SerializeField]private AudioClip[] pickAxeHitMoss;
        [SerializeField]private AudioClip[] pickAxeHitFlesh;
        [SerializeField]private AudioClip[] pickAxeHitSand;
        [SerializeField]private AudioClip[] pickAxeCrushStone;
        [SerializeField] private AudioClip[] pickAxeCrushMoss;
        [SerializeField] private AudioClip[] pickAxeCrushFlesh;
        [SerializeField] private AudioClip[] pickAxeCrushSand;



        [SerializeField]private AudioClip[] footstepFlesh;
        [SerializeField]private AudioClip[] footstepSand;
        [SerializeField]private AudioClip[] footstepMoss;
        [SerializeField]private AudioClip[] footstepStone;
        [SerializeField]private AudioClip[] footstepIndoor;
        [SerializeField]private AudioClip[] footstepAltar;

        private Dictionary<SoundName,Sound> soundsDictionary = new();
        private Dictionary<MusicName,Music> musicDictionary = new();
        AudioSource musicSource;
        MusicName activeMusic;
        AudioSource stepSource;
        AudioSource getHitSource;

        SoundSettings soundSettings = new SoundSettings();

        public Action GlobalMuteChanged;

        private void OnEnable()
        {            
            SavingUtility.LoadingComplete += LoadingComplete;
        }

        private void LoadingComplete()
        {
            Debug.Log("Loading of settings complete setting the soundSettings");

            soundSettings = SavingUtility.gameSettingsData.soundSettings;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
            Debug.Log("SoundMaster Start");        
            // Define all sounds
            foreach (var sound in sounds)
            {
                sound.SetSound(gameObject.AddComponent<AudioSource>());
                sound.audioSource.outputAudioMixerGroup = SFXMixerGroup;
                soundsDictionary.Add(sound.name, sound);
            }
            foreach (var sound in effects)
            {
                sound.SetSound(gameObject.AddComponent<AudioSource>());
                sound.audioSource.outputAudioMixerGroup = SFXMixerGroup;
                soundsDictionary.Add(sound.name, sound);
            }

            //Steps
            stepSource = gameObject.AddComponent<AudioSource>();
            stepSource.volume = 0.5f;
            stepSource.outputAudioMixerGroup = SFXMixerGroup;

            // And Music
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.outputAudioMixerGroup = musicMixerGroup;

            foreach (var music in musics)
            {
                // All music use same source (since only one will be playing at a time)
                music.SetSound(musicSource); 
                musicDictionary.Add(music.name, music);
            }

            // Play theme sound
            activeMusic = MusicName.MenuMusic;
            
            
            PlayMusic(MusicName.MenuMusic);

            
        }

        //private bool haveNotSaidExplode = true;
        //public void BombHissing()
        //{
        //    if (haveNotSaidExplode)
        //    {
        //        //PlaySound(SoundName.ItsGonaBlow);
        //        haveNotSaidExplode= false;
        //    }
        //    PlaySound(SoundName.Hissing);
        //}
        public void AddRestartSpeech()
        {
            StartCoroutine(AddRestartSpeechCO());
        }
        public IEnumerator AddRestartSpeechCO()
        {
            yield return new WaitForSeconds(1.5f);
            //PlaySpeech(SoundName.MyHeadHurts);
            yield return new WaitForSeconds(1.5f);
            //PlaySpeech(SoundName.INeedToBeMoreCareful);
        }

        public void PlayMusic(MusicName name)
        {
            //if (activeMusic == name) return;

            Debug.Log("PLAY MUSIC "+name);

            activeMusic = name; // Leave this here so the correct music that should be played is still updated if music is reenabled

            if (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseMusic) return;
            //f (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseMusic || !SavingUtility.HasLoaded) return; <. Also checks for savefile settings to load
            
            if (musicDictionary.ContainsKey(name))
            {
                if (musicDictionary[name].audioSource.isPlaying && !musicDictionary[name].loop)
                    return;
                musicSource.clip = musicDictionary[name].clip;
                musicSource.volume = musicDictionary[name].volume;
                musicSource.pitch = musicDictionary[name].pitch;
                musicSource.loop = musicDictionary[name].loop;
                musicSource.Play();
            }
            else
                Debug.LogWarning("No clip named "+name+" in dictionary.");

        }

        private void Update()
        {
            if (speechQueue.Count > 0)
            {
                if (speechQueue[0].isPlaying)
                    return;
                speechQueue.RemoveAt(0);
                // At least one speech to play
                if (speechQueue.Count >= 1)
                    speechQueue[0].Play();
            }
        }

        private List<AudioSource> speechQueue = new List<AudioSource>();

        public void PlaySound(SoundName name, bool allowInterupt= true)
        {
            //Debug.Log("Play Sound "+name);
            if (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseSFX) return;

            //Debug.Log("Play Sound: "+name);
            if (soundsDictionary.ContainsKey(name))
            {
                if (!allowInterupt && soundsDictionary[name].audioSource.isPlaying && !soundsDictionary[name].loop)
                    return;
                
                Debug.Log("Start Sound: "+name);
                
                soundsDictionary[name].audioSource.Play();
            }
            else 
                Debug.LogWarning("No clip named "+name+" in dictionary.");

        }

        public void FadeMusic(float time = 1f)
        {
            StartCoroutine(MusicFade(time));
        }
        public IEnumerator MusicFade(float time)
        {
            float changePerSecond = musicSource.volume / time;
            while (musicSource.volume > 0)
            {
                musicSource.volume -= changePerSecond * Time.deltaTime;
                yield return null;
            }
            musicSource.Stop();
        }
        public void UpdateVolume()
        {
            //Debug.Log("SOUNDMASTER - Updating SoundMaster's Volumes, This uses Sound Settings values");
            // Convert to dB
            mixer.SetFloat("Volume", Mathf.Log10(soundSettings.MasterVolume) * 20);
        
            //Set Music
            mixer.SetFloat("MusicVolume", Mathf.Log10(soundSettings.MusicVolume) * 20);
        
            // Set SFX
            mixer.SetFloat("SFXVolume", Mathf.Log10(soundSettings.SFXVolume) * 20);

            EnableSoundAccordingToMixersVolumes();
            
        }

        private void EnableSoundAccordingToMixersVolumes()
        {
            //Debug.Log("SOUNDMASTER - Enabling Sound According to Mixer Volumes, Music volume = " + soundSettings.MusicVolume);
            //Master
            soundSettings.UseMaster = soundSettings.MasterVolume > MuteBoundary;
            soundSettings.UseMusic  = soundSettings.MusicVolume > MuteBoundary;
            soundSettings.UseSFX    = soundSettings.SFXVolume > MuteBoundary;

            //Debug.Log("SOUNDMASTER - Global Sound:" + (soundSettings.GlobalMaster==true?"ON":"OFF") + "    Master: "+ soundSettings.UseMaster + " Music: " +soundSettings.UseMusic+" SFX: "+ soundSettings.UseSFX);

            if (soundSettings.GlobalMaster && soundSettings.UseMaster && soundSettings.UseMusic)
            {
                //Debug.Log("SOUNDMASTER - Global and Master and Music is ON");
                if (!musicSource.isPlaying)
                {
                    //Debug.Log("SOUNDMASTER - Resume Music");
                    ResumeMusic();
                }
            }
            else
            {
                //Debug.Log("SOUNDMASTER - Global or Master or Music is OFF");

                if (musicSource.isPlaying)
                    musicSource.Stop();
                //Stop all SFX?
            }

        }
        /*
        public void ToggleAllAudio(InputAction.CallbackContext context)
        {
            if (NameChanger.Instance != null && NameChanger.Instance.IsEditingName) {
                SpecialInfo.Instance?.ShowInfo("Editing Textfield - do not toggle audio");
                return;
            }
            //Debug.Log("Soundmaster toggle all audio");
            soundSettings.GlobalMaster = !soundSettings.GlobalMaster;
            ToggleMusic();
            //GlobalMuteChanged?.Invoke();
        }
        public void ToggleMusic(InputAction.CallbackContext context)
        {
            if (NameChanger.Instance != null && NameChanger.Instance.IsEditingName) {
                SpecialInfo.Instance?.ShowInfo("Editing Textfield - do not toggle music");
                return;
            }
            //Debug.Log("Soundmaster toggle music");
            soundSettings.UseMusic = !soundSettings.UseMusic;
            ToggleMusic();
        }
        */
        public void ToggleMusic()
        {
            //Debug.Log("TOGGLE MUSIC");
            
            //Debug.Log("Global Sound Set To:"+ (soundSettings.GlobalMaster==true?"ON":"OFF") + " Master: "+ soundSettings.UseMaster + " Music: " +soundSettings.UseMusic+" SFX: "+ soundSettings.UseSFX);
            // Update Music playing 
            if (soundSettings.GlobalMaster)
            {
                //Debug.Log("GLobal master is on");
                if (soundSettings.UseMaster && soundSettings.UseMusic && !musicSource.isPlaying)
                    ResumeMusic();
                else
                    musicSource.Stop();
            }
            else 
                if (musicSource.isPlaying)
                {
                        //Debug.Log("Stopping Music from playing?");
                    musicSource.Stop();
                }
            GameSettingsData.GameSettingsUpdated?.Invoke();
        }   
        public void StopSound(SoundName name)
        {
            if (soundsDictionary.ContainsKey(name))
            {
                soundsDictionary[name].audioSource.Stop();
            }
            else
                Debug.LogWarning("No clip named " + name + " in dictionary.");
        }
        public void ResetMusic()
        {
            ResumeMusic();
        }
        public void ResumeMusic()
        {
            //Debug.Log("Resume Music "+activeMusic);
            PlayMusic(activeMusic);
        }

        public void PlayCoinSound(bool buying = false)
        {
            //Debug.Log("Playing coin sound");
            if (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseSFX) return;
            if(buying)
                stepSource.PlayOneShot(coins[0]);
            else
                stepSource.PlayOneShot(coins[Random.Range(1, coins.Length)]);

        }
        public void PlayBossTakeDamageSound()
        {
            if (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseSFX) return;
            stepSource.PlayOneShot(bossGetHit[Random.Range(0, bossGetHit.Length)]);
        }
        public void PlayGetHitSound()
        {
            if (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseSFX) return;
            stepSource.PlayOneShot(getHit[Random.Range(0, getHit.Length)]);
        }
        public void PlayStepSound(int stepSoundType = 0)
        {
            if (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseSFX) return;

            switch (stepSoundType)
            {
                case 0:
                    stepSource.PlayOneShot(footstepFlesh[Random.Range(0, footstepFlesh.Length)]);
                    break;
                case 1:
                    stepSource.PlayOneShot(footstepSand[Random.Range(0, footstepSand.Length)]);
                    break;
                case 2:
                    stepSource.PlayOneShot(footstepStone[Random.Range(0, footstepStone.Length)]);
                    break;
                case 3:
                    stepSource.PlayOneShot(footstepMoss[Random.Range(0, footstepMoss.Length)]);
                    break;
                case 4: 
                    stepSource.PlayOneShot(footstepAltar[Random.Range(0, footstepAltar.Length)]);
                    break;
                case 5:
                    stepSource.PlayOneShot(footstepIndoor[Random.Range(0, footstepIndoor.Length)]);
                    break;

                default:
                    break;
            }
            // Only play foot step if last footstep is finished playing
            //if (!stepSource.isPlaying)
        }


        public void PlayPickAxeHitWall(WallSoundType type = WallSoundType.Stone, bool crushed = false)
        {
            if (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseSFX) return;
            //Debug.Log("Hitting wall "+type+" crush = "+crushed);
            switch (type)
            {
                case WallSoundType.Stone:
                    if (!crushed && pickAxeHitStone.Length > 0)
                        stepSource.PlayOneShot(pickAxeHitStone[Random.Range(0, pickAxeHitStone.Length)]);
                    else if (crushed && pickAxeCrushStone.Length > 0)
                        stepSource.PlayOneShot(pickAxeCrushStone[Random.Range(0, pickAxeCrushStone.Length)]);
                    break;
                case WallSoundType.Moss:
                    if (!crushed && pickAxeHitMoss.Length > 0)
                        stepSource.PlayOneShot(pickAxeHitMoss[Random.Range(0, pickAxeHitMoss.Length)]);
                    else if (crushed && pickAxeCrushMoss.Length > 0)
                        stepSource.PlayOneShot(pickAxeCrushMoss[Random.Range(0, pickAxeCrushMoss.Length)]);
                    break;
                case WallSoundType.Flesh:
                    if (!crushed && pickAxeHitFlesh.Length > 0)
                        stepSource.PlayOneShot(pickAxeHitFlesh[Random.Range(0, pickAxeHitFlesh.Length)]);
                    else if (crushed && pickAxeCrushFlesh.Length > 0)
                        stepSource.PlayOneShot(pickAxeCrushFlesh[Random.Range(0, pickAxeCrushFlesh.Length)]);
                    break;
                case WallSoundType.Sand:
                    if (!crushed && pickAxeHitSand.Length > 0)
                        stepSource.PlayOneShot(pickAxeHitSand[Random.Range(0, pickAxeHitSand.Length)]);
                    else if (crushed && pickAxeCrushSand.Length > 0)
                        stepSource.PlayOneShot(pickAxeCrushSand[Random.Range(0, pickAxeCrushSand.Length)]);
                    break;
            }

        }
        public void PlayPickAxeCrushStone(WallSoundType type = WallSoundType.Stone)
        {
            if (!soundSettings.GlobalMaster || !soundSettings.UseMaster || !soundSettings.UseSFX) return;

            switch (type)
            {
                case WallSoundType.Stone:
                    if (pickAxeHitStone.Length > 0)
                        stepSource.PlayOneShot(pickAxeHitStone[Random.Range(0, pickAxeHitStone.Length)]);
                    break;
                case WallSoundType.Moss:
                    if (pickAxeHitMoss.Length > 0)
                        stepSource.PlayOneShot(pickAxeHitMoss[Random.Range(0, pickAxeHitMoss.Length)]);
                    break;
                case WallSoundType.Flesh:
                    if (pickAxeHitFlesh.Length > 0)
                        stepSource.PlayOneShot(pickAxeHitFlesh[Random.Range(0, pickAxeHitFlesh.Length)]);
                    break;
                case WallSoundType.Sand:
                    if (pickAxeHitSand.Length > 0)
                        stepSource.PlayOneShot(pickAxeHitStone[Random.Range(0, pickAxeHitSand.Length)]);
                    break;
            }

        }
        public void ReadDataFromSave()
        {
            //Debug.Log("Updating Sound Volumes from saved file");
            UpdateVolume();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }
    }
}
