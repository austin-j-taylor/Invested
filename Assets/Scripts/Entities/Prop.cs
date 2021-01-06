using UnityEngine;
using System.Collections;
/*
 * A Prop is an object in the scene that can be moved around, creating sound effects when hit.
 * The audiosource's Clip shouldn't be assigned in the inspector, as it gets assigned here
 *  based on the SoundMaterial.
 */
public class Prop : MonoBehaviour {

    protected const float impulseWeak = 1; // any fall speed above this -> weak impact
    protected const float impulseStrong = 10; // any fall speed above this -> strong impact
    protected const float expFactorGeneral = 40, expFactorCoin = 1000;

    private enum SoundMaterial { General, MetalDark, MetalLight, Tile, Coin }

    [SerializeField]
    private SoundMaterial soundMaterial = SoundMaterial.General;
    private Rigidbody rb;
    private AudioSource sound;
    private AudioClip strong, weak;
    private float expFactor = expFactorGeneral;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        sound = gameObject.AddComponent<AudioSource>();
        sound.playOnAwake = false;
        sound.spatialBlend = 1;

        switch (soundMaterial) {
            case SoundMaterial.General:
                strong = GameManager.AudioManager.prop_general_strong;
                weak = GameManager.AudioManager.prop_general_weak;
                break;
            case SoundMaterial.MetalDark:
                strong = GameManager.AudioManager.prop_metalDark_strong;
                weak = GameManager.AudioManager.prop_metalDark_weak;
                break;
            case SoundMaterial.MetalLight:
                strong = GameManager.AudioManager.prop_metalLight_strong;
                weak = GameManager.AudioManager.prop_metalLight_weak;
                break;
            case SoundMaterial.Tile:
                strong = GameManager.AudioManager.prop_Stone_strong;
                weak = GameManager.AudioManager.prop_Stone_weak;
                break;
            case SoundMaterial.Coin:
                strong = GameManager.AudioManager.prop_coin_strong;
                weak = GameManager.AudioManager.prop_coin_weak;
                expFactor = expFactorCoin;
                break;
        }
    }
    private void Start() {
        sound.outputAudioMixerGroup = GameManager.AudioManager.MixerEffectsGroup;
    }

    /// <summary>
    /// Plays this prop's sound effect of colliding with its current strong/weak sound effect.
    /// </summary>
    /// <param name="impulse">The strength of the sound effect.</param>
    public void PlayCollisionEffect(float impulse) {
        if (sound != null && sound.isActiveAndEnabled) {
            if (impulse > impulseStrong) {
                // Strong impulse: strong sound effect
                sound.clip = strong;
                sound.volume = 1 - Mathf.Exp(-impulse / expFactor);
                //Debug.Log("strong volume set to " + sound.volume + " from " + impulse + " from " + name, gameObject);
                sound.Play();
            } else if (impulse > impulseWeak) {
                // Weak impulse: weak sound effect
                sound.clip = weak;
                sound.volume = 1 - Mathf.Exp(-impulse / expFactor);
                //Debug.Log("weak volume set to " + sound.volume + " from " + impulse + " from " + name, gameObject);
                sound.Play();
            } else {
                //Debug.Log("Collision too weak: " + impulse + " from " + name, gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        // play this prop's sound effect
        //if(rb)
        //    PlayCollisionEffect(collision.impulse.magnitude / rb.mass);
        //else
        if(collision.transform.gameObject.activeSelf)
            PlayCollisionEffect(Vector3.Dot(collision.relativeVelocity, collision.GetContact(0).normal));
        //if (collision.impulse.magnitude > impulseWeak)
        //    Debug.Log("soft volume set to " + sound.volume + " from " + collision.impulse.magnitude + " from " + name + " against " + collision.gameObject.name, gameObject);
    }
}
