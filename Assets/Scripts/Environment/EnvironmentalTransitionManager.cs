using UnityEngine;
using System.Collections;

// Handles fading/transitioning of indoor vs. outdoor music and cloud effects.
public class EnvironmentalTransitionManager : MonoBehaviour {

    private const float crossfadeTime = 3;
    private AudioSource[] interiors, exteriors;

    private void Awake() {
        interiors = transform.Find("Interiors").GetComponents<AudioSource>();
        exteriors = transform.Find("Exteriors").GetComponents<AudioSource>();
    }

    public void StartInterior() {
        GameManager.CloudsManager.FadeCloudsOutImmediate();
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
        foreach (AudioSource source in interiors) {
            source.Play();
            source.volume = 0;
        }
        foreach (AudioSource source in exteriors) {
            source.Play();
            source.volume = 1;
        }
    }


    private void Transition(AudioSource[] froms, AudioSource[] tos) {
        int i;
        for (i = 0; i < tos.Length; i++) {
            if (i >= froms.Length) {
                StartCoroutine(FadeIn(tos[i]));
            } else {
                StartCoroutine(Crossfade(froms[i], tos[i]));
            }
        }
        while (i < froms.Length) {
            StartCoroutine(FadeOut(froms[i]));
            i++;
        }
    }

    public void EnterInterior(TriggerMusic trigger) {
        Transition(exteriors, interiors);
        GameManager.CloudsManager.FadeCloudsOut(crossfadeTime);
    }
    
    public void ExitInterior(TriggerMusic trigger) {
        Transition(interiors, exteriors);
        GameManager.CloudsManager.FadeCloudsIn(crossfadeTime);
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to) {
        float count = 0;
        while (count < 1) {
            count += Time.deltaTime / crossfadeTime;
            from.volume = 1 - count;
            to.volume = count;
            yield return null;
        }
        from.volume = 0;
        to.volume = 1;
    }
    private IEnumerator FadeIn(AudioSource to) {
        float count = 0;
        while (count < 1) {
            count += Time.deltaTime / crossfadeTime;
            to.volume = count;
            yield return null;
        }
        to.volume = 1;
    }
    private IEnumerator FadeOut(AudioSource from) {
        float count = 0;
        while (count < 1) {
            count += Time.deltaTime / crossfadeTime;
            from.volume = 1 - count;
            yield return null;
        }
        from.volume = 0;
    }
}
