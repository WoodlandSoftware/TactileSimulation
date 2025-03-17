using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TransactionEvent : GameEvent
{
    public string merchantName;
    public int NumberOfProducts;
}

