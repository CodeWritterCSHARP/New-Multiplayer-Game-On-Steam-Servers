using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Field", menuName = "Field")]
public class Field : ScriptableObject
{
    public string name;

    [TextArea]
    public string description;

    public int moneyPrice;
    public int unitPrice;

    public int stepOnMPrice;
    public int stepOnUPrice;

    public int type;

    public GameObject building;

    public int firstbuy;
    public int secondbuy;
    public int thirdbuy;
    public int fourthbuy;
}
