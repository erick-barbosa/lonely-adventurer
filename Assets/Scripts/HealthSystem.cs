using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour {
    [SerializeField] private int maxHealth = 12;
    public int CurrentHealth { get; private set; }

    public delegate void OnHealthChanged(int currentHealth, int maxHealth);

    public event Action OnDeath;
    public event Action OnHit;
    public event Action OnHeal;

    private void Awake() {
        CurrentHealth = maxHealth;
    }

    public void ApplyDamage(float damage) {
        CurrentHealth -=(int)damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

        OnHit?.Invoke();

        if (CurrentHealth <= 0) {
            Die();
        }
    }

    public void Heal(int amount) {
        CurrentHealth += amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

        OnHeal?.Invoke();
    }

    private void Die() {
        OnDeath?.Invoke();
        Debug.Log($"{gameObject.name} has died!");
        //Destroy(gameObject);
    }
}
