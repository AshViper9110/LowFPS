using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject loadoutPanel;

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Title View")]
    [SerializeField] private Vector3 titlePos = new Vector3(0, 0.8f, -10);
    [SerializeField] private Vector3 titleRot = new Vector3(0, 0, 0);

    [Header("Loadout View")]
    [SerializeField] private Vector3 loadoutPos = new Vector3(9, 0.8f, 11);
    [SerializeField] private Vector3 loadoutRot = new Vector3(0, 98, 0);

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 5f;

    private Vector3 targetPos;
    private Quaternion targetRot;

    private void Start()
    {
        // 初期位置をタイトル画面に設定
        MoveCameraInstant(titlePos, titleRot);

        targetPos = titlePos;
        targetRot = Quaternion.Euler(titleRot);

        ShowTitlePanel();
    }

    private void Update()
    {
        // 位置補間
        targetCamera.transform.position = Vector3.Lerp(
            targetCamera.transform.position,
            targetPos,
            Time.deltaTime * moveSpeed
        );

        // 回転補間
        targetCamera.transform.rotation = Quaternion.Lerp(
            targetCamera.transform.rotation,
            targetRot,
            Time.deltaTime * rotateSpeed
        );
    }

    public void GammePlay()
    {
        Debug.Log("Push Play");
        SceneManager.LoadScene("TestScene");
    }

    public void SetLoadout()
    {
        Debug.Log("Push SetLoadout");

        MoveCamera(loadoutPos, loadoutRot);

        if (titlePanel != null) titlePanel.SetActive(false);
        if (loadoutPanel != null) loadoutPanel.SetActive(true);
    }

    public void backTitle()
    {
        Debug.Log("Push backTitle");

        MoveCamera(titlePos, titleRot);

        ShowTitlePanel();
    }

    public void Quit()
    {
        Debug.Log("Push Quit");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowOption()
    {
        Debug.Log("Push ShowOption");
    }

    // =========================
    // Camera Move
    // =========================

    private void MoveCamera(Vector3 pos, Vector3 rot)
    {
        targetPos = pos;
        targetRot = Quaternion.Euler(rot);
    }

    private void MoveCameraInstant(Vector3 pos, Vector3 rot)
    {
        targetCamera.transform.position = pos;
        targetCamera.transform.rotation = Quaternion.Euler(rot);
    }

    private void ShowTitlePanel()
    {
        if (titlePanel != null) titlePanel.SetActive(true);
        if (loadoutPanel != null) loadoutPanel.SetActive(false);
    }
}