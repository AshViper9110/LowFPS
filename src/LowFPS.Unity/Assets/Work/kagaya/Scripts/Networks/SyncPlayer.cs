using Cysharp.Threading.Tasks;
using LowFPS.Shared.Models.Entities;
using System;
using UnityEngine;

public class SyncPlayer : MonoBehaviour {
    // 送る間隔
    [SerializeField] private float SendSpan = 0.1f;
    private float sendTimer = 0f;

    // ConnectionId
    public Guid connectionId;

    [Header("Sync Target")]
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform gunTransform;

    private void Awake() {
        RoomModel.I.OnUpdatedUserTransfrom += OnUpdatedUserTransform;
    }

    private void Update() {
        UpdateUserTransformAsync();
    }

    private void OnDisable() {
        if (RoomModel.I != null) {
            RoomModel.I.OnUpdatedUserTransfrom -= OnUpdatedUserTransform;
        }
    }

    private void OnDestroy() {
        OnDisable();
    }

    /// <summary>
    /// Transform同期
    /// </summary>
    private void UpdateUserTransformAsync() {
        if (!IsOwner()) {
            return;
        }

        sendTimer += Time.deltaTime;

        if (sendTimer >= SendSpan) {
            sendTimer = 0;
            PlayerTransform playerTransform = new PlayerTransform
            (
                bodyTransform.ToSimpleTransform(),
                cameraTransform.ToSimpleTransform(),
                gunTransform.ToSimpleTransform()
            );
            RoomModel.I.UpdateUserTransformAsync(playerTransform).Forget();
        }
    }

    /// <summary>
    /// [サーバー通知]
    /// オブジェクトのTransform通知
    /// </summary>
    public void OnUpdatedUserTransform(Guid connectionId, PlayerTransform sTransform, float conSpan) {
        if (this.connectionId != connectionId || connectionId == null) {
            return;
        }

        this.bodyTransform.ApplyTransform(sTransform.body, SendSpan);
        this.cameraTransform.ApplyTransform(sTransform.camera, SendSpan);
        this.gunTransform.ApplyTransform(sTransform.gun, SendSpan);
    }

    /// <summary>
    /// 自分自身だったら
    /// </summary>
    public bool IsOwner() {
        return RoomModel.I.ConnectionId == connectionId;
    }
}