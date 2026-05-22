using Cysharp.Runtime.Multicast;
using System.Collections.Concurrent;

namespace LowFPS.Server.StreamingHubs {
    public class RoomContextRepository(IMulticastGroupProvider groupProvider) {
        private readonly ConcurrentDictionary<string, RoomContext> contexts =
            new ConcurrentDictionary<string, RoomContext>();

        /// <summary>
        /// ルームコンテキストの作成
        /// </summary>
        public RoomContext CreateContext(string roomName) {
            var context = new RoomContext(groupProvider, roomName);
            contexts[roomName] = context;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("{CreateRoom}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("<Room>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"Id：{context.Id}\n" +
                $"Name : {context.Name}\n"
                );

            return context;
        }

        /// <summary>
        /// コンテキストの取得
        /// </summary>
        public RoomContext? GetContext(string roomName) {
            if (!contexts.ContainsKey(roomName)) {
                return null;
            }
            return contexts[roomName];
        }

        /// <summary>
        /// ルームコンテキストの削除
        /// </summary>
        public void RemoveContext(string roomName) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{DeleteRoom}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("<Room>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"Id：{contexts[roomName].Id}\n" +
                $"Name : {contexts[roomName].Name}\n"
                );

            if (contexts.Remove(roomName, out var RoomContext)) {
                RoomContext.Dispose();
            }
        }

        /// <summary>
        /// 全ルームコンテキストの取得
        /// </summary>
        public ConcurrentDictionary<string, RoomContext> GetAllContext() {
            return contexts;
        }
    }
}
