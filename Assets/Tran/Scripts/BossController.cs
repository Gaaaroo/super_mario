using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    [Header("Health & UI")]
    public int maxHealth = 100;

    private int currentHealth;
    public Slider healthBar;

    [Header("Attack Settings")]
    public Transform fightPoint;

    public GameObject fireballPrefab;
    public float fireRate = 2f;
    private float nextFireTime = 0f;

    [Header("Movement & AI")]
    public Transform player;

    public float moveSpeed = 2f;
    public float chaseDistance = 100f;
    public float stopDistance = 4f;

    [Header("Rage Mode")]
    private bool isRageMode = false;

    public float rageFireRate = 1f;
    public float rageMoveSpeed = 4f;

    private bool isFacingRight = true;

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        LookAtPlayer();

        if (distanceToPlayer < chaseDistance && distanceToPlayer > stopDistance)
        {
            ChasePlayer();
        }

        if (Time.time >= nextFireTime)
        {
            Shoot();
            float currentRate = isRageMode ? rageFireRate : fireRate;
            nextFireTime = Time.time + currentRate;
        }
    }

    private void LookAtPlayer()
    {
        if (player.position.x > transform.position.x && !isFacingRight)
        {
            Flip();
        }
        else if (player.position.x < transform.position.x && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void ChasePlayer()
    {
        float currentSpeed = isRageMode ? rageMoveSpeed : moveSpeed;

        Vector2 targetPosition = new Vector2(player.position.x, transform.position.y);

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);
    }

    private void Shoot()
    {
        Instantiate(fireballPrefab, fightPoint.position, fightPoint.rotation);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (healthBar != null) healthBar.value = currentHealth;

        if (currentHealth <= maxHealth / 2 && !isRageMode)
        {
            EnterRageMode();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void EnterRageMode()
    {
        isRageMode = true;
        Debug.Log("Boss entered RAGE MODE!");
        GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 0.5f);
    }

    private void Die()
    {
        Debug.Log("Boss Defeated!");
        Destroy(gameObject);
    }
}