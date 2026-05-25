using LowFPS.Shared.Interfaces.Services;
using LowFPS.Shared.Models.Entities;
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
        /*
         * 基本処理
         */

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

        /*
         * ユーザー
         */

        /// <summary>
        /// ユーザーのTransfrom同期
        /// </summary>
        Task UpdateUserTransformAsync(SimpleTransform playerTransform);

        /*
         * オブジェクト
         */

        /// <summary>
        /// オブジェクト生成
        /// </summary>
        Task<Guid> CreateObjectAsync(SimpleTransform createdTransform, int objectListId);

        /// <summary>
        /// オブジェクトリストに追加
        /// </summary>
        Task AddObjectListAsync(Guid objectId, int objectListId, SimpleTransform simpleTransform);

        /// <summary>
        /// オブジェクトのTransform同期
        /// </summary>
        Task UpdateObjectTransformAsync(Guid objectId, SimpleTransform sTransform);

        /// <summary>
        /// オブジェクトの削除
        /// </summary>
        Task DestroyObjectAsync(Guid objectId);

        /// <summary>
        /// 所有権を取得する
        /// </summary>
        Task<bool> GetOwnershipAsync(Guid objectId, bool forcibly = false);

        /// <summary>
        /// 所有権を放棄する
        /// </summary>
        Task OwnershipAbandonmentAsync(Guid objectId);
    }
}
