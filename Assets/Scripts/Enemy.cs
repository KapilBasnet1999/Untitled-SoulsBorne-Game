using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] float attackCD = 3f;
    [SerializeField] float attackRange = 1f;
    [SerializeField] float aggroRange = 4f;

    [Header("Patrol Settings")]
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] float patrolWaitTime = 2f;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;  
    [SerializeField] private AudioClip screamSound;    
    [SerializeField] private AudioClip attackSound;    

    private GameObject player;
    private Animator animator;
    private NavMeshAgent agent;
    private EnemyStats stats;

    private int currentPatrolIndex = 0;
    private float patrolTimer = 0f;
    private float attackTimer = 0f;
    private float destinationUpdateTimer = 0f;
    private float destinationUpdateCooldown = 0.5f;

    private bool isDead = false;

    void Start()
    {
        stats = GetComponent<EnemyStats>();
        player = GameObject.FindWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Ensure the AudioSource is attached to the enemy
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        animator.SetFloat("speed", agent.velocity.magnitude / agent.speed);

        if (distanceToPlayer <= aggroRange)
        {
            ChasePlayer(distanceToPlayer);
        }
        else
        {
            Patrol();
        }

        attackTimer += Time.deltaTime;
    }

    void ChasePlayer(float distanceToPlayer)
    {
        destinationUpdateTimer -= Time.deltaTime;

        if (destinationUpdateTimer <= 0f)
        {
            agent.SetDestination(player.transform.position);
            destinationUpdateTimer = destinationUpdateCooldown;
        }

        Vector3 lookPos = player.transform.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        // Play scream sound when enemy starts chasing the player
        if (distanceToPlayer <= aggroRange && !audioSource.isPlaying && screamSound != null)
        {
            PlaySound(screamSound);  
        }

        if (distanceToPlayer <= attackRange && attackTimer >= attackCD)
        {
            animator.SetTrigger("attack");
            attackTimer = 0f;
            PlaySound(attackSound); 
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolTimer += Time.deltaTime;

            if (patrolTimer >= patrolWaitTime)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                patrolTimer = 0f;
            }
        }
        else
        {
            patrolTimer = 0f;
        }
    }

    public void DealDamage()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (distanceToPlayer <= attackRange * 1.2f)
        {
            CharacterStats playerStats = player.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(10); 
            }
        }
    }

    public void OnDeath()
    {
        isDead = true;
        agent.isStopped = true;

        if (animator != null)
        {
            animator.SetTrigger("die");
        }
    }

    // TEMP test: Damage when clicked
    void OnMouseDown()
    {
        if (stats != null && !stats.IsDead())
        {
            stats.TakeDamage(20);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }

    // Play sound through AudioSource
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
