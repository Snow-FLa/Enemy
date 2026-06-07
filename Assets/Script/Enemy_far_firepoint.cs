using UnityEngine;

public class Enemy_far_firepoint : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 2f;
    public float attackRange = 5f;

    [Header("공격 설정")]
    public GameObject projectilePrefab;
    public float attackCooldown = 1.5f;

    // 💡 추가됨: 투사체가 시작될 정확한 위치를 담을 변수
    [Tooltip("투사체가 생성될 '빈 오브젝트'를 여기에 드래그해서 넣으세요.")]
    public Transform firePoint;

    [Header("타겟 설정")]
    public Transform playerTransform;

    private float lastAttackTime;
    private SpriteRenderer spriter;
    private Animator anim;
    private bool isLive = true;

    void Start()
    {
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isLive || playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        FlipSprite();

        if (distanceToPlayer > attackRange)
        {
            ChasePlayer();
        }
        else
        {
            AttackPlayer();
        }
    }

    private void FlipSprite()
    {
        if (spriter != null)
        {
            spriter.flipX = playerTransform.position.x < transform.position.x;
        }
    }

    private void ChasePlayer()
    {
        anim.SetFloat("speed", moveSpeed);
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    private void AttackPlayer()
    {
        anim.SetFloat("speed", 0);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            anim.SetTrigger("attack");
            FireProjectile();
            lastAttackTime = Time.time;
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null) return;

        // 💡 핵심 변경: firePoint가 연결되어 있다면 그 위치에서, 안 되어있으면 기본 발바닥 위치에서 생성합니다.
        Vector2 spawnPos = firePoint != null ? firePoint.position : transform.position;

        // 발바닥(transform.position) 대신 새로 설정한 위치(spawnPos)에서 스폰!
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // 날아갈 방향은 발사구가 아니라 여전히 '몹의 몸통 -> 플레이어' 방향으로 계산
        Vector2 direction = (playerTransform.position - (Vector3)spawnPos).normalized;

        Enemy_far_attack projScript = projectile.GetComponent<Enemy_far_attack>();
        if (projScript != null) projScript.SetDirection(direction);
    }

    public void Die()
    {
        if (!isLive) return;

        isLive = false;
        anim.SetTrigger("dead");

        if (GetComponent<Collider2D>())
            GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 2f);
    }
}