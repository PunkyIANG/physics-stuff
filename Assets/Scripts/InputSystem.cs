using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;
using UnityEngine;

#region InputSystem

public class InputSystem : MonoBehaviour
{
    Vector2 rawInputVector = Vector2.zero;
    public Queue<InputFrame<Vector2>> movementInputQueue = new Queue<InputFrame<Vector2>>();
    
    // Start is called before the first frame update

    private Stopwatch stopwatch = new Stopwatch();
    private TimeSpan previousTime;

    void Start()
    {
        stopwatch.Start();
    }

    // Update is called once per frame
    void Update()
    {
        // print(rawInputVector);
        rawInputVector = Vector2.zero;
        
    }

    void OnGUI() {

        rawInputVector += (Input.GetAxisRaw("Horizontal") * Vector2.right
            + Input.GetAxisRaw("Vertical") * Vector2.up)
            * (stopwatch.Elapsed - previousTime).Milliseconds / 1000;   //ms to s
        
        print((stopwatch.Elapsed - previousTime));

        previousTime = stopwatch.Elapsed;
    }
}

public class InputFrame<T> {
    public T Value {get; set; }
    public float Timestep { get; set; }
}

#endregion

#region KeybindConfig
//keep this class persistent between scenes
public class KeybindManager : MonoBehaviour {

    private KeybindConfig currentConfig;
    private const string configFilePath = Application.dataPath + "inputConfig.cfg";

    void Start() {
        ReloadConfig();
    }

    private void ReloadConfig()
    {
        try 
        {
            var config = JsonUtility.FromJson<KeybindConfig>(File.ReadAllText(configFilePath));
        } 
        catch (Exception e) 
        {
            print(e);
            ResetKeybindsConfig();
        }
    }

    public void ResetKeybindsConfig() {
        //create default config object
        SaveConfig();
    }

    public void SetKeybind(KeybindConfig newConfig) {
        currentConfig = newConfig;
        SaveConfig();
    }

    public void SaveConfig() {
        //save current config to file
    }
}

[Serializable]
public class KeybindConfig {
    public KeyCode KeybindUp { get; set; } = KeyCode.W;
    public KeyCode KeybindDown { get; set; } = KeyCode.S;
    public KeyCode KeybindLeft { get; set; } = KeyCode.A;
    public KeyCode KeybindRight { get; set; } = KeyCode.D;
    public KeyCode KeybindEnter { get; set; } = KeyCode.Return;
    public KeyCode KeybindX { get; set; } = KeyCode.X;
}

#endregion