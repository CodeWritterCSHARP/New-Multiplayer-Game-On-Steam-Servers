using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyUIManager : MonoBehaviour
{
    public Text moneyTextPrefab;

    private Dictionary<ulong, Text> moneyTexts = new Dictionary<ulong, Text>();

    public void UpdateMoney(ulong clientId, int money)
    {
        if (!moneyTexts.ContainsKey(clientId))
        {
            Text newText = Instantiate(moneyTextPrefab, transform);
            newText.text = "Player " + clientId + ": " + money.ToString();
            moneyTexts.Add(clientId, newText);
        }
        else
        {
            moneyTexts[clientId].text = "Player " + clientId + ": " + money.ToString();
        }
    }

    public void RemoveMoneyText(ulong clientId)
    {
        if (moneyTexts.ContainsKey(clientId))
        {
            Destroy(moneyTexts[clientId].gameObject);
            moneyTexts.Remove(clientId);
        }
    }
}
