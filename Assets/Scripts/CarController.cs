using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float maxSteeringAngle = 90f;
    [SerializeField] private float inputSmoothing = 5f;

    [Header("Calibration")]
    [SerializeField, Range(0f, 0.3f)] private float deadZone = 0.05f;
    [SerializeField] private bool invertSpeed = false;
    [SerializeField] private bool invertSteering = false;

    private float currentSpeed;
    private float currentSteering;

    public float CurrentSpeed => currentSpeed;

    void Update()
    {
        var reader = ArduinoSerialReader.Instance;
        if (reader == null || !reader.IsConnected) return;

        reader.GetNormalizedValues(out float rawSpeed, out float rawSteering);

        if (invertSpeed) rawSpeed = -rawSpeed;
        if (invertSteering) rawSteering = -rawSteering;

        float targetSpeed = ApplyDeadZone(rawSpeed) * maxSpeed;
        float targetSteering = ApplyDeadZone(rawSteering) * maxSteeringAngle;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * inputSmoothing);
        currentSteering = Mathf.Lerp(currentSteering, targetSteering, Time.deltaTime * inputSmoothing);

        // Steer only when moving
        float steeringFactor = Mathf.Sign(currentSpeed) * Mathf.Clamp01(Mathf.Abs(currentSpeed) / 2f);

        transform.Rotate(0f, currentSteering * steeringFactor * Time.deltaTime, 0f);
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }

    private float ApplyDeadZone(float value)
    {
        if (Mathf.Abs(value) < deadZone) return 0f;
        float sign = Mathf.Sign(value);
        return sign * (Mathf.Abs(value) - deadZone) / (1f - deadZone);
    }
}