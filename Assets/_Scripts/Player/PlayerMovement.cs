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
    public Transform centerPoint;  // ⬅️ 캐릭터의 '중심'을 잡아줄 오브젝트
    public float attackRadius = 1.5f; // ⬅️ 원의 반지름 (인스펙터에서 조절 가능!)

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
        FaceMouseDirection(); //  공격을 시작하자마자 마우스 방향으로 몸을 휙 돌린다!
        PlaySpumAnimSafely(PlayerState.ATTACK, 0); // 공격 리스트의 0번(0_Attack_Normal)을 실행해라! -> 캐릭터 휘두르는 모션 재생
        
        if (slashPrefab != null && centerPoint != null)
        {
            // 1. 마우스 위치를 유니티의 '월드 좌표'로 변환
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f; // 2D 게임이므로 Z축은 0으로 고정
            
            // 2. 중심점 위치
            Vector3 centerPos = centerPoint.position;

            // 3. 방향 벡터 계산: (마우스 위치 - 중심점 위치) 후 정규화(길이를 1로 만듦)
            Vector3 direction = (mousePos - centerPos).normalized;

            // 4. 소환위치 계산: (P_spawn = P_center + V_dir * R)
            Vector3 spawnPos = centerPos + direction * attackRadius;

            // 5. 회전 각도 계산 (마우스가 있는 곳을 쳐다보게 만들기)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;    

            GameObject slash  = Instantiate(slashPrefab, spawnPos, Quaternion.Euler(0, 0, angle));   // (무엇을, 어느 위치에, 어떤 각도로) 소환할지 결정
            //Quaternion.Euler(0, 0, angle): 방금 구한 마우스를 향하는 회전 각도.
            
            //각도를 180도 돌려버리면, 방향은 맞으나 위아래가 뒤집혀 버리는 현상 발생. 따라서 y축 뒤집어주는거임
            if (direction.x < 0) 
            {
                Vector3 sScale = slash.transform.localScale;
                sScale.y = -Mathf.Abs(sScale.y); // Y축을 뒤집어줌
                slash.transform.localScale = sScale;
            }

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

    // 마우스가 있는 방향으로 캐릭터를 쳐다보게 하는 범용 함수
    private void FaceMouseDirection()
    {
        // 1. 마우스의 월드 좌표 가져오기. Screen 좌표계의 마우스 위치를 월드좌표계로 변환해서 mousePos에 저장
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2. 마우스 X좌표 - 캐릭터 X좌표 = 방향 (음수면 왼쪽, 양수면 오른쪽)
        float directionX = mousePos.x - transform.position.x;

        // 3. 우리가 예전에 만들어둔 뒤집기 함수를 그대로 재활용!
        HandleFlip(directionX);
    }

    private void HandleFlip(float x)
    {
        Vector3 currentScale = transform.localScale;
        if (x < 0) currentScale.x = Mathf.Abs(currentScale.x);
        else if (x > 0) currentScale.x = -Mathf.Abs(currentScale.x);
        transform.localScale = currentScale;
    }
}