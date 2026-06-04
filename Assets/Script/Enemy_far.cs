using UnityEngine;

public class Enemy_far : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 2f;
    public float attackRange = 5f;

    [Header("공격 설정")]
    public GameObject projectilePrefab;
    public float attackCooldown = 1.5f;

    [Header("타겟 설정")]
    public Transform playerTransform;

    private float lastAttackTime;
    private SpriteRenderer spriter;
    private Animator anim; // 💡 애니메이터 변수 추가
    private bool isLive = true; // 💡 생존 여부 변수 추가

    void Start()
    {
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>(); // 💡 컴포넌트 초기화
    }

    void Update()
    {
        // 죽었거나 타겟이 없으면 로직 정지
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
        // 💡 이동할 때는 speed 파라미터를 moveSpeed로 설정 (Run 재생)
        anim.SetFloat("speed", moveSpeed);

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    private void AttackPlayer()
    {
        // 💡 공격 사거리 안에서는 멈추므로 speed를 0으로 설정 (Idle 재생)
        anim.SetFloat("speed", 0);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // 💡 공격 트리거 발동
            anim.SetTrigger("attack");
            FireProjectile();
            lastAttackTime = Time.time;
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null) return;
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        Enemy_far_attack projScript = projectile.GetComponent<Enemy_far_attack>();
        if (projScript != null) projScript.SetDirection(direction);
    }

    // 💡 사망 시 호출할 함수
    public void Die()
    {
        if (!isLive) return;

        isLive = false;
        anim.SetTrigger("dead"); // 💡 사망 트리거 발동

        // 시체가 충돌을 막지 않게 콜라이더 끄기
        if (GetComponent<Collider2D>())
            GetComponent<Collider2D>().enabled = false;

        // 선택사항: 2초 뒤 삭제
        Destroy(gameObject, 2f);
    }
}