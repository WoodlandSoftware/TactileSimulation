using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Merchant
{
    public string Name;
    public string Type;
    public QueueStall QueueStall;
}

public class NavPoints : MonoBehaviour
{
    public GameObject Entrance;
    public List<Merchant> Merchants = new List<Merchant>();
    public List<Transform> WanderPoints;
    public float podiumRadius;

    public int FoodCourts;
    public int Bars;
    public int Coins;

    public int FoodRow;
    public int BarRow;
    public int CoinRow;

    public int FoodServing;
    public int BarServing;
    public int CoinServing;

    private void Start()
    {
        Merchants.Clear();

        // Find all Food points
        GameObject[] foodPoints = GameObject.FindGameObjectsWithTag("FoodStand");
        foreach (GameObject go in foodPoints)
        {
            Merchant m = new Merchant();
            m.Name = go.name;
            m.Type = "Food";
            m.QueueStall = go.GetComponent<QueueStall>();
            Merchants.Add(m);
            FoodCourts++;
        }

        // Find all Drink points
        GameObject[] drinkPoints = GameObject.FindGameObjectsWithTag("DrinkStand");
        foreach (GameObject go in drinkPoints)
        {
            Merchant m = new Merchant();
            m.Name = go.name;
            m.Type = "Bar";
            m.QueueStall = go.GetComponent<QueueStall>();
            Merchants.Add(m);
            Bars++;
        }

        // Find all Coin points
        GameObject[] CoinPoints = ToggleTactile.FindAllWithTagIncludingInactive("CoinStand");
        foreach (GameObject go in CoinPoints)
        {
            Merchant m = new Merchant();
            m.Name = go.name;
            m.Type = "Coin";
            m.QueueStall = go.GetComponent<QueueStall>();
            Merchants.Add(m);
            Coins++;
        }
    }


    private void Update()
    {
        FoodRow = 0;
        BarRow = 0;
        CoinRow = 0;
        FoodServing = 0;
        BarServing = 0;
        CoinServing = 0;
        foreach (Merchant m in Merchants)
        {
            if(m.Type == "Food")FoodRow += m.QueueStall.visitorsQueue.Count;
            if(m.Type == "Bar") BarRow += m.QueueStall.visitorsQueue.Count;
            if(m.Type == "Coin") CoinRow += m.QueueStall.visitorsQueue.Count;
            if (ToggleTactile.TactileActive)
            {
                if (m.Type == "Food") FoodServing += m.QueueStall.servingQueue.Count;
                if (m.Type == "Bar") BarServing += m.QueueStall.servingQueue.Count;
            }
            else if (m.Type == "Coin") CoinServing += m.QueueStall.servingQueue.Count;

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
