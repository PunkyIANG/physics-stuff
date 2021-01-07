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
    private Dictionary<GameAction, Queue<InputChange>> inputQueue;
    private Dictionary<GameAction, EventType> lastEvent;

    public delegate void InputEventHandler(GameAction action, InputChange inputChange);

    public event InputEventHandler InputEvent = delegate(GameAction action, InputChange change) {  };
    //empty event but hey at least it's not null

    [Inject]
    void Init(KeybindManager keybindManager) {
        this.keybindManager = keybindManager;
    }
    
    void Start()
    {
        inputQueue = new Dictionary<GameAction, Queue<InputChange>>();
        lastEvent = new Dictionary<GameAction, EventType>();

        //for each game action
        //add an entry to inputQueue
        foreach (GameAction action in Enum.GetValues(typeof(GameAction)))
        {
            inputQueue.Add(action, new Queue<InputChange>());
            inputQueue[action] = new Queue<InputChange>();

            lastEvent[action] = EventType.KeyUp;
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
                if (lastEvent[action] != currEventType)
                {
                    var inputChange = new InputChange
                    {
                        Value = currEventType,
                        Timestep = stopwatch.Elapsed
                    };

                    lastEvent[action] = currEventType;
                    
                    inputQueue[action].Enqueue(inputChange);

                    InputEvent(action, inputChange);
                    
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