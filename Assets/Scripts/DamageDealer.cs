using UnityEngine;
using System.Collections.Generic;

public class DamageDealer : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] float weaponLength = 1f;
    [SerializeField] float[] attackDamages = new float[3] { 20f, 25f, 30f };
    [SerializeField] LayerMask targetLayer;

    private bool canDealDamage;
    private List<GameObject> hasDealtDamage;
    private int currentAttackType = 0;

    private void Awake()
    {
        hasDealtDamage = new List<GameObject>();
    }

    private void Update()
    {
        if (!canDealDamage) return;

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, weaponLength, targetLayer))
        {
            if (!hasDealtDamage.Contains(hit.transform.gameObject))
            {
                DealDamageToTarget(hit.transform.gameObject);
                hasDealtDamage.Add(hit.transform.gameObject);
            }
        }
    }

    private void DealDamageToTarget(GameObject target)
    {
        CharacterStats targetStats = target.GetComponent<CharacterStats>();
        if (targetStats != null)
        {
            targetStats.TakeDamage(Mathf.RoundToInt(attackDamages[currentAttackType]));
        }
    }

    // Animation Event Callbacks
    public void StartDealDamage(int attackType)
    {
        currentAttackType = Mathf.Clamp(attackType - 1, 0, attackDamages.Length - 1);
        canDealDamage = true;
        hasDealtDamage.Clear();
    }

    public void EndDealDamage()
    {
        canDealDamage = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = canDealDamage ? Color.red : Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * weaponLength);
    }
}
