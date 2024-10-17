using Unity.Netcode;
using UnityEngine;

public class MainMenuDisplay : MonoBehaviour
{
    public float time;
    public dissolve dissolve;

    private void Start() => Refresh();

    public void StartHost()
    {
        Refresh();
        Invoke("StartHostInvoke", time);
        dissolve.enabled = true;
        dissolve.apper = false;
    }

    public void Back()
    {
        Refresh();
        Invoke("BackInvoke", time);
        dissolve.time = 0.8f;
        dissolve.enabled = true;
        dissolve.apper = true;
    }

    public void BackInvoke()
    {
        Refresh();
        dissolve.enabled = false;
    }

    public void StartServer()
    {
        ServerManager.Instance.StartServer();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void StartHostInvoke()
    {
        ServerManager.Instance.StartHost();
        dissolve.enabled = false;
    }

    private void Refresh()
    {
        dissolve.time = 0f;
        dissolve.materials.SetFloat(Shader.PropertyToID("_dissolveAmount"), 0.15f);
        dissolve.materials.SetFloat(Shader.PropertyToID("_verticaldissolve"), 0f);
    }
}
