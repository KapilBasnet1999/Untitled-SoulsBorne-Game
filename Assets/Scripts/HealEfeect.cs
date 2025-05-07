using UnityEngine;

public class HealEffect : MonoBehaviour
{
    private ParticleSystem healParticles;
    private CharacterStats playerStats;
    public int healAmount = 20;

    void Start()
    {
        healParticles = GetComponent<ParticleSystem>();
        playerStats = GetComponentInParent<CharacterStats>(); 

        if (healParticles != null)
        {
            healParticles.Stop();
        }
        else
        {
            Debug.LogWarning("No ParticleSystem found on HealEffect GameObject");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TryHeal();
        }
    }

    void TryHeal()
    {
        // Only heal if player exists and isn't at max health
        if (playerStats != null && playerStats.currentHealth < playerStats.maxHealth)
        {
            playerStats.currentHealth = Mathf.Min(
                playerStats.currentHealth + healAmount,
                playerStats.maxHealth
            );

            if (healParticles != null)
            {
                healParticles.Play();
            }

            Debug.Log($"Healed! Current HP: {playerStats.currentHealth}");
        }
    }
}