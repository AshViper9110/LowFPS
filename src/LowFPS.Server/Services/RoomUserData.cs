using LowFPS.Shared.Interfaces.Services;
using LowFPS.Shared.Models.Entities;

namespace LowFPS.Server.Services {
    public class RoomUserData {
        /// <summary>
        /// 接続済みユーザー情報
        /// </summary>
        public JoinedUser joinedUser = new JoinedUser();

        /// <summary>
        /// ユーザーのTransform情報
        /// </summary>
        public SimpleTransform transform = new SimpleTransform();
    }
}
