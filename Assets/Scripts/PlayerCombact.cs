using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] float[] attackRanges = new float[3] { 1.5f, 1.8f, 2f };
    [SerializeField] float[] attackDamages = new float[3] { 20f, 25f, 30f };
    [SerializeField] float[] attackCooldowns = new float[3] { 1f, 1.5f, 2f };
    [SerializeField] LayerMask enemyLayer;

    [Header("Weapon State")]
    [SerializeField] private bool isSwordEquipped = true;
    
    private Animator animator;
    private float[] cooldownTimers;
    private int currentAttackIndex = 0;
    private bool canAttack = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        cooldownTimers = new float[3];
    }

    void Update()
    {
        UpdateCooldowns();
        
        // Completely block attack input when sword isn't equipped
        if (!isSwordEquipped)
        {
            canAttack = false;
            return;
        }
        
        canAttack = true;
        HandleAttackInput();
    }

    void UpdateCooldowns()
    {
        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0)
            {
                cooldownTimers[i] -= Time.deltaTime;
            }
        }
    }

    void HandleAttackInput()
    {
        if (!canAttack) return;

        if (Input.GetButtonDown("Attack1") && cooldownTimers[0] <= 0)
        {
            ExecuteAttack(0);
        }
        else if (Input.GetButtonDown("Attack2") && cooldownTimers[1] <= 0)
        {
            ExecuteAttack(1);
        }
        else if (Input.GetButtonDown("Attack3") && cooldownTimers[2] <= 0)
        {
            ExecuteAttack(2);
        }
    }

    void ExecuteAttack(int attackIndex)
    {
        if (!canAttack) return;

        currentAttackIndex = attackIndex;
        animator.SetTrigger($"Attack{attackIndex + 1}");
        cooldownTimers[attackIndex] = attackCooldowns[attackIndex];
    }

    // Animation event - will only be called if animation plays
    public void StartDealDamage()
    {
        if (!canAttack) return;
        
        DealDamage();
        Debug.Log("StartDealDamage animation event triggered");
    }

    // Animation event - will only be called if animation plays
    public void EndDealDamage()
    {
        if (!canAttack) return;
        Debug.Log("EndDealDamage animation event triggered");
    }

    public void DealDamage()
    {
        if (!canAttack) return;

        Collider[] hitEnemies = Physics.OverlapSphere(
            transform.position,
            attackRanges[currentAttackIndex],
            enemyLayer
        );

        foreach (Collider enemy in hitEnemies)
        {
            EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage((int)attackDamages[currentAttackIndex]);
                Debug.Log($"Player dealt {attackDamages[currentAttackIndex]} damage to {enemy.name}");
            }
        }
    }

    public void SetSwordEquipped(bool equipped)
    {
        isSwordEquipped = equipped;
        canAttack = equipped;
        
        // Immediately cancel any ongoing attacks
        if (!equipped)
        {
            animator.ResetTrigger("Attack1");
            animator.ResetTrigger("Attack2");
            animator.ResetTrigger("Attack3");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < attackRanges.Length; i++)
        {
            Gizmos.DrawWireSphere(transform.position, attackRanges[i]);
        }
    }
}