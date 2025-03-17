using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class AbstractVisitor
{
    public string userId;
    [SerializeReference]
    public List<GameEvent> events = new List<GameEvent>();
}
