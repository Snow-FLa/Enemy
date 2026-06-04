using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_close : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 3f;
    public Rigidbody2D target;

    // 💡 방금 추가한 핵심 변수! (인스펙터에 나타납니다)
    [Tooltip("플레이어의 몸통(중심)이 아닌 발밑을 추적하도록 좌표를 조정합니다.")]
    public Vector2 targetOffset = new Vector2(0f, -0.5f);

    [Header("공격 설정")]
    public int damage = 10;

    bool isLive = true;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (!isLive || target == null) return; // 타겟이 없을 때의 에러 방지 추가

        // 💡 핵심 변경: target.position에 오프셋 값을 더해 '진짜 목표 위치'를 만듭니다.
        Vector2 actualTargetPos = (Vector2)target.position + targetOffset;

        // 💡 몹의 현재 위치에서 '진짜 목표 위치'로 가는 방향을 계산합니다.
        Vector2 dirVec = actualTargetPos - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;

        rigid.MovePosition(rigid.position + nextVec);
        rigid.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive || target == null) return;

        spriter.flipX = target.position.x < rigid.position.x;
        anim.SetFloat("speed", speed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLive && collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("플레이어와 충돌! 데미지: " + damage);
            // TODO: 플레이어 데미지 처리 스크립트 연결
            // collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }

    public void Die()
    {
        if (!isLive) return;

        isLive = false;

        rigid.linearVelocity = Vector2.zero;

        // Collider2D 비활성화 시 안전 장치 추가
        if (GetComponent<Collider2D>())
            GetComponent<Collider2D>().enabled = false;

        anim.SetTrigger("dead");

        Destroy(gameObject, 1.5f);
    }
}