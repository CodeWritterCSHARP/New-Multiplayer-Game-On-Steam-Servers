using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class FieldSpawn : NetworkBehaviour
{
    [SerializeField] private GameObject current;
    public Field[] scriptables;

    private int middlePos = 10;
    private bool isSpawned = false;

    private int count = 0;

    private void Start()
    {
        SpawnObjectsOnce();
    }

    public void SpawnObjectsOnce()
    {
        for (int i = 1; i < 3; i++) MassiveAdd(i, true);
        for (int i = 1; i < 3; i++) MassiveAdd(i, false);
    }

    private void MassiveAdd(int type, bool sunta)
    {
        if (sunta)
        {
            for (int x = -middlePos; x < 12; x += 2)
            {
                for (int y = -middlePos; y < 12; y += 2)
                {
                    if ((type == 1 && x == -middlePos && y != middlePos) || (type == 2 && y == middlePos))
                    {
                        Spawn(x, y, type);
                    }
                }
            }
        }
        else
        {
            for (int x = 10; x > -middlePos - 2; x -= 2)
            {
                for (int y = 10; y > -middlePos - 2; y -= 2)
                {
                    if ((type == 1 && x == middlePos && y != middlePos) || (type == 2 && x != -middlePos && x != middlePos && y == -middlePos))
                    {
                        Spawn(x, y, type);
                    }
                }
            }
        }
    }

    private void Spawn(int x, int y, int type)
    {
        try
        {
            var cur = Instantiate(current, new Vector3(x, -9.5f, y), Quaternion.identity);

            cur.name = scriptables[count].name;
            FieldDisplay fieldDisplay = cur.GetComponent<FieldDisplay>();
            fieldDisplay.index = count;

            fieldDisplay.money = -scriptables[count].firstbuy;
            fieldDisplay.secondUp = -scriptables[count].secondbuy;
            fieldDisplay.thirdUp = -scriptables[count].thirdbuy;
            fieldDisplay.fourthUp = -scriptables[count].fourthbuy;

            count++;
            switch (type)
            {
                case 1: cur.transform.GetChild(1).localPosition = new Vector3(-1f, cur.transform.position.y, 0.55f); break;
                case 2: cur.transform.GetChild(1).localPosition = new Vector3(-0.15f, cur.transform.position.y, 1.6f); break;
                case 3: cur.transform.GetChild(1).localPosition = new Vector3(0.675f, cur.transform.position.y, -0.775f); break;
                case 4: cur.transform.GetChild(1).localPosition = new Vector3(-0.115f, cur.transform.position.y, -0.6f); break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}