using System.Threading.Tasks;
using UnityEngine;

public class TestManager : MonoBehaviour {
    private async void Start() {
        await RoomModel.I.ConnectAsync();

        await RoomModel.I.JoinRoomAsync("TestUser", "TestRoom");
    }

    private async void Update() {
        if (Input.GetKeyDown(KeyCode.F)) {
            await RoomModel.I.SpeedTestAsync();
        }
    }
}
