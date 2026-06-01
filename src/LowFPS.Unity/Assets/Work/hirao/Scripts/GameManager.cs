using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] private GameObject SpectateUI;
    private void Awake()
    {
        NetworkManager.I.JoinRoom();
    }
    
    public async void ReSpawnAsync()
    {
        await RoomModel.I.ReSpawnAsync();
        SpectateUI.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (NetworkManager.I.MyPlayerCon == null) return;

        if (NetworkManager.I.MyPlayerCon.isDead)
        {
            SpectateUI.SetActive(true);
        }
        else
        {
            SpectateUI.SetActive(false);
            return;
        }
    }
}
