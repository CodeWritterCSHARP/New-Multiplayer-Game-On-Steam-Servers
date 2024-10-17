using UnityEngine;
using UnityEngine.UI;

public class PurchaseDialog : MonoBehaviour
{
    public Text messageText;
    public Button yesButton;
    public Button noButton;

    public void Setup(string message, System.Action onYes, System.Action onNo)
    {
        messageText.text = message;

        yesButton.onClick.AddListener(() =>
        {
            onYes?.Invoke();
            Destroy(gameObject);
        });

        noButton.onClick.AddListener(() =>
        {
            onNo?.Invoke();
            Destroy(gameObject);
        });
    }
}
