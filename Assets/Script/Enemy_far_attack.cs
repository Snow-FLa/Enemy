using UnityEngine;

public class Enemy_far_attack : MonoBehaviour
{
    [Header("투사체 설정")]
    [Tooltip("투사체 날아가는 속도")]
    public float speed = 7f;
    [Tooltip("투사체 데미지")]
    public int damage = 10;

    [Tooltip("이 거리 이상 날아가면 스스로 파괴됨 (시간 대신 사용)")]
    public float maxDistance = 15f;

    private Vector2 moveDirection; // 날아갈 방향
    private Vector2 startPosition; // 💡 처음 스폰된 위치를 기억할 변수

    void Start()
    {
        // 💡 투사체가 생성된 순간의 위치를 저장해둡니다.
        startPosition = transform.position;
    }

    void Update()
    {
        // 1. 매 프레임 지정된 방향으로 날아갑니다.
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // 2. 💡 처음 위치와 현재 위치의 '거리'를 계산합니다.
        float currentDistance = Vector2.Distance(startPosition, transform.position);

        // 3. 거리가 최대 사거리를 넘어가면 파괴합니다.
        if (currentDistance > maxDistance)
        {
            Destroy(gameObject);
        }
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
            // TODO: 플레이어 체력을 깎는 코드 추가 
            // 예: collision.GetComponent<PlayerHealth>().TakeDamage(damage);

            // 💡 플레이어에게 맞으면 투사체 파괴
            Destroy(gameObject);
        }
    }
}