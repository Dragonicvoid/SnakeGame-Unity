using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SaveState
{
    public List<int>? WonStat;
    public long? TimeLastTutorial;
    public int? LastDiffSelect;
    public int? AudioVolume;

    //constructor when the save are created
    public SaveState()
    {
        IEnumerable<DIFFICULTY> enumVal = Enum.GetValues(typeof(DIFFICULTY)).Cast<DIFFICULTY>();
        WonStat = new List<int>();

        int highestVal = 0;

        foreach (DIFFICULTY val in enumVal)
        {
            if ((int)val + 1 > highestVal)
            {
                highestVal = (int)val + 1;
            }
        }
        int remaining = Mathf.Max(highestVal - WonStat.Count, 0);

        for (int i = 0; i < remaining; i++)
        {
            WonStat.Add(0);
        }

        TimeLastTutorial = 0;
        LastDiffSelect = (int)DIFFICULTY.MEDIUM;
        AudioVolume = 2;
    }
}