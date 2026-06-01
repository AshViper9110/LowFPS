using LowFPS.Server.Services;
using LowFPS.Shared.Interfaces.Services;
using LowFPS.Shared.Interfaces.StreamingHubs;
using LowFPS.Shared.Models.Entities;
using MagicOnion.Server.Hubs;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace LowFPS.Server.StreamingHubs {
    public class RoomHub : StreamingHubBase<IRoomHub, IRoomHubReceiver>, IRoomHub {
        private readonly RoomContextRepository _roomContextRepository;

        private RoomContext? _roomContext;

        public RoomHub(RoomContextRepository roomContextRepository) {
            _roomContextRepository = roomContextRepository;
        }

        /*
         * 
         * 基本処理
         * 
         */

        /// <summary>
        /// 切断時の処理
        /// </summary>
        protected override ValueTask OnDisconnected() {
            // ルームから退出
            LeaveRoomAsync();

            return CompletedTask;
        }

        /// <summary>
        /// 接続ID取得
        /// </summary>
        public Task<Guid> GetConnectionId() {
            return Task.FromResult<Guid>(this.ConnectionId);
        }

        /// <summary>
        /// 通信速度測定
        /// </summary>
        public Task<DateTime> SpeedTestAsync(DateTime sendTime, TimeSpan receivedSpan) {
            DateTime receivedTime = DateTime.UtcNow;
            TimeSpan elapsedTime = receivedTime - sendTime;

            _roomContext.RoomUserDataList[this.ConnectionId].sendSpan = elapsedTime;
            _roomContext.RoomUserDataList[this.ConnectionId].receivedSpan = receivedSpan;

            return Task.FromResult<DateTime>(receivedTime);
        }

        /*
         * 
         * ゲーム内処理
         * 
         */

        /// <summary>
        /// ルーム作成
        /// </summary>
        public Task CreateRoomAsync(string roomName) {
            // 同時に生成しない用に排他制御
            lock (_roomContextRepository) {
                // 指定の名前のルームがあるかどうかを確認
                this._roomContext = _roomContextRepository.GetContext(roomName);
                if (this._roomContext == null) {
                    // なかったら生成
                    this._roomContext = _roomContextRepository.CreateContext(roomName);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// ルーム削除
        /// </summary>
        public Task DeleteRoomAsync() {
            _roomContextRepository.RemoveContext(_roomContext.Name);

            return Task.CompletedTask;
        }

        /// <summary>
        /// ルームに接続
        /// </summary>
        public async Task<JoinedUser[]> JoinRoomAsync(string userName, string roomName) {
            await CreateRoomAsync(roomName);

            // すでにいるか
            if (this._roomContext.RoomUserDataList.ContainsKey(this.ConnectionId)) {
                throw new Exception("すでに入室済みです。");
            }

            // ルームに参加 ＆ ルームを保持
            this._roomContext.Group.Add(this.ConnectionId, Client);

            // 入室済みユーザーのデータを作成
            var joinedUser = new JoinedUser();
            joinedUser.ConnectionId = this.ConnectionId;
            joinedUser.Name = userName;
            joinedUser.JoinOrder = this._roomContext.RoomUserDataList.Count + 1;

            // ルームコンテキストにユーザー情報を登録
            var roomUserData = new RoomUserData() { joinedUser = joinedUser };
            this._roomContext.RoomUserDataList[this.ConnectionId] = roomUserData;

            // コンソールにログを表示
            _roomContext.WriteConsoleJoinInfo(joinedUser);

            // ルーム参加者全員に、ユーザーの入室通知を送信
            this._roomContext.Group.All.OnJoinRoom(joinedUser);

            // 入室リクエストをしたユーザーに、参加者の情報をリストで返す
            return this._roomContext.RoomUserDataList.Select(f => f.Value.joinedUser).ToArray();
        }

        /// <summary>
        /// 退出処理
        /// </summary>
        public Task LeaveRoomAsync() {
            // ルームにいなかったら無視
            if (!this._roomContext.RoomUserDataList.ContainsKey(this.ConnectionId)) {
                return Task.CompletedTask;
            }

            // コンソールにログを表示
            _roomContext.WriteConsoleLeaveInfo(this.ConnectionId);

            // 退出したことを全メンバーに通知
            int LeaveJoinOrder = _roomContext.RoomUserDataList[this.ConnectionId].joinedUser.JoinOrder;
            this._roomContext.Group.All.OnLeaveRoom(this.ConnectionId, LeaveJoinOrder);

            // ルーム内のメンバーから自分を削除
            this._roomContext.Group.Remove(this.ConnectionId);

            // 参加順番を繰り下げ
            foreach (RoomUserData roomUserData in _roomContext.RoomUserDataList.Values) {
                if (roomUserData.joinedUser.JoinOrder > LeaveJoinOrder) {
                    roomUserData.joinedUser.JoinOrder -= 1;
                }
            }

            // ルームデータから退出したユーザーを削除
            this._roomContext.RoomUserDataList.Remove(this.ConnectionId);

            // ルーム内にユーザーが一人もいなかったらルームを削除
            if (this._roomContext.RoomUserDataList.Count == 0) {
                DeleteRoomAsync();
            }

            return Task.CompletedTask;
        }

        /*
         * 
         * ユーザー
         * 
         */

        /// <summary>
        /// ユーザーのTransfrom同期
        /// </summary>
        public Task UpdateUserTransformAsync(PlayerTransform playerTransform) {
            // サーバーに保持
            _roomContext.RoomUserDataList[this.ConnectionId].transform = playerTransform;

            TimeSpan sendSpan = _roomContext.RoomUserDataList[this.ConnectionId].sendSpan;

            // 自分以外のユーザーに通知
            _roomContext.Group.Except([this.ConnectionId]).OnUpdateUserTransform(this.ConnectionId, playerTransform, sendSpan);

            return Task.CompletedTask;
        }

        /*
         * 
         * オブジェクト
         * 
         */

        /// <summary>
        /// オブジェクト作成
        /// </summary>
        public Task<Guid> CreateObjectAsync(SimpleTransform createdTransform, int objectListId) {
            // id作成
            Guid objId = Guid.NewGuid();

            // 情報作成
            RoomObjectData roomObjectData = new RoomObjectData() {
                objectListId = objectListId,
                simpleTransform = createdTransform,
                ownerConnectionId = this.ConnectionId,
                ownerExist = true,
            };

            // サーバーに保持
            this._roomContext.RoomObjectDataList[objId] = roomObjectData;

            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnCreateObject(objId, this.ConnectionId, createdTransform, objectListId);

            return Task.FromResult<Guid>(objId);
        }

        /// <summary>
        /// オブジェクトリストに追加
        /// </summary>
        public Task AddObjectListAsync(Guid objectId, int objectListId, SimpleTransform simpleTransform) {
            // 情報作成
            RoomObjectData roomObjectData = new RoomObjectData() {
                objectListId = objectListId,
                simpleTransform = simpleTransform,
                ownerConnectionId = this.ConnectionId,
            };

            // サーバーに保持
            this._roomContext.RoomObjectDataList[objectId] = roomObjectData;

            return Task.CompletedTask;
        }

        /// <summary>
        /// オブジェクトのTransform同期
        /// </summary>
        public Task UpdateObjectTransformAsync(Guid objectId, SimpleTransform sTransform) {
            // そのオブジェクトIdがあるか所有者のIdが一致しているか
            if (!this._roomContext.RoomObjectDataList.ContainsKey(objectId) ||
                this._roomContext.RoomObjectDataList[objectId].ownerConnectionId != this.ConnectionId) {
                return Task.CompletedTask;
            }

            // サーバーに保持
            this._roomContext.RoomObjectDataList[objectId].simpleTransform = sTransform;

            TimeSpan sendSpan = _roomContext.RoomUserDataList[this.ConnectionId].sendSpan;

            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnUpdateObjectTransform(objectId, sTransform, sendSpan);

            return Task.CompletedTask;
        }

        /// <summary>
        /// オブジェクトの削除
        /// </summary>
        public Task DestroyObjectAsync(Guid objectId) {
            // そのオブジェクトIdがあるか所有者のIdが一致しているか
            if (!this._roomContext.RoomObjectDataList.ContainsKey(objectId) ||
                this._roomContext.RoomObjectDataList[objectId].ownerConnectionId != this.ConnectionId) {
                return Task.CompletedTask;
            }

            // サーバーから削除
            this._roomContext.RoomObjectDataList.Remove(objectId);

            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnDestroyObject(objectId);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 所有権を取得する
        /// </summary>
        public Task<bool> GetOwnershipAsync(Guid objectId, bool forcibly = false) {
            // そのプレイヤーとオブジェとが存在するか
            if (!this._roomContext.RoomUserDataList.ContainsKey(this.ConnectionId) ||
                !this._roomContext.RoomObjectDataList.ContainsKey(objectId)) {
                return Task.FromResult<bool>(false);
            }

            // もし所有者だったら何もしない
            if (this._roomContext.RoomObjectDataList[objectId].ownerConnectionId == this.ConnectionId) {
                return Task.FromResult<bool>(true);
            }

            // 前の所有者
            Guid beforeOwner = this._roomContext.RoomObjectDataList[objectId].ownerConnectionId;

            // 同時に所有権を取得しないように排他制御
            lock (this._roomContext.RoomObjectDataList) {
                // 強制じゃなければ
                if (!forcibly) {
                    // 別のプレイヤーが所有者を有していたら無効
                    if (this._roomContext.RoomObjectDataList[objectId].ownerExist) {
                        return Task.FromResult<bool>(false);
                    }
                }

                this._roomContext.RoomObjectDataList[objectId].ownerExist = true;
                this._roomContext.RoomObjectDataList[objectId].ownerConnectionId = this.ConnectionId;

                // 前の所有者に所有権削除通知をおくる
                this._roomContext.Group.Only([beforeOwner]).OnDeleateOwnership(objectId);
            }

            return Task.FromResult<bool>(true);
        }

        /// <summary>
        /// 所有権を放棄する
        /// </summary>
        public Task OwnershipAbandonmentAsync(Guid objectId) {
            if (this._roomContext == null) {
                return Task.CompletedTask;
            }

            // そのプレイヤーとオブジェとが存在するか
            if (!this._roomContext.RoomUserDataList.ContainsKey(this.ConnectionId) ||
                !this._roomContext.RoomObjectDataList.ContainsKey(objectId)) {
                return Task.CompletedTask;
            }

            // もし所有者じゃなかったら何もしない
            if (this._roomContext.RoomObjectDataList[objectId].ownerConnectionId != this.ConnectionId) {
                return Task.CompletedTask;
            }

            // 解除
            this._roomContext.RoomObjectDataList[objectId].ownerExist = false;
            return Task.CompletedTask;
        }

        public Task GunShotAsync(Guid connectonId, Vector3 muzzlePos, Vector3 direction, float range, int damage)
        {
            // 自分以外に通知
            this._roomContext.Group.Except([this.ConnectionId]).OnGunShot(connectonId, muzzlePos, direction, range, damage);

            return Task.CompletedTask;
        }
    }
}
