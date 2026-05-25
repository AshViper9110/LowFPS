using Cysharp.Threading.Tasks;
using LowFPS.Shared.Interfaces.Services;
using LowFPS.Shared.Interfaces.StreamingHubs;
using LowFPS.Shared.Models.Entities;
using MagicOnion;
using MagicOnion.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class RoomModel : Singleton<RoomModel>, IRoomHubReceiver {
    [SerializeField] private ServerConfigSO serverConfig;

    private GrpcChannelx channelx;
    private IRoomHub roomHub;

    /// <summary>
    /// 　接続ID
    /// </summary>
    public Guid ConnectionId { get; private set; }

    /// <summary>
    /// ユーザー名
    /// </summary>
    public string UserName { get; private set; }

    /// <summary>
    /// MagicOnionに接続しているか
    /// </summary>
    private bool isConnected = false;
    public bool IsConnected { get { return isConnected; } }

    /// <summary>
    /// ロームに入っているか
    /// </summary>
    private bool isJoinRoom = false;
    public bool IsJoinRoom { get { return isJoinRoom; } }

    // 送る間隔
    private float SendSpan = 0.1f;
    private float sendTimer = 0f;

    /// <summary>
    /// サーバーからの受信時間
    /// </summary>
    private TimeSpan receivedSpan = TimeSpan.Zero;

    /*
    * サーバー通知
    */

    /// <summary>
    /// ユーザー接続通知
    /// </summary>
    public Action<JoinedUser> OnJoinedUser { get; set; }
    /// <summary>
    /// ユーザー退出通知
    /// </summary>
    public Action<Guid, int> OnLeavedUser { get; set; }

    /// <summary>
    /// ユーザーのTransfrom通知
    /// </summary>
    public Action<Guid, SimpleTransform, float> OnUpdatedUserTransfrom { get; set; }

    /// <summary>
    /// オブジェクト作成通知
    /// </summary>
    public Action<Guid, Guid, SimpleTransform, int> OnCreatedObject { get; set; }

    /// <summary>
    /// オブジェクトのTransform通知
    /// </summary>
    public Action<Guid, SimpleTransform, float> OnUpdatedObjectTransform { get; set; }

    /// <summary>
    /// オブジェクトの削除通知
    /// </summary>
    public Action<Guid> OnDestroyedObject { get; set; }

    /// <summary>
    /// オブジェクトの所有権削除通知
    /// </summary>
    public Action<Guid> OnDeleatedOwnership { get; set; }

    /*
     * 処理
     */

    private async void Update() {
        sendTimer += Time.deltaTime;

        if (sendTimer >= SendSpan) {
            sendTimer = 0;
            await SpeedTestAsync();
        }
    }

    /// <summary>
    /// 　MagicOnion接続処理
    /// </summary>
    public async UniTask ConnectAsync() {
        channelx = GrpcChannelx.ForAddress(
#if DEBUG
            serverConfig.DEBUG.url
#else
            serverConfig.PRODUCTION.url
#endif
            );
        roomHub = await StreamingHubClient.
             ConnectAsync<IRoomHub, IRoomHubReceiver>(channelx, this);
        this.ConnectionId = await roomHub.GetConnectionId();
        isConnected = true;
    }

    /// <summary>
    /// MagicOnion切断処理
    /// </summary>
    public async UniTask DisconnectAsync() {
        isConnected = false;
        if (roomHub != null) await roomHub.DisposeAsync();
        if (channelx != null) await channelx.ShutdownAsync();
        roomHub = null;
        channelx = null;
    }
    /// <summary>
    /// 破棄処理
    /// </summary>
    protected override void OnDestroy() {
        base.OnDestroy();
        DisconnectAsync().Forget();
    }

    /// <summary>
    /// ゲーム終了時
    /// </summary>
    protected override void OnApplicationQuit() {
        base.OnApplicationQuit();
        DisconnectAsync().Forget();
    }

    /*
     * 
     * 基本処理
     * 
     */

    /// <summary>
    /// 通信速度測定
    /// </summary>
    public async UniTask SpeedTestAsync() {
        if (roomHub == null) {
            // throw new Exception("RoomHubがnullです。");
            return;
        }
        else if (!IsConnected ||
            !IsJoinRoom) {
            return;
        }

        DateTime sendTime = DateTime.UtcNow;
        DateTime receivedTime = await roomHub.SpeedTestAsync(sendTime, receivedSpan);
        receivedSpan = receivedTime - DateTime.UtcNow;
    }

    /// <summary>
    /// ルームに入室
    /// </summary>
    public async UniTask JoinRoomAsync(string userName, string roomName) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        try {
            JoinedUser[] joinedUsers = await roomHub.JoinRoomAsync(userName, roomName);
            isJoinRoom = true;
            UserName = userName;
            if (joinedUsers != null) {
                foreach (var user in joinedUsers) {
                    // 自分自身はスキップ
                    if (user.ConnectionId != ConnectionId) {
                        OnJoinedUser(user);
                    }
                }
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
        }

    }


    /// <summary>
    /// [サーバー通知]
    /// ロビーの入室通知
    /// </summary>
    public void OnJoinRoom(JoinedUser user) {
        if (OnJoinedUser != null) {
            OnJoinedUser(user);
        }
    }

    /// <summary>
    /// ルームから退室
    /// </summary>
    public async UniTask LeaveRoomAsync() {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        isJoinRoom = false;

        await roomHub.LeaveRoomAsync();
    }

    /// <summary>
    /// [サーバー通知]
    /// ロビーの退室通知
    /// </summary>
    public void OnLeaveRoom(Guid connectionId, int joinOrder) {
        if (OnLeavedUser != null) {
            OnLeavedUser(connectionId, joinOrder);
        }
    }

    /*
     * 
     * ユーザー
     * 
     */

    /// <summary>
    /// ユーザーのTransform同期
    /// </summary>
    public async UniTask UpdateUserTransformAsync(SimpleTransform playerTransform) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }
        else if (!IsConnected ||
            !IsJoinRoom) {
            return;
        }
        await roomHub.UpdateUserTransformAsync(playerTransform);
    }

    /// <summary>
    /// [サーバー通知]
    /// ユーザーのTransfrom通知
    /// </summary>
    public void OnUpdateUserTransform(Guid connectionId, SimpleTransform playerTransform, TimeSpan sendSpan) {
        if (OnUpdatedUserTransfrom != null) {
            float conSpan = (float)sendSpan.TotalSeconds + (float)receivedSpan.TotalSeconds;
            Debug.Log($"send：{sendSpan}, rece：{receivedSpan}");
            OnUpdatedUserTransfrom(connectionId, playerTransform, conSpan);
        }
    }

    /*
     * 
     * オブジェクト
     * 
     */

    /// <summary>
    /// オブジェクト生成
    /// </summary>
    public async UniTask<Guid> CreateObjectAsync(SimpleTransform createdTransform, int objectListId) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        return await roomHub.CreateObjectAsync(createdTransform, objectListId);
    }

    /// <summary>
    /// [サーバー通知]
    /// オブジェクト作成通知
    /// </summary>
    public void OnCreateObject(Guid objectId, Guid createrConnectionId, SimpleTransform createdTransform, int objectListId) {
        if (OnCreatedObject != null) {
            OnCreatedObject(objectId, createrConnectionId, createdTransform, objectListId);
        }
    }

    /// <summary>
    /// オブジェクトリストに追加
    /// </summary>
    public async UniTask AddObjectListAsync(Guid objectId, int objectListId, SimpleTransform simpleTransform) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        await roomHub.AddObjectListAsync(objectId, objectListId, simpleTransform);
    }

    /// <summary>
    /// オブジェクトのTransform同期
    /// </summary>
    public async UniTask UpdateObjectTransformAsync(Guid objectId, SimpleTransform sTransform) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        await roomHub.UpdateObjectTransformAsync(objectId, sTransform);
    }

    /// <summary>
    /// [サーバー通知]
    /// オブジェクトのTransform通知
    /// </summary>
    public void OnUpdateObjectTransform(Guid objectId, SimpleTransform sTransform, TimeSpan sendSpan) {
        if (OnUpdatedObjectTransform != null) {
            float conSpan = (float)sendSpan.TotalSeconds + (float)receivedSpan.TotalSeconds;
            OnUpdatedObjectTransform(objectId, sTransform, conSpan);
        }
    }

    /// <summary>
    /// オブジェクトの削除
    /// </summary>
    public async UniTask DestroyObjectAsync(Guid objectId) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        await roomHub.DestroyObjectAsync(objectId);
    }

    /// <summary>
    /// [サーバー通知]
    /// オブジェクトの削除通知
    /// </summary>
    public void OnDestroyObject(Guid objectId) {
        if (OnDestroyedObject != null) {
            OnDestroyedObject(objectId);
        }
    }

    /// <summary>
    /// 所有権を取得する
    /// </summary>
    public async UniTask<bool> GetOwnershipAsync(Guid objectId, bool forcibly = false) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        return await roomHub.GetOwnershipAsync(objectId, forcibly);
    }

    /// <summary>
    /// 所有権を放棄する
    /// </summary>
    public async UniTask OwnershipAbandonmentAsync(Guid objectId) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        await roomHub.OwnershipAbandonmentAsync(objectId);
    }

    /// <summary>
    /// [サーバー通知]
    /// 所有者削除通知
    /// </summary>
    public void OnDeleateOwnership(Guid objectId) {
        if (OnDeleatedOwnership != null) {
            OnDeleatedOwnership(objectId);
        }
    }
}
