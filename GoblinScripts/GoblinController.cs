using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GoblinController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Движение")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Прыжок")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private int maxJumps = 2;

    [Header("Стена")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpForceX = 12f;
    [SerializeField] private float wallJumpForceY = 10f;

    [Header("Метание")]
    [SerializeField] private GameObject shurikenPrefab;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private int maxShurikens = 3;

    [Header("Выносливость")]
    [SerializeField] private StaminaBar staminaBar;
    [SerializeField] private float staminaRegenTime = 5f;

    [Header("Лестница")]
    [SerializeField] private float climbSpeed = 4f;
    [SerializeField] private LayerMask ladderLayer;

    [Header("Здоровье")] 
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private int healthBonusPerLevel = 1;  
    
    [Header("Завершение уровня")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private float healthMultiplier = 1.5f;

    [Header("Звуки")]
    [SerializeField] private AudioClip shurikenSound;
    [SerializeField] private float shurikenVolume = 1f;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float deathVolume = 1f;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private float winVolume = 1f;

    private Rigidbody2D rb;
    private float moveInput;
    private int facingDirection = 1;
    private int jumpsLeft;
    private int shurikensLeft;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isOnLadder;
    private bool wasOnLadder;
    private bool staminaLocked;
    private float staminaTimer;
    private float climbInput;
    private float defaultGravityScale;
    private int currentHealth;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 platformVelocity;
    public static int PersistentMaxHealth = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultGravityScale = rb.gravityScale;

        if (PersistentMaxHealth > 0)
            maxHealth = PersistentMaxHealth;
        else
            PersistentMaxHealth = maxHealth;   

        currentHealth = maxHealth;
        jumpsLeft = maxJumps;
        shurikensLeft = maxShurikens;

        if (staminaBar != null) staminaBar.SetStamina(1f);
        if (healthBar != null) healthBar.SetHealth((float)currentHealth / maxHealth);
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        climbInput = Input.GetAxisRaw("Vertical");

        if (moveInput > 0)
            facingDirection = -1;
        else if (moveInput < 0)
            facingDirection = 1;

        if (animator != null)
        {
            bool isRunning = Mathf.Abs(moveInput) > 0.1f && isGrounded;
            animator.SetBool("IsRunning", isRunning);
        }
        
        if (animator != null)
            animator.SetBool("IsOnLadder", isOnLadder);
        
        transform.localScale = new Vector3(facingDirection, 1, 1);

        if (staminaLocked)
        {
            staminaTimer -= Time.deltaTime;
            float fill = 1f - (staminaTimer / staminaRegenTime);
            if (staminaBar != null)
                staminaBar.SetStamina(fill);

            if (staminaTimer <= 0)
            {
                staminaLocked = false;
                shurikensLeft = maxShurikens;
                if (staminaBar != null)
                    staminaBar.SetStamina(1f);
            }
        }


        if (isOnLadder)
        {
            float climbInput = Input.GetAxisRaw("Vertical");

            if (Mathf.Abs(moveInput) > 0.5f)
            {
                isOnLadder = false;
                rb.gravityScale = defaultGravityScale;
            }
            else if (Input.GetButtonDown("Jump"))
            {
                isOnLadder = false;
                rb.gravityScale = 3;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }
        if (!isOnLadder && Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
                GroundJump();
            else if (isTouchingWall)
                WallJump();
            else if (jumpsLeft > 0)
                AirJump();
        }

        if (Input.GetKeyDown(KeyCode.E) && !staminaLocked && shurikensLeft > 0)
        {
            ThrowShuriken();
        }
    }

    private void FixedUpdate()
    {
        wasOnLadder = isOnLadder;
        isOnLadder = Physics2D.OverlapCircle(transform.position, 0.4f, ladderLayer);

        if (isOnLadder)
        {
            float climbInput = Input.GetAxisRaw("Vertical");
            rb.linearVelocity = new Vector2(0, climbInput * climbSpeed);
            return;
        }

        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.15f, groundLayer);

     
        if (isGrounded)
        {
            jumpsLeft = maxJumps;

            RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.15f, groundLayer);
            if (hit.rigidbody != null)
            {
                MovingPlatformHorizontal horiz = hit.rigidbody.GetComponent<MovingPlatformHorizontal>();
                MovingPlatformVertical vert = hit.rigidbody.GetComponent<MovingPlatformVertical>();

                if (horiz != null)
                    platformVelocity = horiz.Velocity;
                else if (vert != null)
                    platformVelocity = vert.Velocity;
                else
                    platformVelocity = hit.rigidbody.linearVelocity; 
            }
            else
            {
                platformVelocity = Vector2.zero;
            }
        }
        else
        {
            platformVelocity = Vector2.zero;
        }

        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }

        Vector2 targetVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (isGrounded && Mathf.Abs(moveInput) < 0.1f)
        {
            targetVelocity.x = platformVelocity.x;
        }
        else if (isGrounded && Mathf.Abs(moveInput) > 0.1f)
        {
            targetVelocity.x += platformVelocity.x;
        }

        rb.linearVelocity = targetVelocity;
    }

    private void GroundJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpsLeft = maxJumps - 1;
    }

    private void WallJump()
    {
        float jumpDirection = -facingDirection;
        rb.linearVelocity = new Vector2(jumpDirection * wallJumpForceX, wallJumpForceY);
        facingDirection = -facingDirection;
        jumpsLeft = maxJumps - 1;
    }

    private void AirJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpsLeft = 0;
    }

    private void ThrowShuriken()
    {
        if (shurikenPrefab == null || attackPoint == null)
        {
            Debug.Log("Префаб или точка атаки пустые");
            return;
        }
        
        if (animator != null)
            animator.SetTrigger("Throw");

        if (shurikenSound != null)
        {
            GameObject soundObject = new GameObject("ShurikenSound");
            soundObject.transform.position = transform.position;
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = shurikenSound;
            audioSource.volume = shurikenVolume;
            audioSource.Play();
            Destroy(soundObject, shurikenSound.length);
        }
        
        Vector2 throwDirection = facingDirection == 1 ? Vector2.left : Vector2.right;
        Instantiate(shurikenPrefab, attackPoint.position, Quaternion.identity).GetComponent<Shuriken>().Launch(throwDirection);
        Debug.Log("Сюрикен брошен");
        
        shurikensLeft--;
        staminaBar.SetStamina((float)shurikensLeft / maxShurikens);

        if (shurikensLeft <= 0)
        {
            staminaLocked = true;
            staminaTimer = staminaRegenTime;
        }
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (healthBar != null)
            
            healthBar.SetHealth((float)currentHealth / maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (deathSound != null)
        {
            GameObject soundObject = new GameObject("DeathSound");
            soundObject.transform.position = transform.position;
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = deathSound;
            audioSource.volume = deathVolume;
            audioSource.Play();
            Destroy(soundObject, deathSound.length);
        }
        if (deathPanel != null)  deathPanel.SetActive(true);
        
        Time.timeScale = 0f;
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & ladderLayer) != 0)
        {
            if (!isOnLadder)
            {
                Debug.Log("ЗАЛЕТЕЛ НА ЛЕСТНИЦУ");
                isOnLadder = true;
                rb.gravityScale = 0;
            }
        }
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            if (!isTouchingWall)
                isTouchingWall = true;
        }
    }
    
    private void LevelComplete()
    {
        if (winSound != null)
        {
            GameObject soundObj = new GameObject("WinSound");
            soundObj.transform.position = transform.position;
            AudioSource audioSource = soundObj.AddComponent<AudioSource>();
            audioSource.clip = winSound;
            audioSource.volume = winVolume;
            audioSource.Play();
            Destroy(soundObj, winSound.length);
        }

        maxHealth += healthBonusPerLevel;         
        currentHealth = maxHealth;
        PersistentMaxHealth = maxHealth;       

        if (healthBar != null) healthBar.SetHealth((float)currentHealth / maxHealth);

        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
      
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
            isTouchingWall = true;
        if
            (other.CompareTag("EnemyProjectile"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Chest"))
        {
            LevelComplete();
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & ladderLayer) != 0)
        {
            isOnLadder = false;
            rb.gravityScale = defaultGravityScale;
        }

        if (((1 << other.gameObject.layer) & groundLayer) != 0)
            isTouchingWall = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 0.15f);
        }
    }
}