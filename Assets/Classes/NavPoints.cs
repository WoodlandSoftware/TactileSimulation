using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Merchant
{
    public string Name;
    public QueueStall QueueStall;
}

public class NavPoints : MonoBehaviour
{
    public GameObject Entrance;
    public List<Merchant> Merchants = new List<Merchant>();
    public List<Transform> WanderPoints;
    public float podiumRadius;

    private void Start()
    {
        foreach(Merchant m in Merchants)
        {
            m.QueueStall.ThisMerchant = m;
        }
    }

    public static Vector3 RandomPointInArea(Vector3 center, float range, int areaMask, int maxAttempts = 30)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;

            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out UnityEngine.AI.NavMeshHit hit, range, areaMask))
                return hit.position;
        }

        //Debug.LogWarning("Failed to find a random point in the specified area after max attempts.");
        return center; // fallback if no valid point found
    }
}
