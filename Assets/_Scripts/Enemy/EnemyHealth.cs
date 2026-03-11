using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public int maxHealth = 3;       // 몬스터의 최대 체력
    private int currentHealth;

    [Header("피격 효과")]
    public Material flashMaterial;      // 아까 만든 하얀색 재질을 넣을 곳!
    private Material originalMaterial;  // 몬스터의 원래 재질을 기억해둘 변수
    private SpriteRenderer[] spriteRenderers; // 몸통과 그림자 렌더러를 모두 가져옴

    private Animator anim;
    private EnemyRandomWander aiScript; // 죽었을 때 움직임을 멈추기 위해 가져옴
    private bool isDead = false;

    //추가: 현재 실행 중인 코루틴(타이머)을 기억할 변수
    private Coroutine flashCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        aiScript = GetComponent<EnemyRandomWander>();

        // 1. 자식 오브젝트(몸통, 그림자)에 있는 모든 SpriteRenderer를 찾아옴
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        // 2. 태어났을 때의 원래 재질(Sprites-Default)을 기억해둠
        if (spriteRenderers.Length > 0 ) originalMaterial = spriteRenderers[0].material;
    }

    //누군가(참격)가 이 함수를 부르면 데미지를 입음!
    public void TakeDamage(int damage)
    {
        if (isDead) return; // 이미 죽었으면 무시

        currentHealth -= damage;
        Debug.Log("몬스터 피격! 남은 체력: " + currentHealth);


        // 수정: 기존에 돌고 있던 번쩍임 타이머가 있다면 강제 종료!
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        // 새로운 flash코루틴 타이머를 시작하고, 그 타이머를 변수에 기억해둠!
        flashCoroutine = StartCoroutine(HitFlashRoutine()); 

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitFlashRoutine()
    {
        // 1. 모든 부위(몸통, 그림자)를 하얀색 재질로 덮어씌우기
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.material = flashMaterial;
        }

        // 2. 0.1초 동안 대기 (번쩍!)
        yield return new WaitForSeconds(0.1f);

        // 3. 아직 살아있다면 원래 재질로 원상복구
        if (!isDead)
        {
            foreach (SpriteRenderer sr in spriteRenderers)
            {
                sr.material = originalMaterial;
            }
        }


    }

    private void Die()
    {
        isDead = true;

        //움직임 멈추기 (AI 스크립트 끄기 + 물리력 0으로 만들기)
        if (aiScript != null) aiScript.enabled = false;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;


        // 중요: 죽을 때는 반드시 원래 재질로 돌려놔야 페이드아웃(투명화)이 예쁘게 적용됨!
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.material = originalMaterial;
        }

        //'페이드아웃(사망)' 애니메이션 재생!
        if (anim != null) anim.Play("Num1_Death");

        // 애니메이션이 끝날 즈음(0.5초 뒤) 시체 삭제
        Destroy(gameObject, 0.5f);
    }
}