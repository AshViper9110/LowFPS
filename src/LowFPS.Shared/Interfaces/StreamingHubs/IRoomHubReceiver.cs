using LowFPS.Shared.Interfaces.Services;
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
        /// <summary>
        /// ユーザーの入室通知
        /// </summary>
        public void OnJoinRoom(JoinedUser user);

        /// <summary>
        /// ユーザーの退室通知
        /// </summary>
        public void OnLeaveRoom(Guid connectionId, int joinOrder);
    }
}
