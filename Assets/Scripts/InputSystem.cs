﻿using System.Collections;
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
    private Dictionary<GameAction, QueueFP<InputChange>> inputQueue;

    void Start()
    {
        keybindManager = GetComponent<KeybindManager>();
        inputQueue = new Dictionary<GameAction, QueueFP<InputChange>>();

        //for each game action
        //add an entry to inputqueue
        foreach (GameAction action in Enum.GetValues(typeof(GameAction)))
        {
            inputQueue.Add(action, new QueueFP<InputChange>());
            inputQueue[action] = new QueueFP<InputChange>();
        }

        stopwatch.Start();
    }

    void OnGUI()
    {
        if (Event.current.isKey && Event.current.keyCode != KeyCode.None)
        {
            var currKeycode = Event.current.keyCode;
            var currEventType = Event.current.type;

            foreach (var action in keybindManager.currentConfig.keybinds
                .Where(k => k.keyCode == currKeycode)
                .Select(k => k.action))
            {
                if (inputQueue[action].FrontPeek() == null ||
                    inputQueue[action].FrontPeek().Value != currEventType)
                {
                    inputQueue[action].Enqueue(new InputChange
                    {
                        Value = currEventType,
                        Timestep = stopwatch.Elapsed
                    });

                    //debug
                    var lastEvent = inputQueue[action].FrontPeek();
                    print(action + " " + lastEvent.Value + " " + lastEvent.Timestep);
                }
            }
        }
    }
}

public class InputChange
{
    public EventType Value { get; set; }
    public TimeSpan Timestep { get; set; }
}

public class QueueFP<T> : Queue<T>
{
    private T latestVar = default(T);   //kinda null but also supports non-null 

    public T FrontPeek()
    {
        return latestVar;
    }

    public new void Enqueue(T item)
    {
        latestVar = item;
        base.Enqueue(item);
    }
}

