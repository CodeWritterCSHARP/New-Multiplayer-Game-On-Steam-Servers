using UnityEngine;
using UnityEngine.Video;
using Unity.Netcode;

public class StartGameIntro : NetworkBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private GameObject rawImage;
    [SerializeField] private GameObject canvas;
    [SerializeField] private float timer = 7f;
    [SerializeField] private SpawnDice spawnDice;
    [SerializeField] private GameObject btn;

    private void Start()
    {
        targetObject.GetComponent<VideoPlayer>().frame = 0;
        if (NetworkManager.Singleton.IsHost) btn.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetActiveServerRpc(bool isActive)
    {
        targetObject.GetComponent<VideoPlayer>().frame = 0;
        targetObject.SetActive(isActive); rawImage.SetActive(isActive); if(canvas!=null) canvas.SetActive(false);
        SetActiveClientRpc(isActive);
    }

    [ClientRpc]
    private void SetActiveClientRpc(bool isActive)
    {
        targetObject.GetComponent<VideoPlayer>().frame = 0;
        targetObject.SetActive(isActive); rawImage.SetActive(isActive); if (canvas != null) canvas.SetActive(false);
    }
    public void TriggerSetActive(bool isActive) { SetActiveServerRpc(isActive); Invoke("InvokeIt", timer); }

    public void OnNetworkReady()
    {
        if (btn.activeSelf) btn.SetActive(false);
        if (NetworkManager.Singleton.IsHost) { spawnDice.sp(); spawnDice.sp(); TriggerSetActive(true); }
        else if (NetworkManager.Singleton.IsClient) SetActiveClientRpc(true);
        if (IsHost) FindObjectOfType<turnArray>().StartTimer();
    }

    private void InvokeIt()
    {
        if (NetworkManager.Singleton.IsHost) { TriggerSetActive(false); }
        else if (NetworkManager.Singleton.IsClient) SetActiveClientRpc(false);
    }
}
