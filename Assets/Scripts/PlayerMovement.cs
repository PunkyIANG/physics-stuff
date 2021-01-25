using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Zenject;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update

    [Inject]
    public InputSystem inputSystem;

    public float acceleration;
    private Rigidbody2D _playerRb;
    public ForceMode2D mode;
    private Dictionary<GameAction, InputChange> _lastEvent;
    private Dictionary<GameAction, Vector2> _eventDir;
    private void Start()
    {
        _lastEvent = new Dictionary<GameAction, InputChange>();
        _playerRb = GetComponent<Rigidbody2D>();

        foreach (GameAction action in Enum.GetValues(typeof(GameAction)))
        {
            _lastEvent.Add(action, new InputChange
            {
                Value = EventType.KeyUp,
                Timestep = inputSystem.stopwatch.Elapsed
            });
        }

        _eventDir = new Dictionary<GameAction, Vector2>();

        _eventDir.Add(GameAction.MoveUp, Vector2.up);
        _eventDir.Add(GameAction.MoveDown, Vector2.down);
        _eventDir.Add(GameAction.MoveLeft, Vector2.left);
        _eventDir.Add(GameAction.MoveRight, Vector2.right);

        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        ForceMovement();
    }

    private void ForceMovement()
    {
        // var inputMovement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        // _playerRb.AddForce(inputMovement.normalized * acceleration, mode);

        var currentTimestep = inputSystem.stopwatch.Elapsed;
        var targetVelocity = Vector2.zero;

        foreach (var action in _eventDir.Keys)
        {
            while (inputSystem.inputQueue[action].Count != 0)
            {
                // print("stuff");

                var currInput = inputSystem.inputQueue[action].Dequeue();
                var lastInput = _lastEvent[action];

                if (lastInput.Value == EventType.KeyDown
                    && currInput.Value == EventType.KeyUp)
                {
                    targetVelocity += _eventDir[action] * GetDiff(currInput, lastInput) * acceleration;
                }

                _lastEvent[action] = currInput;
            }

            if (_lastEvent[action].Value == EventType.KeyDown)
            {
                targetVelocity += _eventDir[action]
                    * GetDiff(currentTimestep, _lastEvent[action].Timestep)
                    * acceleration;

                _lastEvent[action] = new InputChange
                {
                    Value = EventType.KeyDown,
                    Timestep = currentTimestep
                };
            }
        }

        if (targetVelocity != Vector2.zero)
        {
            _playerRb.AddForce(targetVelocity, mode);
            // print("FORCE!");
        }
    }

    private float GetDiff(TimeSpan first, TimeSpan second)
    {
        var diffTicks = first.Ticks - second.Ticks;
        var result = (float)(first.Ticks - second.Ticks) / Stopwatch.Frequency;
        // print(diffTicks + " " + result);
        return result;
    }

    private float GetDiff(InputChange first, InputChange second)
    {
        return GetDiff(first.Timestep, second.Timestep);
    }
}