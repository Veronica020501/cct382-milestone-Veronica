using UnityEngine;

public class ComboAudioPlayer : MonoBehaviour
{
    [SerializeField] private ComboController comboController;
    [SerializeField] private AudioSource hitAudioSource; // Explicitly assign your combo audio source here

    void Awake()
    {
        if (comboController == null)
            comboController = GetComponent<ComboController>();

        if (hitAudioSource == null)
            hitAudioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        if (comboController != null)
            comboController.onHit.AddListener(PlayHitSound);
    }

    void OnDisable()
    {
        if (comboController != null)
            comboController.onHit.RemoveListener(PlayHitSound);
    }

    void PlayHitSound(AttackData attackData, Vector3 hitPoint)
    {
        if (attackData.swingSounds == null || attackData.swingSounds.Length == 0) return;

        AudioClip clip = attackData.swingSounds[Random.Range(0, attackData.swingSounds.Length)];
        if (clip == null) return;

        hitAudioSource.pitch = Random.Range(attackData.pitchRange.x, attackData.pitchRange.y);
        float randomVolume = Random.Range(attackData.volumeRange.x, attackData.volumeRange.y);

        hitAudioSource.PlayOneShot(clip, randomVolume);
    }
}