using UnityEngine;

public class HealthPowerUp : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D collision) {
        print(collision);
        if (collision.CompareTag("Player")) {
            HealthSystem healthSystem = collision.GetComponent<HealthSystem>();
            if (healthSystem != null) {
                healthSystem.Heal(5); // Heal the player by 1 point
                Destroy(gameObject); // Destroy the power-up after use
            }
        }
    }
}
