using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DiceBehaviour : NetworkBehaviour
{
    public SpawnDice parent;
    [SerializeField] private Transform plane;
    [SerializeField] private float powerMin, powerMax;
    [SerializeField] private float time;
    [SerializeField] private Transform[] points;
    [SerializeField] private float autoDestoyTime;
    private bool isGround = false;

    [SerializeField] private Text diceIndexText;//

    private NetworkVariable<int> diceIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        try { plane = GameObject.FindGameObjectWithTag("Plane").transform; } catch { Debug.Log("Not found plane"); }
        if (diceIndexText == null) diceIndexText = GameObject.Find("DiceInfo").GetComponent<Text>();//
    }

    private void Start()
    {
        Vector3 direction = transform.position - plane.position;
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.drag = Random.Range(0.1f, 0.3f);
        rigidbody.angularDrag = Random.Range(0.05f, 0.1f);
        rigidbody.AddForceAtPosition(direction.normalized * Random.Range(powerMin, powerMax), transform.position);

        float power = 0;
        int dir = Random.Range(1, 3);
        if (dir == 1) power = Random.Range(-0.6f, 0f); else power = Random.Range(0f, 0.6f);
        rigidbody.AddTorque(transform.up * power, ForceMode.VelocityChange);
        dir = Random.Range(1, 3);
        if (dir == 1) power = Random.Range(0.5f, 1f); else power = Random.Range(-1f, -0.5f);
        rigidbody.AddTorque(transform.right * power, ForceMode.VelocityChange);

        Invoke("AutoDestroyServerRpc", autoDestoyTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plane") && isGround == false) { Invoke("OnGround", time); isGround = true; }
    }

    private void OnGround()
    {
        float max = -1000f;
        int index = 0;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].position.y > max) { max = points[i].position.y; index = i + 1; }
        }
        UpdateDiceIndexServerRpc(index); //
        if (parent != null) parent.DestroyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void AutoDestroyServerRpc()
    {
        try
        {
            GetComponent<NetworkObject>().Despawn();
            Destroy(gameObject);
        }
        catch { }
    }

    //
    [ServerRpc(RequireOwnership = false)]
    private void UpdateDiceIndexServerRpc(int index)
    {
        diceIndex.Value = index;
    }

    private void OnEnable()
    {
        diceIndex.OnValueChanged += OnDiceIndexChanged;
    }

    private void OnDisable()
    {
        diceIndex.OnValueChanged -= OnDiceIndexChanged;
    }

    private void OnDiceIndexChanged(int oldIndex, int newIndex)
    {
        UpdateDiceIndexUIClientRpc(newIndex);
    }

    [ClientRpc]
    private void UpdateDiceIndexUIClientRpc(int newIndex)
    {
        if (diceIndexText != null)
        {
            diceIndexText.text += newIndex.ToString();
            if (diceIndexText.text.Length == 1) diceIndexText.text = "Dice index: ";
        }
    }
}
