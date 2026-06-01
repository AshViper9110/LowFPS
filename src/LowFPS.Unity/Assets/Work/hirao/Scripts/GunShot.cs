using UnityEngine;

public class GunShot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GunData gunData;
    private PlayerCon playerCon;

    [Header("Shell")]
    [SerializeField] private GameObject shellPrefab;

    [SerializeField] private float shellForce = 3f;

    [SerializeField] private float shellTorque = 5f;

    [SerializeField] private float shellLifeTime = 10f;

    [Header("Shoot")]
    [SerializeField] private float range = 200f;

    [Header("Effects")]
    [SerializeField] private GameObject bulletHolePrefab;

    [SerializeField] private float bulletHoleLifeTime = 10f;

    // 次に撃てる時間
    private float nextFireTime;

    private void Start()
    {
        GameObject playerObj = GameObject.Find("Player(Clone)");

        if (playerObj != null)
        {
            playerCon = playerObj.GetComponent<PlayerCon>();
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (gunData == null)
        {
            Debug.LogWarning("GunData is Null");
            return;
        }

        // RPM -> 秒間発射間隔
        float fireInterval = 60f / gunData.Rpm;

        if (Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + fireInterval;

        Shoot();
    }

    private async void Shoot()
    {
        if (playerCon != null)
        {
            playerCon.AddRecoil(gunData.Backlash);
        }
        // Shell Eject
        EjectShell();

        // Accuracy
        Vector3 direction = GetSpreadDirection();

        // Hit Scan
        Ray ray = new Ray(gunData.MuzzlePoint.position, direction);

#if UNITY_EDITOR
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 1f);
#endif

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log($"Hit : {hit.collider.name}");

            CreateBulletHole(hit);
        }

        await RoomModel.I.GunShotAsync(gunData.MuzzlePoint.position, direction, range, gunData.Damage);
    }

    private Vector3 GetSpreadDirection()
    {
        // accuracy が高いほどブレが小さい想定
        float spread = 1f - gunData.Accuracy;

        Vector3 direction = gunData.MuzzlePoint.forward;

        direction += gunData.MuzzlePoint.right * Random.Range(-spread, spread);

        direction += gunData.MuzzlePoint.up * Random.Range(-spread, spread);

        return direction.normalized;
    }

    private void EjectShell()
    {
        if (shellPrefab == null || gunData.ShellEjectPoint == null)
        {
            return;
        }

        GameObject shell = Instantiate(
            shellPrefab,
            gunData.ShellEjectPoint.position,
            gunData.ShellEjectPoint.rotation
        );

        Rigidbody shellRb = shell.GetComponent<Rigidbody>();

        if (shellRb != null)
        {
            shellRb.AddForce(
                gunData.ShellEjectPoint.right * shellForce,
                ForceMode.Impulse
            );

            shellRb.AddTorque(
                Random.insideUnitSphere * shellTorque,
                ForceMode.Impulse
            );
        }

        Destroy(shell, shellLifeTime);
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
}