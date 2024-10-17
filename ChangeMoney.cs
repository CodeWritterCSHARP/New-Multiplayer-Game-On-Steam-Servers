using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System;

public class ChangeMoney : NetworkBehaviour
{
    private void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.E) && IsLocalPlayer)
        {
            ChangeServerRpc(10);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangeServerRpc(int addValue, ServerRpcParams rpcParams = default)
    {
        ChangeClientRpc(addValue);
    }

    [ClientRpc]
    void ChangeClientRpc(int addValue)
    {
        Text curText = GameObject.Find(NetworkManager.Singleton.LocalClientId.ToString()).GetComponent<Text>();
        Debug.Log(curText.name);
        string number = ""; bool add = false;
        for (int i = 0; i < curText.text.Length; i++)
        {
            if (add == true) number += curText.text[i];
            if (curText.text[i] == ':') add = true;
        }
        int playerN = Convert.ToInt32(number);
        playerN += addValue;
        curText.text = NetworkManager.Singleton.LocalClientId.ToString() + ":" + playerN.ToString();
    }
}
