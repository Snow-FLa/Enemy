using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.InputSystem;
using UnityEngine;

public class Enemy_cat : MonoBehaviour
{
    public float speed;
    public float stopDistance = 1.5f;
    public Rigidbody2D target;

    bool isLive = true;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    Collider2D coll; // [추가] 충돌체 제어를 위한 변수

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>(); // [추가] 충돌체 컴포넌트 가져오기
    }

    void FixedUpdate()
    {
        if (!isLive) return;

        Vector2 dirVec = target.position - rigid.position;
        float distance = dirVec.magnitude;

        if (distance <= stopDistance)
        {
            speed = 0f;
        }

        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);

        rigid.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        anim.SetFloat("speed", speed);

        if (!isLive) return;

        spriter.flipX = target.position.x < rigid.position.x;
    }

    // 몹이 죽을 때 호출할 함수
    public void Die()
    {
        // 이미 죽은 상태라면 중복 실행 방지
        if (!isLive) return;

        isLive = false;

        // 1. 죽는 애니메이션 트리거 실행
        anim.SetTrigger("Dead");

        // 2. 충돌체 비활성화 (플레이어가 시체에 막히거나 타격 판정이 남는 것을 방지)
        if (coll != null)
        {
            coll.enabled = false;
        }

        // 3. 물리 연산 정지 (시체가 밀리지 않도록 아예 물리 시뮬레이션을 끕니다)
        rigid.simulated = false;

        // 4. 일정 시간 뒤에 오브젝트 삭제 (애니메이션 길이에 맞춰 시간(예: 1.0f) 조절)
        Destroy(gameObject, 1.0f);
    }
}