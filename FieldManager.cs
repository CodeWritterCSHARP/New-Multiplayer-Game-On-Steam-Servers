using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System;
using UnityEngine.UI;
using UnityEngine;

public class FieldManager : NetworkBehaviour
{
    public NetworkList<int> fieldOwners;
    public Text listDisplayText;
    public GameObject sphere;

    public NetworkList<int> fieldUpgrades; //

    private void Awake()
    {
        fieldOwners = new NetworkList<int>();
        if (listDisplayText == null) listDisplayText = GameObject.Find("FieldsStats").GetComponent<Text>();

        fieldUpgrades = new NetworkList<int>(); //
    }

    private void Start()
    {
        fieldOwners.OnListChanged += OnFieldOwnersChanged;

        fieldUpgrades.OnListChanged += OnFieldUpgradesChanged; //

        if (IsHost)
        {
            InitializeFields();
        }
    }

    //
    private void OnFieldUpgradesChanged(NetworkListEvent<int> changeEvent)
    {
        FieldDisplay[] fields = FindObjectsOfType<FieldDisplay>();
        foreach (FieldDisplay field in fields)
        {
            if (field.index >= 0 && field.index < fieldOwners.Count/* && field.owner == -1*/)
            {
                field.UpdateUpgrade(fieldUpgrades[field.index]); //
            }
        }
    }
    //

    private void InitializeFields()
    {
        var fieldSpawn = FindObjectOfType<FieldSpawn>();
        if (fieldSpawn != null)
        {
            int fieldCount = fieldSpawn.scriptables.Length;
            for (int i = 0; i < fieldCount; i++)
            {
                fieldOwners.Add(-1);
                fieldUpgrades.Add(0);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BuyFieldServerRpc(int fieldIndex, ulong clientId)
    {
        if (fieldIndex >= 0 && fieldIndex < fieldOwners.Count && fieldOwners[fieldIndex] == -1)
        {
            fieldOwners[fieldIndex] = (int)clientId;

            FieldSpawn fieldspawn = FindObjectOfType<FieldSpawn>();
            string name = fieldspawn.scriptables[fieldIndex].name;
            Vector3 pos = GameObject.Find(name).transform.position;
            pos.y += 2f;

            GameObject curSphere = Instantiate(sphere, pos, Quaternion.identity);
            curSphere.GetComponent<NetworkObject>().Spawn();

            FieldDisplay fieldDisplay = FindFieldDisplayByIndex(fieldIndex);
            if (fieldDisplay != null) fieldDisplay.sphereID = curSphere.GetComponent<NetworkObject>().NetworkObjectId;

            Color color = GetColorForClient((int)clientId);
            ChangeSphereColorClientRpc(curSphere.GetComponent<NetworkObject>().NetworkObjectId, color);
        }

        //
        if(fieldIndex >= 0 && fieldIndex < fieldOwners.Count && fieldUpgrades[fieldIndex] < 4)
        {
            fieldUpgrades[fieldIndex] += 1;
        }
        //
    }

    public FieldDisplay FindFieldDisplayByIndex(int fieldIndex)
    {
        FieldDisplay[] fields = FindObjectsOfType<FieldDisplay>();
        foreach (FieldDisplay field in fields)
        {
            if (field.index == fieldIndex)
            {
                return field;
            }
        }
        return null;
    }

    [ClientRpc]
    public void ChangeSphereColorClientRpc(ulong networkObjectId, Color color)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            GameObject sphere = networkObject.gameObject;
            var renderer = sphere.GetComponent<MeshRenderer>();

            if (renderer != null) renderer.material.color = color;
            else Debug.LogError("MeshRenderer not found on the spawned sphere object.");
        }
        else Debug.LogError("NetworkObject not found on the client.");
    }

    public Color GetColorForClient(int clientId)
    {
        Color color = Color.white;
        switch (clientId)
        {
            case 0: color = Color.green; break;
            case 1: color = Color.yellow; break;
            case 2: color = Color.blue; break;
            case 3: color = Color.red; break;
            default: break;
        }
        return color;
    }

    private void OnFieldOwnersChanged(NetworkListEvent<int> changeEvent)
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (fieldOwners.Count == 0 || fieldUpgrades.Count == 0)
        {
            Debug.LogWarning("Field owners or upgrades list is empty. UI cannot be updated.");
            return;
        }

        FieldSpawn fieldspawn = FindObjectOfType<FieldSpawn>();
        listDisplayText.text = "Field Owners: " + Environment.NewLine;
        for (int i = 0; i < fieldOwners.Count; i++)
        {
            listDisplayText.text += $"Field {fieldspawn.scriptables[i].name}: {fieldOwners[i]} " + Environment.NewLine;
        }

        FieldDisplay[] fields = FindObjectsOfType<FieldDisplay>();
        foreach (FieldDisplay field in fields)
        {
            if (field.index >= 0 && field.index < fieldOwners.Count/* && field.owner == -1*/)
            {
                field.UpdateOwner((ulong)fieldOwners[field.index]);
            }
        }
    }
}
