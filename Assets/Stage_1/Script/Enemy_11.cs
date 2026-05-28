using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_11 : MonoBehaviour
{
    [Header("--- Spec ---")]
    public float speed;
    public float attackRange;    // 공격 사거리
    public float attackCooldown; // 공격 쿨타임
    public Rigidbody2D target;

    [Header("--- State ---")]
    bool isLive = true;
    bool isAttacking = false;
    float timer;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim; // 애니메이션 제어를 위해 추가

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (!isLive || isAttacking) return;

        // 1. 타겟과의 거리 계산
        float distance = Vector2.Distance(target.position, rigid.position);

        // 2. 사거리 안에 들어왔는지 확인
        if (distance <= attackRange)
        {
            StopAndAttack();
        }
        else
        {
            Move();
        }
    }

    void Move()
    {
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.linearVelocity = Vector2.zero;

        // 이동 애니메이션 (Walk/Run 파라미터가 있다면)
        anim.SetBool("isRun", true);
    }

    void StopAndAttack()
    {
        anim.SetBool("isRun", false);

        // 쿨타임 계산
        timer += Time.fixedDeltaTime;
        if (timer >= attackCooldown)
        {
            StartCoroutine(AttackRoutine());
            timer = 0;
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // 애니메이터에 'Attack' 트리거 전달
        anim.SetTrigger("doAttack");

        // 공격 애니메이션이 재생되는 동안 잠시 대기 (약 0.5초 ~ 애니메이션 길이에 맞춰 조절)
        // 실제 데미지 판정은 애니메이션 이벤트(Animation Event)를 사용하는 것이 가장 정확합니다.
        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
    }

    void LateUpdate()
    {


        if (!isLive) return;
        // 타겟 방향에 따라 이미지 반전
        spriter.flipX = target.position.x < rigid.position.x;
    }
}