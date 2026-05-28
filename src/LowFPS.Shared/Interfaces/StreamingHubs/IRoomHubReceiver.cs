using LowFPS.Shared.Interfaces.Services;
using LowFPS.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowFPS.Shared.Interfaces.StreamingHubs {
    /// <summary>
    /// サーバーからクライアントへの通知関連
    /// </summary>
    public interface IRoomHubReceiver {
        /*
         * 基本処理
         */

        /// <summary>
        /// ユーザーの入室通知
        /// </summary>
        public void OnJoinRoom(JoinedUser user);

        /// <summary>
        /// ユーザーの退室通知
        /// </summary>
        public void OnLeaveRoom(Guid connectionId, int joinOrder);

       /*
        * ユーザー
        */

        /// <summary>
        /// ユーザーのTransfrom通知
        /// </summary>
        public void OnUpdateUserTransform(Guid connectionId, PlayerTransform playerTransform, TimeSpan sendSpan);

        /*
         * オブジェクト
         */

        /// <summary>
        /// オブジェクト作成通知
        /// </summary>
        public void OnCreateObject(Guid objectId, Guid createrConnectionId, SimpleTransform createdTransform, int objecListId);

        /// <summary>
        /// オブジェクトのTransform通知
        /// </summary>
        public void OnUpdateObjectTransform(Guid objectId, SimpleTransform sTransform, TimeSpan sendSpan);

        /// <summary>
        /// オブジェクトの削除通知
        /// </summary>
        public void OnDestroyObject(Guid objectId);

        /// <summary>
        /// 所有者削除通知
        /// </summary>
        public void OnDeleateOwnership(Guid objectId);
    }
}
