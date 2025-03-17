using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CheckInOutEvent : GameEvent
{
    // Instead of a string, we now use an enumerator for the event type.
    // When deserializing, you can convert the incoming string ("enter"/"leave") to the appropriate enum.
    public CheckAction action;
}
