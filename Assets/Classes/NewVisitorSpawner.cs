using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NewVisitorSpawner : MonoBehaviour
{
    public GameObject visitorPrefab;
    public GameObject SpawnLocationOne;
    public GameObject SpawnLocationTwo;
    public NavPoints points;

    public EventSettings eventSettings;

    public DateTime currentTime;


    [SerializeField] public List<GameObject> CurrentVisitors = new();

    [Header("Base Agent Settings")]
    public int Spawnamount = 1500;
    public float TimeToSpawn = 5f; // minutes
    private float spawnTimer = 0f;
    private float spawnInterval; // seconds
    private int spawnedCount = 0;


    void Start()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "spawncount.txt");

        try
        {
            string content = System.IO.File.ReadAllText(path);
            if (int.TryParse(content.Trim(), out int result))
            {
                Spawnamount = result;
            }
            else
            {
                Debug.LogWarning("Invalid number in file.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to read spawn file: " + e.Message);
        }




        spawnInterval = (TimeToSpawn * 60f) / Spawnamount;
    }

    // Update is called once per frame
    void Update()
    {
        // Advance simulation time
        //currentTime = currentTime.AddSeconds(Time.deltaTime * EventSettings.speed);

        // Spawn logic
        if (spawnedCount < Spawnamount)
        {
            spawnTimer += Time.deltaTime * EventSettings.speed;

            // spawn as many as needed to catch up
            while (spawnTimer >= spawnInterval && spawnedCount < Spawnamount)
            {
                SpawnVisitor();
                spawnedCount++;
                spawnTimer -= spawnInterval; // subtract, not reset!
            }
        }
    }

    void SpawnVisitor()
    {
        int i = UnityEngine.Random.Range(1, 10);
        Vector3 spawnpos;
        if (i <= 2) spawnpos = SpawnLocationTwo.transform.position;
        else spawnpos = SpawnLocationOne.transform.position;
        GameObject visitorInstance = Instantiate(visitorPrefab, spawnpos, SpawnLocationOne.transform.rotation);
        NeedsBasedVisitor visitorScript = visitorInstance.GetComponent<NeedsBasedVisitor>();
        visitorScript.points = points;
        CurrentVisitors.Add(visitorInstance);
    }
}
