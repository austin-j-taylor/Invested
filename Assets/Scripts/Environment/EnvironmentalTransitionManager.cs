using UnityEngine;
using System.Collections;

/// <summary>
/// Automatically sets interior/exterior BGM to be in the BGM audio mixer group
/// </summary>
public class EnvironmentalTransitionManager : MonoBehaviour {
    
    private const float crossfadeTime = 3;
    private AudioSource[] interiors, exteriors;

    private float interiorVolume = 0;

    private void Awake() {
        interiors = transform.Find("Interiors").GetComponents<AudioSource>();
        exteriors = transform.Find("Exteriors").GetComponents<AudioSource>();

        for (int i = 0; i < interiors.Length; i++) {
            interiors[i].outputAudioMixerGroup = GameManager.AudioManager.MixerBGMGroup;
            interiors[i].loop = true;
        }
        for (int i = 0; i < exteriors.Length; i++) {
            exteriors[i].outputAudioMixerGroup = GameManager.AudioManager.MixerBGMGroup;
            exteriors[i].loop = true;
        }
    }
    public void StartInterior() {
        GameManager.CloudsManager.FadeCloudsOutImmediate();
        interiorVolume = 1;
        foreach (AudioSource source in interiors) {
            source.Play();
            source.volume = 1;
        }
        foreach (AudioSource source in exteriors) {
            source.Play();
            source.volume = 0;
        }
    }
    public void StartExterior() {
        interiorVolume = 0;
        foreach (AudioSource source in interiors) {
            source.Play();
            source.volume = 0;
        }
        foreach (AudioSource source in exteriors) {
            source.Play();
            source.volume = 1;
        }
    }

    public void SetExteriorVolume(float vol) {
        interiorVolume = 1 - vol;
        for (int i = 0; i < exteriors.Length; i++) {
            exteriors[i].volume = vol;
        }
    }

    //private void Transition(AudioSource[] froms, AudioSource[] tos) {
    //    StopAllCoroutines();
    //    int i;
    //    for (i = 0; i < tos.Length; i++) {
    //        if (i >= froms.Length) {
    //            StartCoroutine(FadeIn(tos[i]));
    //        } else {
    //            StartCoroutine(Crossfade(froms[i], tos[i]));
    //        }
    //    }
    //    while (i < froms.Length) {
    //        StartCoroutine(FadeOut(froms[i]));
    //        i++;
    //    }
    //}

    public void EnterInterior(TriggerMusic trigger) {
        StopAllCoroutines();
        StartCoroutine(FadeInterior());
        GameManager.CloudsManager.FadeCloudsOut(crossfadeTime);
    }
    
    public void ExitInterior(TriggerMusic trigger) {
        StopAllCoroutines();
        StartCoroutine(FadeExterior());
        GameManager.CloudsManager.FadeCloudsIn(crossfadeTime);
    }

    private IEnumerator FadeInterior() {
        while (interiorVolume < 1) {
            interiorVolume += Time.deltaTime / crossfadeTime;
            for (int i = 0; i < interiors.Length; i++)
                interiors[i].volume = interiorVolume;
            for (int i = 0; i < exteriors.Length; i++)
                exteriors[i].volume = 1 - interiorVolume;

            yield return null;
        }
        for (int i = 0; i < interiors.Length; i++)
            interiors[i].volume = 1;
        for (int i = 0; i < exteriors.Length; i++)
            exteriors[i].volume = 0;
    }
    private IEnumerator FadeExterior() {
        while (interiorVolume > 0) {
            interiorVolume -= Time.deltaTime / crossfadeTime;
            for (int i = 0; i < interiors.Length; i++)
                interiors[i].volume = interiorVolume;
            for (int i = 0; i < exteriors.Length; i++)
                exteriors[i].volume = 1 - interiorVolume;

            yield return null;
        }
        for (int i = 0; i < interiors.Length; i++)
            interiors[i].volume = 0;
        for (int i = 0; i < exteriors.Length; i++)
            exteriors[i].volume = 1;
    }

    //private IEnumerator Crossfade(AudioSource from, AudioSource to) {
    //    float count = 0;
    //    while (count < 1) {
    //        count += Time.deltaTime / crossfadeTime;
    //        from.volume = 1 - count;
    //        to.volume = count;
    //        yield return null;
    //    }
    //    from.volume = 0;
    //    to.volume = 1;
    //}
    //private IEnumerator FadeIn(AudioSource to) {
    //    float count = 0;
    //    while (count < 1) {
    //        count += Time.deltaTime / crossfadeTime;
    //        to.volume = count;
    //        yield return null;
    //    }
    //    to.volume = 1;
    //}
    //private IEnumerator FadeOut(AudioSource from) {
    //    float count = 0;
    //    while (count < 1) {
    //        count += Time.deltaTime / crossfadeTime;
    //        from.volume = 1 - count;
    //        yield return null;
    //    }
    //    from.volume = 0;
    //}
}
