using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField] private Transform cam; 

    void LateUpdate()
    {
        if (cam != null)
        {
            transform.LookAt(transform.position + cam.forward);
        }
    }

    // Optional: fallback if not assigned manually
    void Start()
    {
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }
    }
}
