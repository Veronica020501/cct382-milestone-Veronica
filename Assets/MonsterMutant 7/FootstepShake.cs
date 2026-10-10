using UnityEngine;
using Unity.Cinemachine;

// Shakes the camera each time a foot lands.
// Put this on the same GameObject as the Animator (and PlayerMovement).
// The walk and run clips call Footstep() through Animation Events.
[RequireComponent(typeof(CinemachineImpulseSource))]
public class FootstepShake : MonoBehaviour
{
    [Header("Shake Strength")]
    public float walkForce = 0.1f;
    public float sprintForce = 0.1f;

    private PlayerMovement player;
    private CinemachineImpulseSource impulseSource;

    void Awake()
    {
        player = GetComponent<PlayerMovement>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // Called by Animation Events on the walk/run clips.
    // The event passes info about which clip fired it.
    public void Footstep(AnimationEvent evt)
    {
        // In a blend tree, clips that are barely visible also fire their events.
        // Ignore those so you get one stomp per step, not several.
        if (evt.animatorClipInfo.weight < 0.5f) return;

        float force = player.isSprinting ? sprintForce : walkForce;

        // Downward kick, like the ground being hit
        impulseSource.GenerateImpulse(Vector3.down * force);
    }
}