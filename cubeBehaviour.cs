using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class cubeBehaviour : NetworkBehaviour
{
    public turnArray turn;

    [SerializeField] private Transform plane;
    [SerializeField] private float powerMin, powerMax;

    private void Awake() => Find();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (turn == null) Find();
    }
    private void Find() { turn = FindObjectOfType<turnArray>(); }

    [SerializeField] private float time;
    [SerializeField] private Transform[] points;
    public int number;

    public void SetNumber(int value)
    {
        number = value;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plane"))
        {
               Invoke("Nt", time);
        }
    }

    void Nt()
    {
        float max = -1000f;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].position.y > max) { max = points[i].position.y; if (NetworkManager.Singleton.LocalClientId == turn.currentPlayerId.Value) SetNumber(i + 1); }
        }
       // turn.HandleCubeCollisionServerRpc(NetworkObjectId);
    }
}
