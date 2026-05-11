using UnityEngine;
using System.Collections.Generic;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<System.Action> queue = new Queue<System.Action>();
    private static UnityMainThreadDispatcher instance;

    public static void Enqueue(System.Action action)
    {
        if (instance == null)
        {
            var go = new GameObject("[MainThreadDispatcher]");
            instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
        lock (queue) queue.Enqueue(action);
    }

    void Update()
    {
        while (true)
        {
            System.Action action;
            lock (queue)
            {
                if (queue.Count == 0) break;
                action = queue.Dequeue();
            }
            action();
        }
    }
}
