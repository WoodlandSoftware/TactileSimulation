using System;
using System.Collections.Generic;
using UnityEngine;

// Define an enumerator for check in or check out actions
public enum CheckAction
{
    CheckIn,   // could map from JSON value "enter"
    CheckOut   // could map from JSON value "leave"
}

[Serializable]
public abstract class GameEvent : IComparable<GameEvent>
{
    public string userId;
    public string timestamp;

    // Sorting based on timestamp
    public int CompareTo(GameEvent other)
    {
        return timestamp.CompareTo(other.timestamp);
    }
}
