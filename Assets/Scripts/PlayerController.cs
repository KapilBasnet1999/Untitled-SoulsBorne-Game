using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator playerAnim;
    [SerializeField] private GameObject sword;
    [SerializeField] private GameObject swordOnShoulder;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private AudioSource audioSource;

    [Header("Sound Effects")]
    public AudioClip equipSound;
    public AudioClip blockSound;
    public AudioClip kickSound;
    public AudioClip attackSound1;
    public AudioClip attackSound2;
    public AudioClip attackSound3;
    public AudioClip walkSound;
    public AudioClip sprintSound;

    private bool isWalking = false;
    private float walkSpeedThreshold = 0.1f;
    private float sprintSpeedThreshold = 4f;

    [Header("Combat Parameters")]
    public float attackResetTime = 0.4f;

    [Header("States")]
    public bool isEquipping;
    public bool isEquipped;
    public bool isBlocking;
    public bool isKicking;
    public bool isAttacking;

    private float timeSinceAttack;
    private int currentAttack;
    private bool attackInput;
    private bool kickInput;
    private bool blockInput;
    private bool equipInput;
    public bool gameIsPaused = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (gameIsPaused) return;

        timeSinceAttack += Time.deltaTime;
        GetInput();
        HandleCombat();
        HandleWalkingSound();
    }

    private void GetInput()
    {
        equipInput = Input.GetKeyDown(KeyCode.R);
        blockInput = Input.GetKey(KeyCode.Mouse1);
        kickInput = Input.GetKey(KeyCode.LeftControl);
        attackInput = Input.GetMouseButtonDown(0);
    }

    private void HandleCombat()
    {
        if (playerAnim.GetBool("Grounded"))
        {
            HandleEquip();
            HandleBlock();
            HandleKick();
            HandleAttack();
        }
    }

    private void HandleEquip()
    {
        if (equipInput)
        {
            isEquipping = true;
            playerAnim.SetTrigger("Equip");
            PlaySound(equipSound);
        }
    }

    private void HandleBlock()
    {
        isBlocking = blockInput;
        playerAnim.SetBool("Block", isBlocking);
        if (isBlocking && !audioSource.isPlaying)
            PlaySound(blockSound);
    }

    private void HandleKick()
    {
        isKicking = kickInput;
        playerAnim.SetBool("Kick", isKicking);
        if (isKicking && !audioSource.isPlaying)
            PlaySound(kickSound);
    }

    private void HandleAttack()
    {
        if (attackInput && timeSinceAttack > attackResetTime && isEquipped)
        {
            currentAttack = currentAttack > 2 ? 1 : currentAttack + 1;
            isAttacking = true;
            playerAnim.ResetTrigger("Attack1");
            playerAnim.ResetTrigger("Attack2");
            playerAnim.ResetTrigger("Attack3");
            playerAnim.SetTrigger("Attack" + currentAttack);
            timeSinceAttack = 0;

            switch (currentAttack)
            {
                case 1: PlaySound(attackSound1); break;
                case 2: PlaySound(attackSound2); break;
                case 3: PlaySound(attackSound3); break;
            }
        }
    }

    public void ActiveWeapon()
    {
        sword.SetActive(!isEquipped);
        swordOnShoulder.SetActive(isEquipped);
        isEquipped = !isEquipped;
    }

    public void ResetAttack()
    {
        currentAttack = 0;
        isAttacking = false;
        timeSinceAttack = 0;
    }

    public void FinishEquip() => isEquipping = false;
    public void FinishAttack() => isAttacking = false;
    public void OnEquipFinish() => FinishEquip();
    public void OnAttackFinish() => FinishAttack();

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void HandleWalkingSound()
    {
        float speed = characterController.velocity.magnitude;

        bool shouldSprint = speed > sprintSpeedThreshold;
        bool shouldWalk = speed > walkSpeedThreshold && speed <= sprintSpeedThreshold;

        if (shouldSprint && (!audioSource.isPlaying || audioSource.clip != sprintSound))
        {
            isWalking = true;
            audioSource.clip = sprintSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (shouldWalk && (!audioSource.isPlaying || audioSource.clip != walkSound))
        {
            isWalking = true;
            audioSource.clip = walkSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (speed <= walkSpeedThreshold && isWalking)
        {
            isWalking = false;
            audioSource.loop = false;
            audioSource.Stop();
        }
    }
}
