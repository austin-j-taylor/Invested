using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

    private const int index_shared = 0,
                      index_pewter = 1,
                      index_rolling = 2;

    [SerializeField]
    AudioMixer mixer = null;
    [SerializeField]
    AudioClip menu_select = null,
                pewter_burst = null,
                pewter_intro = null,
                pewter_loop = null,
                pewter_end = null,
                rolling_loop = null;

    private Coroutine coroutine_pewter, coroutine_rolling;

    AudioSource[] sources;

    private void Start() {
        sources = GetComponents<AudioSource>();
    }

    public void Clear() {
        StopAllCoroutines();
        SetMasterPitch(1);
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

    public void Play_menu_select() {
        sources[index_shared].PlayOneShot(menu_select);
    }

    public void Play_pewter_burst() {
        sources[index_shared].PlayOneShot(pewter_burst);
    }
    public void Play_pewter() {
        if (coroutine_pewter != null)
            StopCoroutine(coroutine_pewter);

        coroutine_pewter = StartCoroutine(Playing_pewter_start_loop());
    }
    public void Stop_pewter() {
        if (coroutine_pewter != null)
            StopCoroutine(coroutine_pewter);

        coroutine_pewter = StartCoroutine(Playing_pewter_end());
    }

    public void Play_rolling() {
        if (coroutine_rolling != null)
            StopCoroutine(coroutine_rolling);

        coroutine_rolling = StartCoroutine(Playing_rolling_start_loop());
    }
    public void Stop_rolling() {
        if (coroutine_rolling != null)
            StopCoroutine(coroutine_rolling);

        coroutine_rolling = StartCoroutine(Playing_rolling_end());
    }

    private IEnumerator Playing_pewter_start_loop() {
        // wait until the last sound effect is done
        sources[index_pewter].loop = false;
        while (sources[index_pewter].isPlaying) {
            yield return null;
        }

        // start the starting sound effect
        sources[index_pewter].clip = pewter_intro;
        sources[index_pewter].loop = false;
        sources[index_pewter].Play();
        while (sources[index_pewter].isPlaying) {
            yield return null;
        }
        sources[index_pewter].clip = pewter_loop;
        sources[index_pewter].loop = true;
        sources[index_pewter].Play();
    }
    private IEnumerator Playing_pewter_end() {
        // wait until the last sound effect is done
        sources[index_pewter].loop = false;
        while (sources[index_pewter].isPlaying) {
            yield return null;
        }
        sources[index_pewter].clip = pewter_end;
        sources[index_pewter].Play();
    }

    private IEnumerator Playing_rolling_start_loop() {
        // wait until the last sound effect is done
        sources[index_rolling].loop = false;
        while (sources[index_rolling].isPlaying) {
            yield return null;
        }

        //// start the starting sound effect
        //sources[index_rolling].clip = pewter_intro;
        //sources[index_rolling].loop = false;
        //sources[index_rolling].Play();
        //while (sources[index_rolling].isPlaying) {
        //    yield return null;
        //}
        sources[index_rolling].clip = rolling_loop;
        sources[index_rolling].loop = true;
        sources[index_rolling].Play();
    }
    private IEnumerator Playing_rolling_end() {
        // wait until the last sound effect is done
        sources[index_rolling].loop = false;
        while (sources[index_rolling].isPlaying) {
            yield return null;
        }
        //sources[index_rolling].clip = rolling_end;
        //sources[index_rolling].Play();
    }
}
