using LowFPS.Shared.Models.Entities;

namespace LowFPS.Server.Services {
    public class RoomObjectData {
        /// <summary>
        /// オブジェクトリストId
        /// </summary>
        public int objectListId;

        /// <summary>
        /// Transform
        /// </summary>
        public SimpleTransform simpleTransform = new SimpleTransform();

        /// <summary>
        /// 所有者のConnectionId
        /// </summary>
        public Guid ownerConnectionId = Guid.Empty;

        /// <summary>
        /// 所有者が存在するか
        /// </summary>
        public bool ownerExist = false;
    }
}
