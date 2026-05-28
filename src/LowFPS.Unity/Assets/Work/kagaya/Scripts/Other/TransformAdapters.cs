using DG.Tweening;
using LowFPS.Shared.Models.Entities;
using UnityEngine;

public static class TransformAdapters {
    /// <summary>
    /// Transform -> DTO
    /// </summary>
    public static SimpleTransform ToSimpleTransform(this Transform t) =>
        new SimpleTransform {
            localPosition = t.position,
            localRotation = t.rotation,
            localScale = t.localScale,
        };

    /// <summary>
    /// DTO ->Transform
    /// </summary>
    public static void ApplyTransform(this Transform t, in SimpleTransform st, float duration)
    {
        // 距離が遠い時だけKill 
        if (Vector3.Distance(t.position, st.localPosition) > 3f) t.DOKill();
        t.DOMove( st.localPosition,duration).SetEase(Ease.Linear).SetUpdate(true);
        t.DORotateQuaternion(st.localRotation,duration).SetEase(Ease.Linear).SetUpdate(true);
    }
}
