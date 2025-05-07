using UnityEngine;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthBar;

    public SceneController sceneController;

    private bool isDead = false;
    private Enemy enemy;
    private Animator animator;

    private void Awake()
    {
        currentHealth = maxHealth;
        enemy = GetComponent<Enemy>();
        animator = GetComponent<Animator>();

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
            healthBar.wholeNumbers = true;
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

        if (enemy != null)
        {
            enemy.OnDeath();
        }

        DisableAllEnemies();

        if (sceneController != null)
        {
            sceneController.gameOver();
        }

        Debug.Log("Player Dead");
    }

    private void DisableAllEnemies()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy e in enemies)
        {
            MonoBehaviour[] scripts = e.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                script.enabled = false;
            }

            AudioSource audio = e.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Stop();
            }

            Animator anim = e.GetComponent<Animator>();
            if (anim != null)
            {
                anim.enabled = false;
            }
        }
    }

}
