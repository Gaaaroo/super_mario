using UnityEngine;

public class DragonBossAI : MonoBehaviour
{
    public Transform player;
    public GameObject fireballPrefab;
    public Transform firePoint;

    public Health health;
    private bool phase2 = false;

    public float attackCooldown = 2f;
    public float moveSpeed = 2f;

    private float timer;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        FacePlayer();
        Move();
        AttackTimer();
        if (!phase2 && health.currentHealth <= health.maxHealth / 2)
        {
            phase2 = true;
            attackCooldown = 1f;
            moveSpeed = 4f;
        }
    }

    private void Move()
    {
        float direction = player.position.x > transform.position.x ? 1 : -1;

        transform.position += new Vector3(direction * moveSpeed * Time.deltaTime, 0, 0);
    }

    private void AttackTimer()
    {
        timer += Time.deltaTime;

        if (timer > attackCooldown)
        {
            Attack();
            timer = 0;
        }
    }

    private void Attack()
    {
        float direction = player.position.x > transform.position.x ? 1 : -1;
        Quaternion rotation = direction > 0 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, rotation);
        fireball.GetComponent<Fireball>().speed = 12f;
    }

    private void FacePlayer()
    {
        if (player == null) return;

        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }
}