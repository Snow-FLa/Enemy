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

    // 💡 추가됨: 그랩 이펙트 관련 설정
    [Header("Effect Settings")]
    [Tooltip("생성할 이펙트 프리팹 (파티클 등)")]
    public GameObject grabEffectPrefab;
    [Tooltip("이펙트가 생성될 기준 위치 (빈 오브젝트)")]
    public Transform grabEffectPoint;
    [Tooltip("애니메이션 시작 후 팔을 뻗기까지 걸리는 시간 (초)")]
    public float effectDelay = 0.3f;

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
            if (canGrab)
            {
                if (distance <= grabDistance && yDifference <= grabYThreshold)
                {
                    StartCoroutine(GrabAttackRoutine());
                }
            }
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

            bool canDoGrabNow = (canGrab && distance <= grabDistance && yDifference <= grabYThreshold);
            bool canDoMeleeNow = (!canGrab && distance <= attack1Distance);

            if (!canDoGrabNow && !canDoMeleeNow)
            {
                Vector2 dirVec = (target.position - rigid.position).normalized;
                Vector2 nextVec = dirVec * moveSpeed * Time.fixedDeltaTime;
                rigid.MovePosition(rigid.position + nextVec);
            }
            else
            {
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
                if (distance <= grabDistance && yDifference <= grabYThreshold)
                    isMoving = false;
            }
            else
            {
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

    IEnumerator MeleeAttackRoutine()
    {
        isAttacking = true;
        canAttack = false;
        anim.SetBool("attack1", true);

        float timer = 0f;
        while (timer < attack1Duration)
        {
            float currentDist = Vector2.Distance(rigid.position, target.position);
            if (currentDist > attack1Distance) break;

            timer += Time.deltaTime;
            yield return null;
        }

        anim.SetBool("attack1", false);
        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // 💡 수정됨: 이펙트 생성 로직이 추가된 그랩 코루틴
    IEnumerator GrabAttackRoutine()
    {
        isAttacking = true;
        canAttack = false;

        anim.SetBool("grab", true);
        StartCoroutine(GrabCooldownRoutine());

        // 1. 애니메이션이 시작되고, 팔을 앞으로 뻗는 프레임까지 잠깐 대기합니다.
        yield return new WaitForSeconds(effectDelay);

        // 2. 이펙트 생성!
        if (grabEffectPrefab != null && grabEffectPoint != null)
        {
            GameObject effect = Instantiate(grabEffectPrefab, grabEffectPoint.position, Quaternion.identity);

            // 만약 몹이 왼쪽을 보고 있다면 이펙트도 좌우 반전 시켜줍니다 (스프라이트 기준)
            if (spriter.flipX)
            {
                Vector3 scale = effect.transform.localScale;
                scale.x *= -1;
                effect.transform.localScale = scale;
            }

            // 0.5초 뒤에 이펙트 자동 삭제 (이펙트 길이에 맞게 조절하세요)
            Destroy(effect, 0.5f);
        }

        // 3. 남은 그랩 시간 동안 대기 (전체 그랩 시간 - 이펙트 딜레이 시간)
        float remainingTime = grabDuration - effectDelay;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

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

        anim.SetBool("attack1", false);
        anim.SetBool("grab", false);
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