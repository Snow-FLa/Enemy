using UnityEngine;

public class Enemy_far1 : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("적의 이동 속도")]
    public float moveSpeed = 2f;
    [Tooltip("공격을 시작할 사정거리")]
    public float attackRange = 5f;

    [Header("공격 설정")]
    [Tooltip("발사할 투사체 프리팹")]
    public GameObject projectilePrefab;
    [Tooltip("공격 쿨타임 (초)")]
    public float attackCooldown = 1.5f;

    [Header("타겟 설정")]
    public Transform playerTransform;

    private float lastAttackTime;

    // 💡 추가됨: SpriteRenderer를 담을 변수
    private SpriteRenderer spriter;

    void Start()
    {
        // 시작할 때 자신의 오브젝트에 붙어있는 SpriteRenderer 컴포넌트를 찾아서 넣습니다.
        spriter = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (playerTransform == null) return;

        // 플레이어와의 현재 거리 계산
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 💡 매 프레임 플레이어 위치를 확인해 좌우 반전 처리
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
        // SpriteRenderer가 정상적으로 연결되어 있을 때만 실행
        if (spriter != null)
        {
            // 💡 말씀하신 코드를 기존 변수명에 맞춘 완벽한 한 줄!
            // 플레이어의 x좌표가 적의 x좌표보다 작으면(왼쪽에 있으면) flipX를 true로 만듭니다.
            spriter.flipX = playerTransform.position.x < transform.position.x;

            // *참고: 만약 몹 기본 이미지가 왼쪽을 보고 있다면 부등호를 반대(>)로 바꿔주시면 됩니다.
        }
    }

    private void ChasePlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    private void AttackPlayer()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
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
        if (projScript != null)
        {
            projScript.SetDirection(direction);
        }
    }
}