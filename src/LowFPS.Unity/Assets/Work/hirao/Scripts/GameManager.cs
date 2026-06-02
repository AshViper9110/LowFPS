using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField] private GameObject SpectateUI;


    [Header("HPBar")]
    [SerializeField] private Text hpText;

    [Header("Weapons")]
    [SerializeField] private Text gunNameText;
    [SerializeField] private Text ammoText;

    [Header("killLog")]
    [SerializeField] private Text killLogText;

    [SerializeField] private List<Transform> respawnPoint = new List<Transform>();

    private void Awake()
    {
        NetworkManager.I.JoinRoom();
    }
    
    public async void ReSpawnAsync()
    {
        NetworkManager.I.spawnPoint = respawnPoint[UnityEngine.Random.Range(0, respawnPoint.Count)].transform;
        await RoomModel.I.ReSpawnAsync();
    }

    private void OnEnable()
    {
        if (RoomModel.I != null)
        {
            RoomModel.I.Ondead += Ondead;
        }
    }

    private void OnDisable()
    {
        if (RoomModel.I != null)
        {
            RoomModel.I.Ondead -= Ondead;
        }
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

    private void Update()
    {
        if (NetworkManager.I.MyPlayerCon == null) return;
        ShowHP();
        ShowAmmo();
    }

    private void ShowHP()
    {
        var player = InRoomPlayerData.I.PlayerList[RoomModel.I.ConnectionId].joinedUser;

        int hp = player.Hp;

        hpText.text = $"{hp}/{100}";
    }

    private void ShowAmmo()
    {
        ammoText.text = $"{NetworkManager.I.MyPlayerCon.currentAmmo}/{NetworkManager.I.MyPlayerCon.gunData.AmmoRounds}";
        gunNameText.text = NetworkManager.I.MyPlayerCon.gunData.GunName;
    }

    private void Ondead(System.Guid myConnectionId, System.Guid enemyConnectionId)
    {
        string killPlayerName =
            InRoomPlayerData.I.PlayerList[enemyConnectionId].joinedUser.Name;

        string deathPlayerName =
            InRoomPlayerData.I.PlayerList[myConnectionId].joinedUser.Name;

        string log = $"{killPlayerName} killed {deathPlayerName}";

        killLogText.text = log + "\n" + killLogText.text;
    }
}
