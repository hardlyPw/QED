using UnityEngine;

public class SlashDamage : MonoBehaviour
{
    public int damage = 1; // 참격의 데미지

    // 콜라이더끼리 겹쳤을 때 자동으로 실행되는 함수!
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 부딪힌 대상의 태그가 "Enemy"인지 확인
        if (collision.CompareTag("Enemy"))
        {
            // 2. 적의 몸에서 EnemyHealth 스크립트를 찾아냄
            EnemyHealth enemyHP = collision.GetComponent<EnemyHealth>();

            // 3. 스크립트가 있다면 데미지를 줌!
            if (enemyHP != null)
            {
                enemyHP.TakeDamage(damage);
            }
        }
    }
}