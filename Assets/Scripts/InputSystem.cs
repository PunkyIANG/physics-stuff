using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;
using UnityEngine;
using System.Linq;

public class InputSystem : MonoBehaviour
{
    public KeybindManager keybindManager;
    private Stopwatch stopwatch = new Stopwatch();
    private TimeSpan previousTime;
    private HashSet<KeyCode> usedKeys;
    private Dictionary<GameAction,QueueFP<InputChange>> inputQueue;

    void Start()
    {
        keybindManager = GetComponent<KeybindManager>();
        keybindManager.ConfigReload += ReloadUsedKeys;
        inputQueue = new Dictionary<GameAction, QueueFP<InputChange>>();

        //for each game action
        //add an entry to inputqueue
        foreach(GameAction gameAction in Enum.GetValues(typeof(GameAction))) {
            inputQueue.Add(gameAction, new QueueFP<InputChange>());
        }

        stopwatch.Start();
    }

    void OnGUI() {
        if (Event.current.isKey && Event.current.keyCode != KeyCode.None) {
            var currKeycode = Event.current.keyCode;
            var currEventType = Event.current.type;

            foreach(var keybind in keybindManager.currentConfig.keybinds
                .Where(k => k.keyCode == currKeycode)
                .Select(k => k.action)) 
            {
                if (inputQueue[keybind].FrontPeek().Value != currEventType) {
                    inputQueue[keybind].Enqueue(new InputChange {
                        Value = currEventType,
                        Timestep = (float)stopwatch.ElapsedMilliseconds / 1000 
                    });
                }
            }
            print(currKeycode + " " + currEventType);
        }
    }

    void ReloadUsedKeys() {
        usedKeys = new HashSet<KeyCode>();
        
        foreach(var keybind in keybindManager.currentConfig.keybinds) {
            usedKeys.Add(keybind.keyCode);
        }
    }
}

public class InputChange {
    public EventType Value { get; set; }
    public float Timestep { get; set; }
}

public class QueueFP<T> : Queue<T> {
    private T latestVar;

    public T FrontPeek() {
        return latestVar;
    }

    public new void Enqueue(T item) {
        latestVar = item;
        base.Enqueue(item);
    }
}

