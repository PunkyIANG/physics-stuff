using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    // public Queue<InputFrame<Vector2>> movementInputQueue = new Queue<InputFrame<Vector2>>();
    
    private Stopwatch stopwatch = new Stopwatch();
    private TimeSpan previousTime;

    void Start()
    {
        stopwatch.Start();
    }

    void Update()
    {
        
    }

    void OnGUI() {
        if (Event.current.ToString() != "Layout" && Event.current.ToString() != "repaint")
            print(Event.current);
    }
}

public class InputFrame<T> {
    public T Value {get; set; }
    public float Timestep { get; set; }
}
