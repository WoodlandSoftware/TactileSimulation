using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Visitor : MonoBehaviour
{
    public enum VisitorState
    {
        GoingToEntrance,
        GoingToMerchant,
        Queueing,
        Wandering
    }

    public AbstractVisitor visitorData;
    public VisitorSpawner visitorSpawner;
    public NavPoints points;
    public int speedDivider;
    public string CurrentDestination;

    public float baseSpeed = 3.5f;
    public float baseAcceleration = 200f;
    public float baseAngularSpeed = 120f;
    public float baseStoppingDistance = 1f;
    public GameObject tag;

    private NavMeshAgent navAgent;
    private MeshRenderer meshRenderer;
    private Collider collider;

    private VisitorState currentState = VisitorState.Wandering;

    void Start()
    {
        if (!ToggleTactile.TactileActive) tag.SetActive(false);
        navAgent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponent<MeshRenderer>();
        collider = GetComponent<Collider>();
        visitorData.events.RemoveAt(0);
        baseSpeed = navAgent.speed;
        baseAcceleration = navAgent.acceleration;
        baseAngularSpeed = navAgent.angularSpeed;
        baseStoppingDistance = navAgent.stoppingDistance;

        SendToMiddle();
    }

    void Update()
    {
        float currentSpeed = EventSettings.speed;
        float speedScale = currentSpeed / baseSpeed;

        navAgent.speed = currentSpeed;
        navAgent.acceleration = baseAcceleration * speedScale;
        navAgent.angularSpeed = baseAngularSpeed * speedScale;
        navAgent.stoppingDistance = baseStoppingDistance * Mathf.Clamp(speedScale, 0.5f, 2f);

        if (visitorData.events.Count == 0)
        {
            navAgent.SetDestination(points.Entrance.transform.position);
            visitorSpawner.CurrentVisitors.Remove(gameObject);
            Destroy(gameObject);
            return;
        }

        var nextEvent = visitorData.events[0];
        DateTime eventTime = DateTime.ParseExact(nextEvent.timestamp, "yyyyMMddHHmmss", null);
        DateTime currentSimTime = visitorSpawner.currentTime;

        var timeUntilEvent = (eventTime - currentSimTime).TotalMinutes;

        if (currentState == VisitorState.Queueing) ;//do nothing
        else if (timeUntilEvent < 0)
        {
            visitorData.events.RemoveAt(0);
            SetState(VisitorState.Wandering);
        }
        else if (timeUntilEvent <= 5)
        {
            if (nextEvent is CheckInOutEvent)
            {
                SetState(VisitorState.GoingToEntrance);
            }
            else if (nextEvent is TransactionEvent)
            {
                SetState(VisitorState.GoingToMerchant);
            }
        }
        else
        {
            SetState(VisitorState.Wandering);
            CurrentDestination = "Wander";
        }

        // Execute behavior based on current state
        switch (currentState)
        {
            case VisitorState.GoingToEntrance:
                navAgent.SetDestination(points.Entrance.transform.position);
                EnableVisitor();
                CurrentDestination = "Enter/Exit";
                break;

            case VisitorState.GoingToMerchant:
                CurrentDestination = ((TransactionEvent)nextEvent).merchantName;
                MoveToMerchant(((TransactionEvent)nextEvent).merchantName);
                break;

            case VisitorState.Queueing:
                // Placeholder for future queueing behavior
                // this is implementen in Queuesteall scripts so nothing will be done here
                break;

            case VisitorState.Wandering:
                Wander();
                break;
        }
    }

    void SetState(VisitorState newState)
    {
        currentState = newState;
    }

    public void ReleaseFromQueue()
    {
        SendToMiddle();
        SetState(VisitorState.Wandering);
    }

    void SendToMiddle()
    {
        //startgoal
        Vector3 Direction = points.WanderPoints[0].transform.position + UnityEngine.Random.insideUnitSphere * 25f;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(Direction, out hit, 15f, NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
        }
    }

    void Wander()
    {
        if (!navAgent.hasPath || navAgent.remainingDistance < navAgent.stoppingDistance)
        {
            int areaMask = 1 << NavMesh.GetAreaFromName("RoamingArea"); // Replace with your area name
            Vector3 randomDirection = transform.position + UnityEngine.Random.insideUnitSphere * 10f;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomDirection, out hit, 15f, areaMask))
            {
                navAgent.SetDestination(hit.position);
            }
            else if (NavMesh.SamplePosition(randomDirection, out hit, 50f, NavMesh.AllAreas))
            {
                navAgent.SetDestination(hit.position);
            }
        }
    }

    public void MoveToMerchant(string merchantName)
    {
        foreach (var merchant in points.Merchants)
        {
            if (merchant.Name == merchantName)
            {
                merchant.QueueStall.AddVisitor(this.gameObject);
                navAgent.SetDestination(merchant.QueueStall.Queueposition(this.gameObject));
                currentState = VisitorState.Queueing;
                return;
            }
        }
    }

    public void MoveInQueue()
    {
        var nextEvent = visitorData.events[0];
        if (nextEvent is CheckInOutEvent)
        {
            Debug.Log("wtf");
            return;
        }
        string Mname = ((TransactionEvent)nextEvent).merchantName;
        foreach (var merchant in points.Merchants)
        {
            if (merchant.Name == Mname)
            {
                navAgent.SetDestination(merchant.QueueStall.Queueposition(this.gameObject));
                return;
            }
        }
    }

    void EnableVisitor()
    {
        //meshRenderer.enabled = true;
        collider.enabled = true;
    }

    public void DisableVisitor()
    {
        visitorSpawner.CurrentVisitors.Remove(gameObject);
        //meshRenderer.enabled = false;
        collider.enabled = false;
    }
}