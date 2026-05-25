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

    // 低遅延用
    [SerializeField] private bool lowLatency = true;
    private Vector3 beforePos;
    private float moveSpeed;
    private Vector3 direction;

    private void Awake() {
        beforePos = this.transform.position;
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
            moveSpeed = Vector3.Distance(this.transform.position, beforePos);
            direction = (this.transform.position - beforePos).normalized;
            beforePos = this.transform.position;
            RoomModel.I.UpdateUserTransformAsync(this.transform.ToSimpleTransform()).Forget();
        }
    }

    /// <summary>
    /// [サーバー通知]
    /// オブジェクトのTransform通知
    /// </summary>
    public void OnUpdatedUserTransform(Guid connectionId, SimpleTransform sTransform, float conSpan) {
        if (this.connectionId != connectionId) {
            return;
        }

        if (lowLatency) {
            float distance = (conSpan + SendSpan) * moveSpeed;
            sTransform.localPosition = sTransform.localPosition + (direction * distance);
        }

        this.transform.ApplyTransform(sTransform, SendSpan);
    }

    /// <summary>
    /// 自分自身だったら
    /// </summary>
    public bool IsOwner() {
        return RoomModel.I.ConnectionId == connectionId;
    }
}