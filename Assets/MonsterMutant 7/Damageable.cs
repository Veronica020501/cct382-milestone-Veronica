using UnityEngine;
using UnityEngine.Events;

// Put this on anything that can be hit (the training dummy, later real enemies).
public class Damageable : MonoBehaviour
{
    [SerializeField] float maxHealth = 40f;
    public float MaxHealth => maxHealth;

    public UnityEvent<float> onDamaged;   // amount of damage taken
    public UnityEvent onDied;

    public float CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0f;

    void Awake() => CurrentHealth = maxHealth;

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        onDamaged?.Invoke(amount);

        if (IsDead) onDied?.Invoke();
    }

    public void Revive() => CurrentHealth = maxHealth;
}