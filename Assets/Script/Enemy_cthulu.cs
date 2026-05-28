using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_cthulu : MonoBehaviour
{
    [Header("Targeting")]
    public Rigidbody2D target;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Attack 1 (Melee/Swipe) Settings")]
    public float attack1Distance = 1.5f;
    public float attack1Duration = 1.0f;

    [Header("Grab (Tentacle) Settings")]
    public float grabDistance = 3.5f;
    public float grabYThreshold = 0.5f;
    public float grabDuration = 1.5f;
    public float grabCooldown = 5.0f;

    [Header("Common Attack Settings")]
    public float attackCooldown = 2.0f;

    bool isLive = true;
    bool isAttacking = false;
    bool canAttack = true;
    bool canGrab = true;

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
        if (!isLive || target == null || isAttacking) return;

        float distance = Vector2.Distance(rigid.position, target.position);
        float yDifference = Mathf.Abs(target.position.y - rigid.position.y);

        if (canAttack)
        {
            // [수정 1] 그랩 쿨타임이 돌았다면 평타를 완전 봉인하고 무조건 그랩만 노림
            if (canGrab)
            {
                if (distance <= grabDistance && yDifference <= grabYThreshold)
                {
                    StartCoroutine(GrabAttackRoutine());
                }
            }
            // 그랩 쿨타임이 돌고 있을 때만 평타를 허용
            else
            {
                if (distance <= attack1Distance)
                {
                    StartCoroutine(MeleeAttackRoutine());
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!isLive) return;

        if (!isAttacking)
        {
            float distance = Vector2.Distance(rigid.position, target.position);
            float yDifference = Mathf.Abs(target.position.y - rigid.position.y);

            // 그랩이 가능할 때는 그랩 조건만 따짐 (평타 거리는 무시)
            bool canDoGrabNow = (canGrab && distance <= grabDistance && yDifference <= grabYThreshold);
            // 평타는 그랩 쿨타임일 때만 따짐
            bool canDoMeleeNow = (!canGrab && distance <= attack1Distance);

            if (!canDoGrabNow && !canDoMeleeNow)
            {
                // 다가가거나 Y축 각을 재기 위해 이동
                Vector2 dirVec = (target.position - rigid.position).normalized;
                Vector2 nextVec = dirVec * moveSpeed * Time.fixedDeltaTime;
                rigid.MovePosition(rigid.position + nextVec);
            }
            else
            {
                // 공격 사거리 및 조건에 충족하면 정지
                rigid.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            rigid.linearVelocity = Vector2.zero;
        }
    }

    void LateUpdate()
    {
        if (!isLive || target == null) return;

        if (!isAttacking)
        {
            float distance = Vector2.Distance(rigid.position, target.position);
            float yDifference = Mathf.Abs(target.position.y - rigid.position.y);

            bool isMoving = true;

            if (canGrab)
            {
                // 그랩 대기 중일 때는 그랩 사거리 & Y축이 맞으면 이동 애니메이션 정지
                if (distance <= grabDistance && yDifference <= grabYThreshold)
                    isMoving = false;
            }
            else
            {
                // 평타 대기 중일 때는 평타 사거리에 들어오면 이동 애니메이션 정지
                if (distance <= attack1Distance)
                    isMoving = false;
            }

            anim.SetFloat("speed", isMoving ? moveSpeed : 0f);
            spriter.flipX = target.position.x < rigid.position.x;
        }
        else
        {
            anim.SetFloat("speed", 0f);
        }
    }

    // [수정 2] 일반 평타 공격 코루틴 (거리가 멀어지면 중간에 캔슬됨)
    IEnumerator MeleeAttackRoutine()
    {
        isAttacking = true;
        canAttack = false;

        // attack1도 grab처럼 Bool 방식으로 켭니다
        anim.SetBool("attack1", true);

        float timer = 0f;
        while (timer < attack1Duration)
        {
            float currentDist = Vector2.Distance(rigid.position, target.position);

            // 공격 도중에 플레이어가 도망가서 거리가 멀어지면 루프 탈출 (공격 강제 취소)
            if (currentDist > attack1Distance)
            {
                break;
            }

            timer += Time.deltaTime;
            yield return null; // 매 프레임마다 거리를 검사
        }

        // 공격 시간이 다 끝났거나, 도중에 취소되었으면 Bool을 끄고 Idle로 돌아감
        anim.SetBool("attack1", false);
        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // 그랩 공격 코루틴
    IEnumerator GrabAttackRoutine()
    {
        isAttacking = true;
        canAttack = false;

        anim.SetBool("grab", true);

        StartCoroutine(GrabCooldownRoutine());

        // 그랩은 한 번 발동하면 끝까지 시전한다고 가정 (원한다면 위처럼 while문으로 바꿀 수 있음)
        yield return new WaitForSeconds(grabDuration);

        anim.SetBool("grab", false);
        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    IEnumerator GrabCooldownRoutine()
    {
        canGrab = false;
        yield return new WaitForSeconds(grabCooldown);
        canGrab = true;
    }

    public void Die()
    {
        if (!isLive) return;

        isLive = false;
        StopAllCoroutines();

        anim.SetBool("attack1", false); // 죽을 때 평타 모션 강제 종료
        anim.SetBool("grab", false);    // 죽을 때 그랩 모션 강제 종료
        anim.SetTrigger("dead");

        if (coll != null) coll.enabled = false;
        rigid.simulated = false;

        Destroy(gameObject, 1.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attack1Distance);

        Gizmos.color = new Color(0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, grabDistance);
    }
}