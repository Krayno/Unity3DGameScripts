using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    public byte CharacterSlot { get; set; }
    public string Name { get; set; }
    public ushort TotalLevel { get; set; }
    public byte ZoneID { get; set; }
    public long Appearance { get; set; }

    public Character(byte characterSlot, string name, ushort totalLevel, byte zoneID, long appearance)
    {
        CharacterSlot = characterSlot;
        Name = name;
        TotalLevel = totalLevel;
        ZoneID = zoneID;
        Appearance = appearance;
    }
}
