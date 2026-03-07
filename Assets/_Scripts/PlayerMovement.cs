using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveDir;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // 시작할 때 내 몸통(물리) 찾기
    }

    void Update()
    {
        // 1. 입력 받기 (Update에서 처리해야 키보드 반응이 빠름)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDir = new Vector2(moveX, moveY).normalized; // 대각선 이동 속도 보정
    }

    void FixedUpdate()
    {
        // 2. 실제 물리 이동 (FixedUpdate에서 처리해야 벽 충돌이 자연스러움)
        rb.linearVelocity = moveDir * moveSpeed;
    }
}