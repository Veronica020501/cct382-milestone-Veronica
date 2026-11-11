using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ComboController : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] ComboChain chain;
    [SerializeField] PlayerMovement movement;

    [Header("Hit Detection")]
    [Tooltip("Optional. The hit sphere is placed in front of this transform. Defaults to the object this script is on.")]
    [SerializeField] Transform attackOrigin;
    [Tooltip("Only colliders on these layers can be hit.")]
    [SerializeField] LayerMask hitLayers = ~0;
    [SerializeField] bool showGizmos = true;

    [Header("Events (HUD, audio, VFX listen here)")]
    public UnityEvent<int> onComboStepChanged;
    public UnityEvent<AttackData> onAttackStarted;
    public UnityEvent<AttackData, Vector3> onHitLanded;   // attack data + world point of the hit
    public UnityEvent onComboReset;

    Animator animator;
    int step = -1;
    bool attacking, hitDone;
    int queuedCount;
    float attackTimer, lastEndTime;

    // remembered for the gizmo flash
    Vector3 lastHitCenter;
    float lastHitRadius;
    float lastHitTime = -999f;

    void Awake() => animator = GetComponentInChildren<Animator>();

    public void RequestAttack()
    {
        if (!IsSetupValid()) return;

        if (!attacking)
        {
            bool expired = step < 0 || Time.time - lastEndTime > chain.comboResetTime;
            StartAttack(expired ? 0 : NextStep());
        }
        else
        {
            var data = chain.attacks[step];
            if (attackTimer >= data.comboWindowStart * data.duration
                && queuedCount < chain.maxBufferedPresses)
                queuedCount++;   // input buffering
        }
    }

    int NextStep() => (step + 1) % chain.attacks.Length;

    void StartAttack(int newStep)
    {
        step = newStep;
        attacking = true;
        hitDone = false;
        attackTimer = 0f;
        movement.isAttacking = true;

        var data = chain.attacks[step];
        animator.CrossFadeInFixedTime(data.stateName, data.crossFadeTime);

        onComboStepChanged?.Invoke(step + 1);
        onAttackStarted?.Invoke(data);
    }

    void Update()
    {
        if (attacking)
        {
            var data = chain.attacks[step];
            attackTimer += Time.deltaTime;

            // The hit lands once, at hitTime
            if (!hitDone && attackTimer >= Mathf.Min(data.hitTime, data.duration))
            {
                hitDone = true;
                DoHit(data);
            }

            if (attackTimer >= data.duration)
            {
                if (queuedCount > 0)
                {
                    queuedCount--;
                    StartAttack(NextStep());
                }
                else EndAttack();
            }
        }
        else if (step >= 0 && Time.time - lastEndTime > chain.comboResetTime)
        {
            step = -1;
            queuedCount = 0;
            onComboStepChanged?.Invoke(0);
            onComboReset?.Invoke();
        }
    }

    void EndAttack()
    {
        attacking = false;
        movement.isAttacking = false;
        lastEndTime = Time.time;
        animator.CrossFadeInFixedTime("Blend Tree", chain.returnFadeTime);
    }

    // ---------- Hit detection ----------

    Vector3 GetHitCenter(AttackData data)
    {
        Transform origin = attackOrigin != null ? attackOrigin : transform;
        return origin.position + origin.forward * data.hitRange + Vector3.up * data.hitHeight;
    }

    void DoHit(AttackData data)
    {
        Vector3 center = GetHitCenter(data);

        // remember for the gizmo flash
        lastHitCenter = center;
        lastHitRadius = data.hitRadius;
        lastHitTime = Time.time;

        Collider[] colliders = Physics.OverlapSphere(
            center, data.hitRadius, hitLayers, QueryTriggerInteraction.Ignore);

        // One target can have several colliders; only damage it once per swing
        var alreadyHit = new HashSet<Damageable>();

        foreach (Collider col in colliders)
        {
            Damageable target = col.GetComponentInParent<Damageable>();
            if (target == null || target.IsDead || !alreadyHit.Add(target)) continue;

            target.TakeDamage(data.damage);
            onHitLanded?.Invoke(data, col.bounds.ClosestPoint(center));
        }
    }

    bool IsSetupValid()
    {
        if (chain == null || chain.attacks == null || chain.attacks.Length == 0 || movement == null)
        {
            Debug.LogWarning("ComboController: assign the Chain (with at least one attack) and Movement fields.", this);
            return false;
        }

        foreach (var a in chain.attacks)
        {
            if (a == null)
            {
                Debug.LogWarning("ComboController: one of the Attacks slots in the chain is empty.", this);
                return false;
            }
        }
        return true;
    }

    // ---------- Gizmos ----------

    void OnDrawGizmos()
    {
        if (!showGizmos || chain == null || chain.attacks == null) return;

        // Red flash where the last hit landed (Play mode)
        if (Application.isPlaying && Time.time - lastHitTime < 0.25f)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(lastHitCenter, lastHitRadius);
        }

        if (Application.isPlaying)
        {
            // Show only the active attack's hit volume
            if (attacking && step >= 0 && step < chain.attacks.Length && chain.attacks[step] != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(GetHitCenter(chain.attacks[step]), chain.attacks[step].hitRadius);
            }
        }
        else
        {
            // In the editor, show every attack's hit volume so you can tune them
            for (int i = 0; i < chain.attacks.Length; i++)
            {
                if (chain.attacks[i] == null) continue;
                Gizmos.color = Color.HSVToRGB(i / (float)chain.attacks.Length, 0.9f, 1f);
                Gizmos.DrawWireSphere(GetHitCenter(chain.attacks[i]), chain.attacks[i].hitRadius);
            }
        }
    }
}