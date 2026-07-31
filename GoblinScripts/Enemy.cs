using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyRanged : MonoBehaviour
{
    [Header("Патруль")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 2f;

    [Header("Атака")]
    [SerializeField] private GameObject axePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwCooldown = 2f;
    [SerializeField] private float detectRange = 5f;

    [Header("Здоровье")]
    [SerializeField] private int maxHealth = 3;

    [Header("Дроп после смерти")] 
    [SerializeField] private GameObject ladderPrefab;
    [SerializeField] private Transform ladderSpawnPoint;
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private Transform chestSpawnPoint;
    
    [Header("Звуки")]
    [SerializeField] private AudioClip axeSound;
    [SerializeField] private float axeVolume = 1f;

    private Rigidbody2D rb;
    private int currentHealth;
    private float startX;
    private int direction = 1;
    private float throwTimer;
    private Transform player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        startX = transform.position.x;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (transform.position.x >= startX + patrolDistance)
            direction = -1;
        else if (transform.position.x <= startX - patrolDistance)
            direction = 1;

        if (GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().flipX = (direction == -1);

        throwTimer -= Time.deltaTime;

        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            if (dist <= detectRange && throwTimer <= 0)
            {
                ThrowAxe();
                throwTimer = throwCooldown;
            }
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveSpeed * direction, rb.linearVelocity.y);
    }

    private void ThrowAxe()
    {
        if (axePrefab == null)
        {
            Debug.Log("axePrefab не назначен в инспекторе");
            return;
        }
    
        if (throwPoint == null)
        {
            Debug.Log("throwPoint не назначен в инспекторе");
            return;
        }
    
        if (player == null)
        {
            Debug.Log("player не найден");
            return;
        }

        Vector2 dir = (player.position - throwPoint.position).normalized;
        if (axePrefab == null)
        {
            Debug.Log("axePrefab пустой!");
            return;
        }

        if (axeSound != null)
        {
            GameObject soundObject = new GameObject("ShurikenSound");
            soundObject.transform.position = transform.position;
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = axeSound;
            audioSource.volume = axeVolume;
            audioSource.Play();
            Destroy(soundObject, axeSound.length);
        }
        
        GameObject axe = Instantiate(axePrefab, throwPoint.position, Quaternion.identity);
        Projectile proj = axe.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Launch(dir);
        }
        else
        {
            Debug.Log("На префабе топора нет скрипта Projectile!");
        }
    }
    private void Die()
    {
        if (ladderPrefab != null && ladderSpawnPoint != null)
            Instantiate(ladderPrefab, ladderSpawnPoint.position, Quaternion.identity);

        if (chestPrefab != null && chestSpawnPoint != null)
            Instantiate(chestPrefab, chestSpawnPoint.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Shuriken"))
        {
            currentHealth--;
            if (currentHealth <= 0)
                Die();
        }
    }
}