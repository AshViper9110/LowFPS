using LowFPS.Shared.Interfaces.Services;
using MagicOnion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowFPS.Shared.Interfaces.StreamingHubs {
    /// <summary>
    /// クライアントから呼び出す処理を実装するクラス用インターフェース
    /// </summary>
    public interface IRoomHub : IStreamingHub<IRoomHub, IRoomHubReceiver> {
        /// <summary>
        /// 接続ID取得
        /// </summary>
        Task<Guid> GetConnectionId();

        /// <summary>
        /// 通信速度測定
        /// </summary>
        Task<DateTime> SpeedTestAsync(DateTime sendTime, TimeSpan receivedSpan);

        /// <summary>
        /// ルームに接続
        /// </summary>
        Task<JoinedUser[]> JoinRoomAsync(string userName, string roomName);

        /// <summary>
        /// 退出処理
        /// </summary>
        Task LeaveRoomAsync();
    }
}
