using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/*
 * Manages global audio properties (volume, pitch) and certain audio sources.
 * 
 */
public class AudioManager : MonoBehaviour {

    // Indexes for the audisources attached to the audimanager
    // Each audiosource may be hot-swapped to different tracks on the fly that are mutually exclusive
    private const int   index_shared = 0, // shared by all one-shot sound effects that are short enough to not worry about stacking
                        index_wind = 1,
                        index_sceneTransition = 2; // used for music that needs to persist between scenes, e.g. the looping sound from the Title Screen to the Tutorial

    private const float windVelocityFactor = 20, windLerpFactor = 10, windVolumeVactor = 2, velocityThreshold = 2;

    [SerializeField]
    AudioMixer mixer = null;
    [SerializeField]
    AudioClip menu_select = null,
                wind_loop = null, // always playing, but becomes louder/higher pitch when moving quickly, especially through the air
                title_screen_loop = null;

    AudioSource[] sources;
    
    public AudioMixer Mixer {
        get {
            return mixer;
        }
    }
    public AudioMixerGroup MixerVoiceBeepsGroup { get; private set; }

    public bool SceneTransitionIsPlaying {
        get {
            return sources[index_sceneTransition].isPlaying;
        }
    }

    private void Awake() {
        sources = GetComponents<AudioSource>();
        MixerVoiceBeepsGroup = mixer.FindMatchingGroups("VoiceBeeps")[0];

        sources[index_wind].clip = wind_loop;
        sources[index_wind].loop = true;
        sources[index_wind].volume = 0;
        sources[index_wind].pitch = 1;
        sources[index_wind].Play(); // always running in background

        SceneManager.sceneLoaded += ClearAfterSceneChange;
    }

    private void Update() {
        // Make pitch and volume of the wind sfx a function of the player's speed
        if(Player.PlayerInstance.isActiveAndEnabled && Time.timeScale > 0) {
            float velocity = Player.PlayerIronSteel.rb.velocity.magnitude;
            if (velocity > velocityThreshold && !Player.PlayerPewter.IsGrounded) {
                float factor = Mathf.Exp(-windVelocityFactor / (velocity - velocityThreshold));
                sources[index_wind].volume = Mathf.Lerp(sources[index_wind].volume, factor * windVolumeVactor, Time.deltaTime * windLerpFactor);
                sources[index_wind].pitch = Mathf.Lerp(sources[index_wind].pitch, 1 + factor, Time.deltaTime * windLerpFactor);
            } else {
                sources[index_wind].volume = Mathf.Lerp(sources[index_wind].volume, 0, Time.deltaTime * windLerpFactor);
                sources[index_wind].pitch = Mathf.Lerp(sources[index_wind].pitch, 1, Time.deltaTime * windLerpFactor);
            }
        }
    }

    public void Clear() {
        StopAllCoroutines();
        SetMasterPitch(1);
        sources[index_wind].volume = 0;
        sources[index_wind].pitch = 1;
    }

    private void ClearAfterSceneChange(Scene scene, LoadSceneMode mode) {
        // For most scenes, stop playing any sceneTransition music
        if(!SceneSelectMenu.IsTutorial(scene.buildIndex)) {
            sources[index_sceneTransition].Stop();
        }
        sources[index_sceneTransition].loop = false;
    }

    public void SetAudioLevels(float master, float music, float effects, float voiceBeeps) {
        mixer.SetFloat("volumeMaster", Mathf.Log10(master) * 20); // logarithmic volume slider
        mixer.SetFloat("volumeMusic", Mathf.Log10(music) * 20);
        mixer.SetFloat("volumeEffects", Mathf.Log10(effects) * 20);
        mixer.SetFloat("volumeVoiceBeeps", Mathf.Log10(voiceBeeps) * 20);
    }
    public void SetMasterPitch(float pitch) {
        mixer.SetFloat("pitchMaster", pitch);
    }


    // Play() commands for sound effects
    public void Play_title_screen_loop() {
        sources[index_sceneTransition].clip = title_screen_loop;
        sources[index_sceneTransition].loop = true;
        sources[index_sceneTransition].Play();
    }

    
    public void Play_menu_select() {
        sources[index_shared].PlayOneShot(menu_select);
    }

}
