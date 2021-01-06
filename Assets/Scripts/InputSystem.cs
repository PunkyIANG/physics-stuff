using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using Zenject;

public class InputSystem : MonoBehaviour
{
    public KeybindManager keybindManager;
    private Stopwatch stopwatch = new Stopwatch();
    private Dictionary<GameAction, QueueFP<InputChange>> inputQueue;

    public delegate void InputEventHandler(GameAction action, InputChange inputChange);

    public event InputEventHandler InputEvent = delegate(GameAction action, InputChange change) {  };
    //empty event but hey at least it's not null

    [Inject]
    void Init(KeybindManager keybindManager) {
        this.keybindManager = keybindManager;
    }
    void Start()
    {
        inputQueue = new Dictionary<GameAction, QueueFP<InputChange>>();

        //for each game action
        //add an entry to inputQueue
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
                    var inputChange = new InputChange
                    {
                        Value = currEventType,
                        Timestep = stopwatch.Elapsed
                    };
                    
                    inputQueue[action].Enqueue(inputChange);

                    InputEvent(action, inputChange);
                    
                    //still kinda shitty that the system relies on the queue's last element for comparison
                    //TODO: set the last input event as a separate variable 

                    //debug
                    // var lastEvent = inputQueue[action].FrontPeek();
                    // print(action + " " + lastEvent.Value + " " + lastEvent.Timestep);
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
    private T latestVar = default(T); //kinda null but also supports non-null // also kinda redundant

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