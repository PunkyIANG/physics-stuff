using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using UnityEngine;

//keep this class persistent between scenes
public class KeybindManager : MonoBehaviour
{
    public KeybindConfig currentConfig;
    private string configFilePath;

    void Start()
    {
        configFilePath = Application.dataPath + "inputConfig.cfg";
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

    public void ResetKeybindsConfig()
    {
        //create default config object
        SaveConfig();
    }

    public void SetKeybind(KeybindConfig newConfig)
    {
        currentConfig = newConfig;
        SaveConfig();
    }

    public void SaveConfig()
    {
        //save current config to file
    }
}

public enum GameAction
{
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Hook
}

public class Keybind
{
    public GameAction action;
    public KeyCode keyCode;

    public Keybind(GameAction action, KeyCode keyCode)
    {
        this.action = action;
        this.keyCode = keyCode;
    }
}

[Serializable]
public class KeybindConfig
{
    public Keybind[] keybinds = new Keybind[] {
        new Keybind(GameAction.MoveUp, KeyCode.W),
        new Keybind(GameAction.MoveDown, KeyCode.S),
        new Keybind(GameAction.MoveLeft, KeyCode.A),
        new Keybind(GameAction.MoveRight, KeyCode.D),
        new Keybind(GameAction.Hook, KeyCode.Space),
    };
}
