using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    public float targetTime;
    public bool isActive;

    private TextMeshProUGUI _counterText;

    private void Start()
    {
        _counterText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if(isActive) targetTime -= Time.deltaTime;
        if (targetTime <= 0) return;

        _counterText.text = $"{targetTime:00}";
    }
}
