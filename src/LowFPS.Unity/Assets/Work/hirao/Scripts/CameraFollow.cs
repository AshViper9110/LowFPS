using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [SerializeField]
    private Vector3 offset = new Vector3(0, 5, -7);

    public void SetFollowTarget(Transform target)
    {
        this.target = target;
        offset = new Vector3(0, 5, -7);
    }

    public void SetSpectateTarget(Transform target)
    {
        this.target = target;
        offset = new Vector3(0, 15, 0);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        transform.position = target.position + offset;

        if (offset.x == 0 && offset.z == 0)
        {
            // ê^è„Ç©ÇÁå©â∫ÇÎÇµ
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            transform.LookAt(target);
        }
    }
}