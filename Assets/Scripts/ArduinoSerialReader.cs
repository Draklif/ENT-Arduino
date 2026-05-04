using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class ArduinoSerialReader : MonoBehaviour
{
    public static ArduinoSerialReader Instance { get; private set; }

    [Header("Serial Configuration")]
    [SerializeField] private string portName = "COM3";
    [SerializeField] private int baudRate = 9600;
    [SerializeField] private int readTimeoutMs = 100;
    [SerializeField] private bool autoConnectOnStart = true;

    // Latest raw values from Arduino (0-1023)
    public int RawSpeed { get; private set; }
    public int RawSteering { get; private set; }
    public bool IsConnected { get; private set; }

    private SerialPort serialPort;
    private Thread readThread;
    private volatile bool running;
    private readonly object dataLock = new object();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (autoConnectOnStart) Connect();
    }

    public void Connect()
    {
        if (IsConnected)
        {
            Debug.LogWarning("[ArduinoSerialReader] Already connected.");
            return;
        }

        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = readTimeoutMs;
            serialPort.NewLine = "\n";
            serialPort.Open();

            running = true;
            readThread = new Thread(ReadLoop) { IsBackground = true };
            readThread.Start();

            IsConnected = true;
            Debug.Log($"[ArduinoSerialReader] Connected to {portName}");
        }
        catch (System.Exception e)
        {
            IsConnected = false;
            Debug.LogError($"[ArduinoSerialReader] Failed to open {portName}: {e.Message}");
        }
    }

    public void Disconnect()
    {
        running = false;

        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join(500);
        }

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }

        IsConnected = false;
        Debug.Log("[ArduinoSerialReader] Disconnected.");
    }

    private void ReadLoop()
    {
        while (running)
        {
            try
            {
                string line = serialPort.ReadLine();
                ParseLine(line);
            }
            catch (System.TimeoutException)
            {
                // Expected when no data is ready
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ArduinoSerialReader] Read error: {e.Message}");
            }
        }
    }

    private void ParseLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return;

        string[] parts = line.Trim().Split(',');
        if (parts.Length != 2) return;

        if (int.TryParse(parts[0], out int speed) &&
            int.TryParse(parts[1], out int steering))
        {
            lock (dataLock)
            {
                RawSpeed = speed;
                RawSteering = steering;
            }
        }
    }

    /// <summary>
    /// Returns normalized values in range [-1, 1], centered at 512.
    /// </summary>
    public void GetNormalizedValues(out float speed, out float steering)
    {
        lock (dataLock)
        {
            speed = (RawSpeed - 512) / 512f;
            steering = (RawSteering - 512) / 512f;
        }

        speed = Mathf.Clamp(speed, -1f, 1f);
        steering = Mathf.Clamp(steering, -1f, 1f);
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Disconnect();
            Instance = null;
        }
    }
}