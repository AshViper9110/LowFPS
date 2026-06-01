using UnityEngine;

public class GunShotAsync : MonoBehaviour
{

    [Header("Effects")]
    [SerializeField] private GameObject bulletHolePrefab;

    [SerializeField] private float bulletHoleLifeTime = 10f;

    private void OnEnable()
    {
        if (RoomModel.I != null)
        {
            RoomModel.I.OnGunshot += OnGunShot;
        }
    }

    private void OnDisable()
    {
        if (RoomModel.I != null)
        {
            RoomModel.I.OnGunshot -= OnGunShot;
        }
    }

    private void OnGunShot(System.Guid connectionId, Vector3 muzzlePos, Vector3 direction, float range, int damage)
    {
        Debug.Log($"GunShot {connectionId}");

        // Hit Scan
        Ray ray = new Ray(muzzlePos, direction);

#if UNITY_EDITOR
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 1f);
#endif

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            //Debug.Log($"Hit : {hit.collider.name}");

            CreateBulletHole(hit);

            if (hit.collider.tag == "Player") ApplyDamage(hit, damage);
        }
    }

    private void CreateBulletHole(RaycastHit hit)
    {
        if (bulletHolePrefab == null)
        {
            return;
        }

        Quaternion rot = Quaternion.LookRotation(hit.normal);

        GameObject hole = Instantiate(
            bulletHolePrefab,
            hit.point + hit.normal * 0.001f,
            rot
        );

        hole.transform.SetParent(hit.collider.transform);

        Destroy(hole, bulletHoleLifeTime);
    }

    private void ApplyDamage(RaycastHit hit, int damage)
    {
        Debug.Log($"Damage : {damage}");
    }
}
