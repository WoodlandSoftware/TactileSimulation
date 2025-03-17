using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Merchant
{
    public string Name;
    public GameObject location;
}

public class NavPoints : MonoBehaviour
{
    public GameObject Entrance;
    public List<Merchant> Merchants = new List<Merchant>();
    public List<Transform> WanderPoints;
    public float podiumRadius;
}
