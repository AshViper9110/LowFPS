using UnityEngine;

public class PlayerStatus : MonoBehaviour {
    public float HP {  get; private set; }
    [SerializeField] private float maxHp = 0;

    [SerializeField] private float regeneAmount = 0;
    [SerializeField] private float regeneStartTime = 0;
    private float regeneStartTimer = 0;
    [SerializeField] private float regeneCoolTime = 0;
    private float regeneTimer = 0;

    private void Awake() {
        HP = maxHp;
    }

    private void Update() {
        Regeneration();
    }

    /// <summary>
    /// 自然回復
    /// </summary>
    private void Regeneration() {
        // 自然回復が始まるまでの時間
        regeneStartTimer += Time.deltaTime;
        if (regeneStartTimer < regeneStartTime) {
            return;
        }

        // 自然回復
        regeneTimer += Time.deltaTime;
        if (regeneTimer >= regeneCoolTime) {
            regeneTimer = 0;

            HP += regeneAmount;
            if (HP > maxHp) {
                regeneStartTimer = 0;
                HP = maxHp;
            }
        }
    }

    /// <summary>
    /// 弾が当たった
    /// </summary>
    public void HitBullet(float damage) {
        regeneStartTimer = 0;

        HP -= damage;

        if (HP <= 0) {

        }
    }
}
