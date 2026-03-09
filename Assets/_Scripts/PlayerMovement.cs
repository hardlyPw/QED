using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveDir;

    [Header("SPUM 애니메이션")]
    public SPUM_Prefabs spumPrefab;
    private int currentDirIndex = 0;

    [Header("공격 이펙트 설정")]
    public GameObject slashPrefab; //공격 이펙트로 뭘 소환활지
    public Transform attackPoint; // 어디에 소환될지

    // 🌟 [추가된 변수: 공격 중인지 확인하는 깃발] 🌟
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if(spumPrefab == null) spumPrefab = GetComponent<SPUM_Prefabs>();
        spumPrefab.OverrideControllerInit();
    }

    void Update()
    {
        // 🌟 [추가된 공격 로직] 🌟
        // 마우스 우클릭(1)을 눌렀고, 지금 공격 중이 아니라면?
        if (Input.GetMouseButtonDown(1) && !isAttacking)
        {
            PerformAttack();
        }

        // 공격 중이라면 아래의 이동 로직을 무시하고 그냥 서 있어라!
        if (isAttacking)
        {
            moveDir = Vector2.zero; // 미끄러짐 방지
            return; // 바로 반환해서 이동로직 작동안함.
        }
        // 🌟 ------------------------------------ 🌟

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDir = new Vector2(moveX, moveY).normalized;

        UpdateDirectionIndex(moveX, moveY);

        // 좌우 반전 로직 (덩치 유지)
        Vector3 currentScale = transform.localScale;
        if (moveX < 0)
        {
            currentScale.x = Mathf.Abs(currentScale.x);
            transform.localScale = currentScale;
        }
        else if (moveX > 0)
        {
            currentScale.x = -Mathf.Abs(currentScale.x);
            transform.localScale = currentScale;
        }

        if (moveDir.magnitude > 0)
        {
            PlaySpumAnimSafely(PlayerState.MOVE, currentDirIndex);
        }
        else
        {
            PlaySpumAnimSafely(PlayerState.IDLE, currentDirIndex);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveDir * moveSpeed;
    }

    private void UpdateDirectionIndex(float x, float y) //지금 어디 보는지 알려주는 나침반역할
    {
        if (x == 0 && y == 0) return;

        if (y < 0 && x == 0) currentDirIndex = 0;
        else if (y > 0 && x == 0) currentDirIndex = 1;
        else if (x < 0 && y == 0) currentDirIndex = 2;
        else if (x > 0 && y == 0) currentDirIndex = 3;
        else if (y < 0 && x < 0) currentDirIndex = 4;
        else if (y > 0 && x < 0) currentDirIndex = 5;
        else if (y < 0 && x > 0) currentDirIndex = 6;
        else if (y > 0 && x > 0) currentDirIndex = 7;
    }

    private void PlaySpumAnimSafely(PlayerState state, int desiredIndex)
    {
        int maxCount = 0;
        //maxCount로 move, IDLE가 애니메이션이 리스트에 총 몇개있는지 확인
        if (state == PlayerState.MOVE) maxCount = spumPrefab.MOVE_List.Count;
        else if (state == PlayerState.IDLE) maxCount = spumPrefab.IDLE_List.Count;
        // 공격일 때도 방어 코드 적용
        else if (state == PlayerState.ATTACK) maxCount = spumPrefab.ATTACK_List.Count;

        if (maxCount == 0) return; //만약 비었으면 실행X

        int safeIndex = desiredIndex % maxCount; //애니메이션이 하나라도 있으면 그걸 틀게하는 로직. "5번방향"을 요청했으나 현재 1번 오른쪽 move 밖에 없다면, 5%1=0 -> 0번 인덱스 오른쪽 걷기 실행됨.
        spumPrefab.PlayAnimation(state, safeIndex);
    }

    // 🌟 [공격 실행 함수] 🌟
    private void PerformAttack()
    {
        isAttacking = true; // 나 지금 공격 시작했다!
        PlaySpumAnimSafely(PlayerState.ATTACK, 0); // 공격 리스트의 0번(0_Attack_Normal)을 실행해라! -> 캐릭터 휘두르는 모션 재생

        if(slashPrefab != null && attackPoint != null)
        {
            GameObject slash  = Instantiate(slashPrefab, attackPoint.position, attackPoint.rotation);   // (무엇을, 어느 위치에, 어떤 각도로) 소환할지 결정

            Destroy(slash, 0.5f); //0.5초후에 삭제. 소환한 뒤엔 지워야함. 애니메이션 시간에 맞게 0.5초후 삭제됨.
        }

        // 0.5초 뒤에 ResetAttack 함수를 실행해서 다시 움직일 수 있게 만들어라!
        // (애니메이션 길이에 따라 0.5f 숫자를 조절해 주면 돼)
        Invoke("ResetAttack", 0.5f);
    }

    private void ResetAttack()
    {
        isAttacking = false; // 공격 끝! 다시 걷기 가능
    }
}