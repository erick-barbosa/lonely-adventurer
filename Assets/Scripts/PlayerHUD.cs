using System;
using System.Collections;
using UnityEngine;

public class PlayerHUD : MonoBehaviour {
    private HealthSystem playerHealthSystem;
    private Animator healthAnimator;

    private void Awake() {
        healthAnimator = GetComponent<Animator>();

        playerHealthSystem = GameObject.FindGameObjectWithTag("Player").GetComponent<HealthSystem>();

        playerHealthSystem.OnHit += ChangeHealthBarSprite;
        playerHealthSystem.OnHeal += ChangeHealthBarSprite;
    }

    private void OnDisable() {
        playerHealthSystem.OnHit -= ChangeHealthBarSprite;
        playerHealthSystem.OnHeal -= ChangeHealthBarSprite;
    }

    private void ChangeHealthBarSprite() {
        int health = playerHealthSystem.CurrentHealth;

        healthAnimator.Play("Life" + health);
    }

    private void Update() {
    
    }
}
