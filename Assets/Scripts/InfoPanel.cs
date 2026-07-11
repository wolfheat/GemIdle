using TMPro;
using UnityEngine;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI infoTextField;

    public static InfoPanel Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public void ShowInfo(string infoText)
    {
        // Show text, maybe later hace this on a timer?
        infoTextField.text = infoText;
    }
}
