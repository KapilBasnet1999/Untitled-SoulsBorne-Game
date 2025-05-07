using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthBar;

    private bool isDead = false;
    private Animator animator;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    private void Update()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (animator != null)
        {
            animator.SetTrigger("damage");
        }

        Debug.Log($"{gameObject.name} took {damage} damage. Current HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("die");
        }

        // Hide health bar
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false); 
        }

        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.OnDeath();
        }
    }

    public bool IsDead()
    {
        return isDead;
    }
}
