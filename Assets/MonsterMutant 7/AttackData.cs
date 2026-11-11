using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack Data")]
public class AttackData : ScriptableObject
{
    [Header("Animation")]
    public string stateName = "Attack1";   // must match the Animator state
    public float crossFadeTime = 0.1f;
    [Tooltip("Seconds until the next hit can start / the attack ends")]
    public float duration = 0.6f;
    [Tooltip("Fraction of duration after which a press is buffered")]
    [Range(0f, 1f)] public float comboWindowStart = 0.4f;

    [Header("Hit Detection")]
    [Tooltip("Seconds after the attack starts when the hit lands. Match this to the contact frame of the swing. Keep it below Duration.")]
    public float hitTime = 0.25f;
    [Tooltip("Radius of the hit sphere")]
    public float hitRadius = 0.8f;
    [Tooltip("How far in front of the attack origin the sphere is centered")]
    public float hitRange = 1.0f;
    [Tooltip("How far above the attack origin the sphere is centered")]
    public float hitHeight = 1.0f;

    [Header("Damage")]
    public float damage = 10f;
    public float knockbackForce = 5f;   // not used yet

    [Header("Feedback (used in the audio/VFX steps)")]
    public AudioClip[] swingSounds;
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);
    public Vector2 volumeRange = new Vector2(0.8f, 1f);
    public GameObject hitVFX;
}