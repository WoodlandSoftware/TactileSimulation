using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VisitorSpawner : MonoBehaviour
{
    public GameObject visitorPrefab;
    public GameObject spawnLocation;
    public NavPoints points;

    public EventSettings eventSettings;
    public DataLoader dataLoader;

    public DateTime currentTime;
    private DateTime endTime;
    public TMP_Text DataTimeHUD;
    public TMP_Text VisitorsHUD;
    [SerializeReference]
    public List<AbstractVisitor> pendingVisitors = new();
    public List<GameObject> CurrentVisitors = new();

    void Start()
    {
        currentTime = DateTime.ParseExact(eventSettings.EventStartDateTime, "yyyyMMddHHmmss", null);
        endTime = DateTime.ParseExact(eventSettings.EventEndDateTime, "yyyyMMddHHmmss", null);

        dataLoader.LoadAndOrganizeEvents();
        pendingVisitors.AddRange(dataLoader.visitors);
        pendingVisitors.Sort((a, b) => a.events[0].timestamp.CompareTo(b.events[0].timestamp));

        StartCoroutine(SimulateTime());
    }

    IEnumerator SimulateTime()
    {
        while (currentTime <= endTime)
        {
            while (pendingVisitors.Count > 0 && currentTime.ToString("yyyyMMddHHmmss").CompareTo(pendingVisitors[0].events[0].timestamp) >= 0)
            {
                SpawnVisitor(pendingVisitors[0]);
                pendingVisitors.RemoveAt(0);
            }
            currentTime = currentTime.AddSeconds(Time.deltaTime * EventSettings.speed);
            //debug.Log(currentTime);
            DataTimeHUD.text = "DateTime: " + currentTime;
            VisitorsHUD.text = "Current Visitors: " + CurrentVisitors.Count;

            yield return null;
        }
    }

    /**
    public void AddBackVisitorToPending(AbstractVisitor visitorData)
    {
        pendingVisitors.Add(visitorData);
        pendingVisitors.Sort((a, b) => a.events[0].timestamp.CompareTo(b.events[0].timestamp));
    }
    /**/

    void SpawnVisitor(AbstractVisitor visitorData)
    {
        GameObject visitorInstance = Instantiate(visitorPrefab, spawnLocation.transform.position, spawnLocation.transform.rotation);
        Visitor visitorScript = visitorInstance.GetComponent<Visitor>();
        visitorScript.visitorData = visitorData;
        visitorScript.points = points;
        visitorScript.visitorSpawner = this;
        CurrentVisitors.Add(visitorInstance);
        //Debug.Log($"Spawned visitor {visitorData.userId} at {currentTime}");
    }

}
