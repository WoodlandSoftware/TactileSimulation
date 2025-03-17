using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Visitor : MonoBehaviour
{
    public AbstractVisitor visitorData;
    public VisitorSpawner visitorSpawner;
    public NavPoints points;
    public int speedDivider;
    public string CurrentDestination;

    public float baseSpeed = 3.5f;
    public float baseAcceleration = 8f;
    public float baseAngularSpeed = 120f;
    public float baseStoppingDistance = 0.5f;


    private NavMeshAgent navAgent;
    private MeshRenderer meshRenderer;
    private Collider collider;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponent<MeshRenderer>();
        collider = GetComponent<Collider>();
        visitorData.events.RemoveAt(0);
    }

    void Update()
    {
        // Current speed from eventSettings
        float currentSpeed = visitorSpawner.eventSettings.speed;

        // Scaling factor based on current speed
        float speedScale = currentSpeed / baseSpeed;

        // Apply scaled settings
        navAgent.speed = currentSpeed;
        navAgent.acceleration = baseAcceleration * speedScale;
        navAgent.angularSpeed = baseAngularSpeed * speedScale;
        navAgent.stoppingDistance = baseStoppingDistance * Mathf.Clamp(speedScale, 0.5f, 2f);

        if (visitorData.events.Count == 0)
        {
            navAgent.SetDestination(points.Entrance.transform.position);
            return;
        }

        var nextEvent = visitorData.events[0];
        DateTime eventTime = DateTime.ParseExact(nextEvent.timestamp, "yyyyMMddHHmmss", null);
        DateTime currentSimTime = visitorSpawner.currentTime;

        var timeUntilEvent = (eventTime - currentSimTime).TotalMinutes;

        if (timeUntilEvent < 0)
        {
            visitorData.events.RemoveAt(0);
            Wander();
        }
        else if (timeUntilEvent <= 5)
        {
            if (nextEvent is CheckInOutEvent checkEvent)
            {
                if (checkEvent.action == CheckAction.CheckIn)
                {
                    EnableVisitor();
                    navAgent.SetDestination(points.Entrance.transform.position);
                }
                else if (checkEvent.action == CheckAction.CheckOut)
                {
                    navAgent.SetDestination(points.Entrance.transform.position);
                }
                CurrentDestination = "Enter/Exit";
            }
            else if (nextEvent is TransactionEvent transactionEvent)
            {
                MoveToMerchant(transactionEvent.merchantName);
                CurrentDestination = transactionEvent.merchantName ;
            }
        }

        else
        {
            Wander(); 
            CurrentDestination = "Wander";
        }
    }

    void Wander()
    {
        if (!navAgent.hasPath || navAgent.remainingDistance < 0.5f)
        {
            int randomIndex = UnityEngine.Random.Range(0, points.WanderPoints.Count);
            navAgent.SetDestination(points.WanderPoints[randomIndex].position);
        }
    }

    void MoveToMerchant(string merchantName)
    {
        foreach (var merchant in points.Merchants)
        {
            if (merchant.Name == merchantName)
            {
                navAgent.SetDestination(merchant.location.transform.position);
                return;
            }
        }
    }

    void EnableVisitor()
    {
        meshRenderer.enabled = true;
        collider.enabled = true;
    }

    public void DisableVisitor()
    {
        meshRenderer.enabled = false;
        collider.enabled = false;
    }



    void OnDestroy()
    {
        // Clean-up if necessary
    }
}
