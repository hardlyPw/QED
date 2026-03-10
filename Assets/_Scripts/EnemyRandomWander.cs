using System.Collections;
using UnityEngine;

public class EnemyRandomWander : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 2f;         // 이동 속도
    public float thinkTimeMin = 1f;      // 최소 생각(이동) 시간
    public float thinkTimeMax = 3f;      // 최대 생각(이동) 시간

    private Vector2 moveDirection;
    private Rigidbody2D rb;

    void Start()
    {
        // 몬스터의 물리 엔진(Rigidbody2D)을 가져옴
        rb = GetComponent<Rigidbody2D>();

        // 태어나자마자 "랜덤 이동 루틴" 시작!
        StartCoroutine(WanderRoutine());
    }

    void FixedUpdate()
    {
        // 물리 엔진을 이용해 부드럽게 이동시킴
        rb.velocity = moveDirection * moveSpeed;

        // 이동하는 방향에 맞춰 좌우 반전 (Flip)
        if (moveDirection.x != 0)
        {
            Vector3 scale = transform.localScale;
            // 오른쪽으로 가면 양수(1), 왼쪽으로 가면 음수(-1)로 뒤집기
            scale.x = moveDirection.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    // 핵심 AI: 일정 시간마다 생각(방향 전환)을 반복하는 코루틴
    IEnumerator WanderRoutine()
    {
        while (true) // 죽기 전까지 무한 반복
        {
            // 1. 랜덤한 방향 고르기 (원 안의 무작위 좌표를 뽑아줌!)
            moveDirection = Random.insideUnitCircle.normalized;

            // 10% 확률로 가만히 멍 때리기 (좀 더 생동감 있는 AI를 위해)
            if (Random.value < 0.1f)
            {
                moveDirection = Vector2.zero;
            }

            // 2. 다음 생각을 할 때까지 랜덤한 시간 동안 대기 (예: 1~3초)
            float waitTime = Random.Range(thinkTimeMin, thinkTimeMax);
            yield return new WaitForSeconds(waitTime);
        }
    }
}