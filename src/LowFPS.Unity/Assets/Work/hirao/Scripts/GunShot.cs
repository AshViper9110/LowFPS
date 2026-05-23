using UnityEngine;

public class GunShot : MonoBehaviour
{
    [Header("References")]
    public Transform muzzlePoint;

    [Header("Shoot")]
    public float damage = 25f;
    public float range = 200f;

    [Header("Effects")]
    public GameObject bulletHolePrefab;

    // 弾痕が消えるまでの時間
    public float bulletHoleLifeTime = 10f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Ray ray =
            new Ray(
                muzzlePoint.position,
                muzzlePoint.forward
            );

#if UNITY_EDITOR
        Debug.DrawRay(
            ray.origin,
            ray.direction * range,
            Color.red,
            1f
        );
#endif

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Hit : " + hit.collider.name);

            //
            // Bullet Hole
            //

            if (bulletHolePrefab != null)
            {
                Quaternion rot =
                    Quaternion.LookRotation(hit.normal);

                GameObject hole =
                    Instantiate(
                        bulletHolePrefab,
                        hit.point + hit.normal * 0.001f,
                        rot
                    );

                hole.transform.SetParent(hit.collider.transform);

                Destroy(
                    hole,
                    bulletHoleLifeTime
                );
            }

            //
            // Damage
            //

            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Player Damage : " + damage);
            }
        }
    }
}