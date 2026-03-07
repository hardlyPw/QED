using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveDir;

    void Start()
    {
        // 게임이 시작될 때 요원의 물리 몸통(Rigidbody2D)을 찾아서 연결해줘!
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. 입력은 Update에서 빠릿빠릿하게 받기
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDir = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        // 2. 실제 물리적인 이동은 FixedUpdate에서 처리! (벽 뚫림, 덜덜거림 완벽 방지)
        rb.linearVelocity = moveDir * moveSpeed;
    }
}