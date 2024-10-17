using UnityEngine;
using UnityEngine.UI;

public class FieldDiscription : MonoBehaviour
{
    private FieldSpawn fieldSpawn;
    private FieldDisplay FieldDisplay;
    private Transform showGrid;
    private int index;

    private void Start()
    {
        fieldSpawn = FindObjectOfType<FieldSpawn>();
        FieldDisplay = GetComponent<FieldDisplay>();
        index = FieldDisplay.index;
        showGrid = GameObject.FindGameObjectWithTag("cardinfo").transform;
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ShowGrid(true);
            string owner = FieldDisplay.owner.ToString();
            showGrid.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = fieldSpawn.scriptables[index].name + " owner: " + owner;
            showGrid.GetChild(1).transform.GetChild(0).GetComponent<Text>().text = fieldSpawn.scriptables[index].description;
            showGrid.GetChild(2).transform.GetChild(0).GetComponent<Text>().text = fieldSpawn.scriptables[index].moneyPrice.ToString();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowGrid(false);
        }
    }

    private void ShowGrid(bool show)
    {
        foreach (Transform child in showGrid)
        {
            child.gameObject.SetActive(show);
        }
    }
}
