using System.Collections;
using UnityEngine;

// Drives the hit-flash and dissolve properties of the Shader Graph material.
// Listens to the Damageable on the same object, so no Inspector wiring is needed.
[RequireComponent(typeof(Damageable))]
public class DamageFeedback : MonoBehaviour
{
    [Header("Renderers (leave empty to find them automatically)")]
    [SerializeField] Renderer[] renderers;

    [Header("Hit Flash")]
    [SerializeField] Color flashColor = Color.red;
    [SerializeField] float flashDuration = 0.15f;
    [Tooltip("Flash strength over the flash. Left = start, right = end.")]
    [SerializeField] AnimationCurve flashCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Header("Dissolve (on death)")]
    [SerializeField] float dissolveDelay = 0.3f;
    [SerializeField] float dissolveDuration = 1.5f;
    [SerializeField] AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Seconds after dissolving before the dummy comes back. 0 = never respawns.")]
    [SerializeField] float respawnDelay = 0f;

    // These must match the Reference names of the properties in the Shader Graph
    static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
    static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
    static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");

    Damageable damageable;
    MaterialPropertyBlock block;
    Coroutine flashRoutine;
    float currentFlash;
    float currentDissolve;

    void Awake()
    {
        damageable = GetComponent<Damageable>();
        block = new MaterialPropertyBlock();

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();
    }

    void OnEnable()
    {
        damageable.onDamaged.AddListener(OnDamaged);
        damageable.onDied.AddListener(OnDied);
    }

    void OnDisable()
    {
        damageable.onDamaged.RemoveListener(OnDamaged);
        damageable.onDied.RemoveListener(OnDied);
    }

    void Start() => Push();

    void OnDamaged(float amount)
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    void OnDied() => StartCoroutine(DeathRoutine());

    IEnumerator FlashRoutine()
    {
        float duration = Mathf.Max(0.01f, flashDuration);
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            currentFlash = flashCurve.Evaluate(t / duration);
            Push();
            yield return null;
        }
        currentFlash = 0f;
        Push();
        flashRoutine = null;
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(dissolveDelay);
        yield return Dissolve(0f, 1f);

        if (respawnDelay > 0f)
        {
            yield return new WaitForSeconds(respawnDelay);
            damageable.Revive();
            yield return Dissolve(1f, 0f);
        }
    }

    IEnumerator Dissolve(float from, float to)
    {
        float duration = Mathf.Max(0.01f, dissolveDuration);
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            currentDissolve = Mathf.Lerp(from, to, dissolveCurve.Evaluate(t / duration));
            Push();
            yield return null;
        }
        currentDissolve = to;
        Push();
    }

    // Sends the current values to every renderer without creating new material instances
    void Push()
    {
        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(block);
            block.SetColor(FlashColorId, flashColor);
            block.SetFloat(FlashAmountId, currentFlash);
            block.SetFloat(DissolveAmountId, currentDissolve);
            r.SetPropertyBlock(block);
        }
    }
}