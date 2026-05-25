using LowFPS.Shared.Interfaces.Services;
using System.Threading.Tasks;
using UnityEngine;

public class TestManager : MonoBehaviour {
    [SerializeField] private GameObject playerPrefab;

    private void Awake() {
        RoomModel.I.OnJoinedUser += OnJoinedUser;
    }

    private async void Start() {
        await RoomModel.I.ConnectAsync();

        await RoomModel.I.JoinRoomAsync("TestUser", "TestRoom");

        GameObject createdObj = Instantiate(playerPrefab);
        createdObj.GetComponent<SyncPlayer>().connectionId = RoomModel.I.ConnectionId;
    }

    private async void Update() {
        if (Input.GetKeyDown(KeyCode.F)) {
            await RoomModel.I.SpeedTestAsync();
        }
    }

    private void OnJoinedUser(JoinedUser user) {
        if (user.ConnectionId == RoomModel.I.ConnectionId) {
            return;
        }

        GameObject createdObj = Instantiate(playerPrefab);
        createdObj.GetComponent<SyncPlayer>().connectionId = user.ConnectionId;
    }
}
