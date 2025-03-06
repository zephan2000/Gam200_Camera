using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ControlGetter
{
    public const string NameOf_ControlLeft = "Left";
    public const string NameOf_ControlRight = "Right";
    public const string NameOf_ControlJump = "Jump";
    public const string NameOf_ControlShrink = "Shrink";
    public const string NameOf_ControlQuickreset = "Quick Reset";

    private static Dictionary<string, KeyCode[]> Presets = new Dictionary<string, KeyCode[]>()
    {
        {
            "Arrow Keys", new KeyCode[]
            {
                KeyCode.LeftArrow,
                KeyCode.RightArrow,
                KeyCode.Z,
                KeyCode.X,
                KeyCode.R,
            }
        },

        {
            "WASD", new KeyCode[]
            {
                KeyCode.A,
                KeyCode.D,
                KeyCode.Space,
                KeyCode.P,
                KeyCode.R,
            }
        },

        {
            "Alwyn", new KeyCode[]
            {
                KeyCode.LeftArrow,
                KeyCode.RightArrow,
                KeyCode.UpArrow,
                KeyCode.Space,
                KeyCode.R,
            }
        },
    };
    public static void CyclePresets()
    {
        string[] eachControl = File.ReadAllText(GetFilePath).Split('\n');
        KeyCode[] arrayofkeycodes = new KeyCode[eachControl.Length];
        for (int i = 0; i < eachControl.Length; ++i)
        {
            string[] controlPair = eachControl[i].Split(':');
            arrayofkeycodes[i] = (KeyCode)int.Parse(controlPair[1]);
        }

        List<KeyValuePair<string, KeyCode[]>> classicOOP = new List<KeyValuePair<string, KeyCode[]>>();
        foreach (KeyValuePair<string, KeyCode[]> pair in Presets)
        {
            classicOOP.Add(pair);
        }
        for (int i = 0; i < classicOOP.Count; ++i)
        {
            KeyValuePair<string, KeyCode[]> pair = classicOOP[i];
            for (int j = 0; j < pair.Value.Length; ++j)
            {
                if (arrayofkeycodes[j] != pair.Value[j]) break;

                if (j == pair.Value.Length - 1)
                {
                    int nextindex = i + 1;
                    if (nextindex >= classicOOP.Count) nextindex = 0;
                    WriteTheControls(classicOOP[nextindex].Value);
                    return;
                }
            }
        }
        
        //If it reaches this point, it's a custom, so start from the default
        WriteTheControls(Presets["Arrow Keys"]);
    }
    public static string GetCurrentPresetName()
    {
        string[] eachControl = File.ReadAllText(GetFilePath).Split('\n');
        KeyCode[] arrayofkeycodes = new KeyCode[eachControl.Length];
        for (int i = 0; i < eachControl.Length; ++i)
        {
            string[] controlPair = eachControl[i].Split(':');
            arrayofkeycodes[i] = (KeyCode)int.Parse(controlPair[1]);
        }

        foreach (KeyValuePair<string, KeyCode[]> pair in Presets)
        {
            for (int i = 0; i < pair.Value.Length; ++i)
            {
                if (arrayofkeycodes[i] != pair.Value[i]) break;

                if (i == pair.Value.Length - 1) return pair.Key;
            }
        }

        return "Custom";
    }
    static string GetFilePath
    {
        get { return Path.Combine(Application.persistentDataPath, "controls.txt"); }
    }
    public static void SwapKey(string keytitle, KeyCode newkeycode)
    {
        string[] eachControl = File.ReadAllText(GetFilePath).Split('\n');
        KeyCode[] arrayofkeycodes = new KeyCode[eachControl.Length];
        for (int i = 0; i < eachControl.Length; ++i)
        {
            string[] controlPair = eachControl[i].Split(':');
            if (controlPair[0] == keytitle)
            {
                arrayofkeycodes[i] = newkeycode;
            }
            else
            {
                arrayofkeycodes[i] = (KeyCode)int.Parse(controlPair[1]);
            }
        }
        WriteTheControls(arrayofkeycodes);
    }
    public static Dictionary<string, KeyCode> GetControls()
    {
        Dictionary<string, KeyCode> keybinds = new Dictionary<string, KeyCode>();

        //If nothing exists, turn 'nothing' into 'something'
        AttemptCreateDefault();

        string[] eachControl = File.ReadAllText(GetFilePath).Split('\n');
        for (int i = 0; i < eachControl.Length; ++i)
        {
            string[] controlPair = eachControl[i].Split(':');
            keybinds.Add(controlPair[0], (KeyCode)int.Parse(controlPair[1]));
        }

        return keybinds;
    }
    private static void WriteTheControls(KeyCode[] values)
    {
        StreamWriter sw = File.CreateText(GetFilePath);
        sw.Write(NameOf_ControlLeft + ":" + (int)values[0]);
        sw.Write("\n");
        sw.Write(NameOf_ControlRight + ":" + (int)values[1]);
        sw.Write("\n");
        sw.Write(NameOf_ControlJump + ":" + (int)values[2]);
        sw.Write("\n");
        sw.Write(NameOf_ControlShrink + ":" + (int)values[3]);
        sw.Write("\n");
        sw.Write(NameOf_ControlQuickreset + ":" + (int)values[4]);
        sw.Close();
    }
    private static void AttemptCreateDefault()
    {
        if (!File.Exists(GetFilePath) || !CheckIfFileMatches())
        {
            WriteTheControls(Presets["Arrow Keys"]);
        }
    }
    private static bool CheckIfFileMatches()
    {
        if (!File.Exists(GetFilePath)) return false;

        int points = 0, pointsthreshold = 5;
        string[] eachControl = File.ReadAllText(GetFilePath).Split('\n');
        for (int i = 0; i < eachControl.Length; ++i)
        {
            string controlKeyname = eachControl[i].Split(':')[0];
            if (controlKeyname == NameOf_ControlJump
                || controlKeyname == NameOf_ControlLeft
                || controlKeyname == NameOf_ControlRight
                || controlKeyname == NameOf_ControlShrink
                || controlKeyname == NameOf_ControlQuickreset
                )
                points++;
        }
        return points >= pointsthreshold;
    }
}
