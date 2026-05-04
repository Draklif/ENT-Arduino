using UnityEngine;
using TMPro;

public class CarHUD : MonoBehaviour
{
    [SerializeField] private CarController car;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text rawValuesText;

    void Update()
    {
        var reader = ArduinoSerialReader.Instance;
        if (reader == null) return;

        if (speedText != null && car != null)
            speedText.text = $"{car.CurrentSpeed:F1} m/s";

        if (rawValuesText != null)
            rawValuesText.text = $"Speed: {reader.RawSpeed}\nSteering: {reader.RawSteering}";
    }
}