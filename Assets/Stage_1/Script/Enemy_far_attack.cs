using UnityEngine;

public class Enemy_far_attack : MonoBehaviour
{

    [Tooltip("투사체 날아가는 속도")]
    public float speed = 7f;
    [Tooltip("투사체 데미지")]
    public int damage = 10;
    [Tooltip("화면에 유지되는 최대 시간 (초)")]
    public float lifeTime = 3f;

    private Vector2 moveDirection; // 날아갈 방향

    void Start()
    {
        // 3초 뒤에 투사체 삭제 (메모리 낭비 방지)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 매 프레임마다 설정된 방향으로 이동 (Space.World를 넣어야 회전해도 똑바로 날아갑니다!)
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    // 적(Enemy_far)이 투사체를 생성하면서 이 함수를 호출해 방향을 알려줌
    public void SetDirection(Vector2 dir)
    {
        moveDirection = dir;

        // 투사체가 날아가는 방향을 바라보게 회전시킴
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어와 부딪히면
        if (collision.CompareTag("Player"))
        {
            Debug.Log("플레이어 피격!");
            // (여기에 플레이어 체력 깎는 코드 추가)

            // 투사체는 파괴됨
            Destroy(gameObject);
        }
    }

}
