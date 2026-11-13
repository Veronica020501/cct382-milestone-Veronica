using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Damageable targetDamageable;
    [SerializeField] private bool useWorldSpaceAboveTarget = false;
    [SerializeField] private Vector3 worldSpaceOffset = new Vector3(0f, 2.5f, 0f);

    private Slider slider;
    private Camera cam;
    private CanvasGroup canvasGroup; // Used to smoothly show/hide the health bar

    void Awake()
    {
        slider = GetComponent<Slider>();
        cam = Camera.main;

        // Ensure we have a CanvasGroup to toggle visibility easily
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        if (targetDamageable == null)
        {
            targetDamageable = GetComponentInParent<Damageable>();
            if (targetDamageable == null) targetDamageable = GetComponentInChildren<Damageable>();
        }

        if (targetDamageable != null)
        {
            targetDamageable.onDamaged.AddListener(OnTargetDamaged);
            targetDamageable.onDied.AddListener(OnTargetDied);
            UpdateBar();
        }
    }

    void OnDestroy()
    {
        if (targetDamageable != null)
        {
            targetDamageable.onDamaged.RemoveListener(OnTargetDamaged);
            targetDamageable.onDied.RemoveListener(OnTargetDied);
        }
    }

    void Update()
    {
        // Periodic check in case the dummy revived (e.g. your respawnDelay timer finished)
        if (targetDamageable != null)
        {
            bool shouldBeVisible = !targetDamageable.IsDead;
            if (canvasGroup.alpha > 0f != shouldBeVisible)
            {
                SetVisible(shouldBeVisible);
                if (shouldBeVisible) UpdateBar();
            }
        }

        if (useWorldSpaceAboveTarget && targetDamageable != null && cam != null && !targetDamageable.IsDead)
        {
            transform.position = cam.WorldToScreenPoint(targetDamageable.transform.position + worldSpaceOffset);
        }
    }

    void OnTargetDamaged(float amount)
    {
        UpdateBar();
    }

    void OnTargetDied()
    {
        SetVisible(false);
    }

    void UpdateBar()
    {
        if (targetDamageable == null || slider == null) return;

        float currentHealth = targetDamageable.CurrentHealth;
        float maxHealth = targetDamageable.MaxHealth;

        slider.value = Mathf.Clamp01(currentHealth / maxHealth);
    }

    void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}