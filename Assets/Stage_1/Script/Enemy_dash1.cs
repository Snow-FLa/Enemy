using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_dash1 : MonoBehaviour
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

    bool isLive = true;
    bool isDashing = false;
    bool canDash = true;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    Collider2D coll; // [추가] 죽었을 때 충돌 판정을 없애기 위한 변수

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>(); // [추가] 컴포넌트 가져오기
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

    // [추가] 몹이 죽을 때 호출할 함수
    public void Die()
    {
        if (!isLive) return; // 이미 죽었다면 중복 실행 방지

        isLive = false;

        // 대쉬 중이거나 대기 중인 모든 코루틴 강제 종료 (대쉬 도중 멈춤)
        StopAllCoroutines();

        // 1. 애니메이터 Trigger 발동
        anim.SetTrigger("Dead");

        // 2. 물리 및 충돌 비활성화
        if (coll != null) coll.enabled = false;
        rigid.simulated = false; // 물리 연산을 완전히 꺼서 시체가 밀리지 않게 함

        // 3. 애니메이션 길이에 맞춰 오브젝트 삭제 (필요 시 시간 조절 또는 삭제 구문 제거)
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