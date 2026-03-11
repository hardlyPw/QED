using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public int maxHealth = 3;       // 몬스터의 최대 체력
    private int currentHealth;

    private Animator anim;
    private EnemyRandomWander aiScript; // 죽었을 때 움직임을 멈추기 위해 가져옴
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        aiScript = GetComponent<EnemyRandomWander>();
    }

    //누군가(참격)가 이 함수를 부르면 데미지를 입음!
    public void TakeDamage(int damage)
    {
        if (isDead) return; // 이미 죽었으면 무시

        currentHealth -= damage;
        Debug.Log("몬스터 피격! 남은 체력: " + currentHealth);

        // (선택) 여기에 맞았을 때 하얗게 번쩍이는 효과를 넣어도 됨!

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // 1. 움직임 멈추기 (AI 스크립트 끄기 + 물리력 0으로 만들기)
        if (aiScript != null) aiScript.enabled = false;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // 2. 아까 만든 '페이드아웃(사망)' 애니메이션 재생!
        if (anim != null) anim.Play("Num1_Death");

        // 3. 애니메이션이 끝날 즈음(0.5초 뒤) 시체 삭제
        Destroy(gameObject, 0.5f);
    }
}