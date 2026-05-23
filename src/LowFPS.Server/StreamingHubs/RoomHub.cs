using LowFPS.Server.Services;
using LowFPS.Shared.Interfaces.Services;
using LowFPS.Shared.Interfaces.StreamingHubs;
using MagicOnion.Server.Hubs;

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

            Console.WriteLine($"送信時間：{elapsedTime}");
            Console.WriteLine($"受信時間：{receivedSpan}");

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
    }
}
