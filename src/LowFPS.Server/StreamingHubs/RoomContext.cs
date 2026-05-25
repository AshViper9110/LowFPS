using Cysharp.Runtime.Multicast;
using LowFPS.Server.Services;
using LowFPS.Shared.Interfaces.Services;
using LowFPS.Shared.Interfaces.StreamingHubs;

namespace LowFPS.Server.StreamingHubs {
    public class RoomContext : IDisposable {
        /// <summary>
        /// ルームid
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// ルーム名
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// グループ
        /// </summary>
        public IMulticastSyncGroup<Guid, IRoomHubReceiver> Group { get; }

        /// <summary>
        /// ユーザーデータリスト
        /// </summary>
        public Dictionary<Guid, RoomUserData> RoomUserDataList { get; } =
            new Dictionary<Guid, RoomUserData>();

        /// <summary>
        /// オブジェクトデータリスト
        /// </summary>
        public Dictionary<Guid, RoomObjectData> RoomObjectDataList { get; } =
            new Dictionary<Guid, RoomObjectData>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RoomContext(IMulticastGroupProvider groupProvider, string roomName) {
            Id = Guid.NewGuid();
            Name = roomName;
            Group = groupProvider.GetOrAddSynchronousGroup<Guid, IRoomHubReceiver>(roomName);
        }

        public void Dispose() {
            Group.Dispose();
        }

        /// <summary>
        /// コンソールに入室ログを表示
        /// </summary>
        public void WriteConsoleJoinInfo(JoinedUser joinedUser) {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("{JoinRoom}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("<Room>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"Id：{Id}\n" +
                $"Name : {Name}"
                );

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("<User>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"Name : {joinedUser.Name}\n" +
                $"ConnectionID : {joinedUser.ConnectionId}\n" +
                $"JoinOrder : {joinedUser.JoinOrder}\n"
                );
        }

        /// <summary>
        /// コンソールに退室ログを表示
        /// </summary>
        public void WriteConsoleLeaveInfo(Guid connectionId) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{LeaveRoom}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("<Room>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"Id：{Id}\n" +
                $"Name : {Name}"
                );

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("<User>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"Name : {RoomUserDataList[connectionId].joinedUser.Name}\n" +
                $"ConnectionID : {connectionId}\n" +
                $"JoinOrder : {RoomUserDataList[connectionId].joinedUser.JoinOrder}\n"
                );
        }
    }
}
