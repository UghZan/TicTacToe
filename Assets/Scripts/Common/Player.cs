using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Player
{
    public string name;
    public bool aiControlled;
    public byte figure;

    public Player(string name, bool ai, bool fig)
    {
        this.name = name;
        this.aiControlled = ai;
        //2 - cross, 1 - zero
        this.figure = (byte)(fig ? 2 : 1);
    }
}
