using UnityEngine;
using Unity.Cinemachine;

// Shakes the camera and plays a stomp sound each time a foot lands.
// Put this on the same GameObject as the Animator (and PlayerMovement).
// The walk and run clips call Footstep() through Animation Events.
[RequireComponent(typeof(CinemachineImpulseSource))]
[RequireComponent(typeof(AudioSource))]
public class FootstepShake : MonoBehaviour
{
    [Header("Shake Strength")]
    public float walkForce = 0.3f;
    public float sprintForce = 0.8f;

    [Header("Stomp Sound")]
    public AudioClip[] stompClips;       // one or more stomp sounds (a random one plays each step)
    public float walkVolume = 0.6f;
    public float sprintVolume = 1f;
    public float pitchVariation = 0.1f;  // random +/- pitch so steps don't sound identical

    private PlayerMovement player;
    private CinemachineImpulseSource impulseSource;
    private AudioSource audioSource;

    void Awake()
    {
        player = GetComponent<PlayerMovement>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        audioSource = GetComponent<AudioSource>();
    }

    // Called by Animation Events on the walk/run clips.
    // The event passes info about which clip fired it.
    public void Footstep(AnimationEvent evt)
    {
        // In a blend tree, clips that are barely visible also fire their events.
        // Ignore those so you get one stomp per step, not several.
        if (evt.animatorClipInfo.weight < 0.5f) return;

        bool sprinting = player.isSprinting;

        // Camera shake: a downward kick, like the ground being hit
        float force = sprinting ? sprintForce : walkForce;
        impulseSource.GenerateImpulse(Vector3.down * force);

        // Stomp sound
        PlayStompSound(sprinting);
    }

    private void PlayStompSound(bool sprinting)
    {
        if (stompClips == null || stompClips.Length == 0) return;

        AudioClip clip = stompClips[Random.Range(0, stompClips.Length)];

        audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.PlayOneShot(clip, sprinting ? sprintVolume : walkVolume);
    }
}