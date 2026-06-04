using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_bear1 : MonoBehaviour
{
    [Header("Movement")]
    public float speed;
    public Rigidbody2D target;
    public Vector2 targetOffset = new Vector2(0f, -0.5f);
    public float stopDistance = 0.5f;

    [Header("Attack Settings")]
    public float attackRange = 2.0f;    // 플레이어와 이 거리만큼 가까워지면 공격 시작
    public float attackRadius = 1.8f;   // 실제 내려찍기 판정 범위
    public float attackDelay = 1.2f;    // 장판이 차오르는 시간
    public float attackCooldown = 4f;
    public LayerMask playerLayer;

    [Header("Visual Settings")]
    public Transform warningCircle;     // 자식으로 둔 원형 스프라이트
    public Color areaColor = new Color(1f, 0f, 0f, 0.4f);

    bool isLive = true;
    bool isAttacking = false;
    bool canAttack = true;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    SpriteRenderer circleRenderer;
    Animator anim;
    Collider2D coll; // [추가] 충돌체 제어용

    float currentMoveSpeed = 0f;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>(); // [추가] 컴포넌트 가져오기

        if (warningCircle != null)
        {
            circleRenderer = warningCircle.GetComponent<SpriteRenderer>();
            circleRenderer.color = areaColor;
            warningCircle.SetParent(null); // 장판을 부모에서 분리
            warningCircle.gameObject.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if (!isLive || target == null) return;

        Vector2 actualTargetPos = (Vector2)target.position + targetOffset;
        float distanceToTarget = Vector2.Distance(rigid.position, actualTargetPos);

        // 1. 공격 조건 체크
        if (distanceToTarget <= attackRange && canAttack && !isAttacking)
        {
            StartCoroutine(SlamAttackRoutine());
        }

        // 2. 이동 로직
        if (!isAttacking && distanceToTarget > stopDistance)
        {
            Vector2 dirVec = actualTargetPos - rigid.position;
            Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
            rigid.linearVelocity = Vector2.zero;

            currentMoveSpeed = speed;
        }
        else
        {
            currentMoveSpeed = 0f;
        }
    }

    void LateUpdate()
    {
        anim.SetFloat("speed", currentMoveSpeed);

        if (!isLive || target == null || isAttacking) return;

        spriter.flipX = target.position.x < rigid.position.x;
    }

    IEnumerator SlamAttackRoutine()
    {
        isAttacking = true;
        canAttack = false;

        anim.SetBool("isAttacking", true);

        // 공격 시작 시점의 플레이어 위치를 저장 (발밑 오프셋 포함)
        Vector3 strikePosition = (Vector2)target.position + targetOffset;

        // 장판 위치를 플레이어 위치로 이동시키고 활성화
        if (warningCircle != null)
        {
            warningCircle.position = strikePosition;
            warningCircle.gameObject.SetActive(true);
            warningCircle.localScale = Vector3.zero;
        }

        // --- 전조 단계 (장판이 플레이어 위치에서 커짐) ---
        float timer = 0f;
        while (timer < attackDelay)
        {
            timer += Time.deltaTime;
            float progress = timer / attackDelay;

            if (warningCircle != null)
            {
                float currentScale = Mathf.Lerp(0f, attackRadius * 2f, progress);
                warningCircle.localScale = new Vector3(currentScale, currentScale, 1f);
            }
            yield return null;
        }

        // --- 타격 단계 ---
        if (warningCircle != null) warningCircle.gameObject.SetActive(false);

        Debug.Log("쾅!!");
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(strikePosition, attackRadius, playerLayer);
        foreach (Collider2D player in hitPlayers)
        {
            Debug.Log("<color=red>플레이어 타격!</color>");
        }

        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
        anim.SetBool("isAttacking", false);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // [추가] 몹이 죽을 때 호출할 함수
    public void Die()
    {
        if (!isLive) return; // 이미 죽었다면 중복 실행 방지

        isLive = false;

        // 공격 코루틴 진행 중에 죽으면 멈춤 (장판 커지는 것 정지 및 데미지 판정 취소)
        StopAllCoroutines();

        // [중요] 부모에서 분리해둔 장판 오브젝트가 씬에 영구적으로 남지 않도록 파괴
        if (warningCircle != null)
        {
            Destroy(warningCircle.gameObject);
        }

        // 1. 애니메이터 Trigger 발동
        anim.SetTrigger("dead");

        // 2. 물리 및 충돌 비활성화
        if (coll != null) coll.enabled = false;
        rigid.simulated = false;

        // 3. 애니메이션 길이에 맞춰 오브젝트 삭제 (예: 1.0초 뒤)
        Destroy(gameObject, 1.0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}