// ComboController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ComboController : MonoBehaviour
{
    [SerializeField] ComboChain chain;
    [SerializeField] PlayerMovement movement;

    [Header("Hit Detection")]
    [Tooltip("Empty child object at chest height, facing forward. Falls back to this transform.")]
    [SerializeField] Transform attackOrigin;
    [Tooltip("Only colliders on these layers can be hit")]
    [SerializeField] LayerMask hitLayers = ~0;

    [Header("Events (HUD, audio, VFX listen here)")]
    public UnityEvent<int> onComboStepChanged;
    public UnityEvent<AttackData> onAttackStarted;
    public UnityEvent<AttackData, Vector3> onHit;   // attack used, world-space hit point
    public UnityEvent onComboReset;

    Animator animator;
    int step = -1;
    bool attacking;
    bool hitPerformed;
    int queuedCount;
    float attackTimer, lastEndTime;

    readonly Collider[] hitBuffer = new Collider[16];
    readonly HashSet<Damageable> hitThisSwing = new HashSet<Damageable>();

    Transform Origin => attackOrigin != null ? attackOrigin : transform;
    Vector3 GetHitCenter(AttackData a) => Origin.position + Origin.forward * a.hitRange;

    void Awake() => animator = GetComponentInChildren<Animator>();

    public void RequestAttack()
    {
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
                queuedCount++;
        }
    }

    int NextStep() => (step + 1) % chain.attacks.Length;

    void StartAttack(int newStep)
    {
        step = newStep;
        attacking = true;
        hitPerformed = false;
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
            attackTimer += Time.deltaTime;
            var data = chain.attacks[step];

            if (!hitPerformed && attackTimer >= data.hitTime * data.duration)
            {
                hitPerformed = true;
                PerformHit(data);
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
            onComboStepChanged?.Invoke(0);
            onComboReset?.Invoke();
            queuedCount = 0;
        }
    }

    void PerformHit(AttackData data)
    {
        Vector3 center = GetHitCenter(data);
        int count = Physics.OverlapSphereNonAlloc(
            center, data.hitRadius, hitBuffer, hitLayers, QueryTriggerInteraction.Ignore);

        hitThisSwing.Clear();
        for (int i = 0; i < count; i++)
        {
            // GetComponentInParent so a collider on a child bone still finds the root's Damageable
            var target = hitBuffer[i].GetComponentInParent<Damageable>();
            if (target == null || target.IsDead || !hitThisSwing.Add(target)) continue;

            target.TakeDamage(data.damage);
            onHit?.Invoke(data, hitBuffer[i].bounds.ClosestPoint(center));
        }
    }

    void EndAttack()
    {
        attacking = false;
        movement.isAttacking = false;
        lastEndTime = Time.time;
        animator.CrossFadeInFixedTime("Blend Tree", chain.returnFadeTime);
    }

    // Select the player in the Hierarchy to see the hit spheres in the Scene view.
    // Faint white = every attack in the chain. In Play mode the active attack turns
    // yellow while waiting for its hit frame, then red once the hit check has run.
    void OnDrawGizmosSelected()
    {
        if (chain == null || chain.attacks == null) return;

        for (int i = 0; i < chain.attacks.Length; i++)
        {
            var a = chain.attacks[i];
            if (a == null) continue;

            bool active = Application.isPlaying && attacking && i == step;
            Gizmos.color = active
                ? (hitPerformed ? Color.red : Color.yellow)
                : new Color(1f, 1f, 1f, 0.25f);
            Gizmos.DrawWireSphere(GetHitCenter(a), a.hitRadius);
        }
    }
}