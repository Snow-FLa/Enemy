using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_dash : MonoBehaviour
{
    [Header("Targeting")]
    public Rigidbody2D target;
    [Tooltip("플레이어의 중심이 아닌 발밑을 추적하도록 좌표를 조정합니다.")]
    public Vector2 targetOffset = new Vector2(0f, -0.5f);
    public float stopDistance = 0.5f;

    [Header("Dash Settings")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float dashPrepTime = 0.5f;
    public float dashCooldown = 1.5f;

    [Header("Attack Settings")] // 💡 추가: 데미지 설정
    public int dashDamage = 10;

    bool isLive = true;
    bool isDashing = false;
    bool canDash = true;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    Collider2D coll;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (!isLive || target == null) return;

        Vector2 actualTargetPos = (Vector2)target.position + targetOffset;
        float distanceToTarget = Vector2.Distance(rigid.position, actualTargetPos);

        if (canDash && !isDashing && distanceToTarget > stopDistance)
        {
            StartCoroutine(DashRoutine(actualTargetPos));
        }
    }

    void FixedUpdate()
    {
        if (!isLive) return;

        if (!isDashing)
        {
            rigid.linearVelocity = Vector2.zero;
        }
    }

    void LateUpdate()
    {
        if (!isLive || target == null) return;

        if (!isDashing)
        {
            spriter.flipX = target.position.x < rigid.position.x;
        }
    }

    IEnumerator DashRoutine(Vector2 targetPos)
    {
        canDash = false;

        // 1. 대쉬 준비
        yield return new WaitForSeconds(dashPrepTime);

        // 2. 대쉬 방향 설정 및 애니메이션 시작
        isDashing = true;
        anim.SetBool("isDashing", true);

        Vector2 dashDir = (targetPos - rigid.position).normalized;

        // 3. 대쉬 실행
        float timer = 0f;
        while (timer < dashDuration)
        {
            rigid.linearVelocity = dashDir * dashSpeed;
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 4. 대쉬 종료 및 애니메이션 종료
        isDashing = false;
        anim.SetBool("isDashing", false);

        rigid.linearVelocity = Vector2.zero;

        // 5. 쿨타임 대기
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // 💡 추가: 대쉬 중 플레이어와 물리적으로 부딪혔을 때 호출
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 살아있고, '대쉬 중'일 때 부딪힌 대상이 'Player'라면
        if (isLive && isDashing && collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("대쉬 적중! 플레이어에게 데미지: " + dashDamage);

            // TODO: 플레이어 체력을 깎는 코드를 이곳에 작성합니다.
            // 예시: collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(dashDamage);
        }
    }

    public void Die()
    {
        if (!isLive) return;

        isLive = false;
        StopAllCoroutines();
        anim.SetTrigger("Dead");

        if (coll != null) coll.enabled = false;
        rigid.simulated = false;

        Destroy(gameObject, 1.0f);
    }

    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.blue;
            Vector2 actualTargetPos = (Vector2)target.position + targetOffset;
            Gizmos.DrawWireSphere(actualTargetPos, 0.2f);
        }
    }
}