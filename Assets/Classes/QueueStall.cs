using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueStall : MonoBehaviour
{
    public enum QueueDirection { North, East, South, West }
    public Merchant ThisMerchant;

    [SerializeField] private float minimumQueueTime;
    [SerializeField] private float maximumQueueTime;

    [SerializeField] public GameObject queuePoint;
    [SerializeField] public float QueueingDistance = 2f;
    [SerializeField] private QueueDirection direction = QueueDirection.North;

    private bool QueueIsRunning = false;

    [SerializeField] public List<GameObject> visitorsQueue = new();
    [SerializeField] public List<GameObject> arrivedVisitors = new();

    private float prepareTimer = 0f;
    private float prepareDuration = 0f;
    private bool preparingVisitor = false;

    [SerializeField] private GameObject currentServingVisitor = null;
    [SerializeField] public  List<GameObject> servingQueue = new List<GameObject>();

    private bool startCounting;
    private float cleanupTimer = 0f;


        public void AddVisitor(GameObject visitor)
    {
        if (!visitorsQueue.Contains(visitor))
        {
            visitorsQueue.Add(visitor);
        }
    }
    public void RemoveVisitor(GameObject visitor)
    {
        if (visitorsQueue.Contains(visitor))
        {
            visitorsQueue.Remove(visitor);
        }
        if (arrivedVisitors.Contains(visitor))
        {
            arrivedVisitors.Remove(visitor);
        }
    }


    private void Update()
    {
        if (servingQueue.Count > 0)
        {
            if (!preparingVisitor)
            {
                StartPreparingVisitor();
            }
            else
            {
                prepareTimer += Time.deltaTime * (EventSettings.speed * 0.5f);

                if (prepareTimer >= prepareDuration)
                {
                    for (int i = 0; i < servingQueue.Count; i++)
                    {
                        ReleaseCurrentVisitor();
                    }
                }
            }
        }

        if (startCounting)
        {
            cleanupTimer += Time.deltaTime;
            if ((ToggleTactile.TactileActive && cleanupTimer >= 10f) || (!ToggleTactile.TactileActive && cleanupTimer >= 30f))
            {
                CleanupHalfQueueVisitors();
                //CleanupInvalidQueueVisitors();
                cleanupTimer = 0f;
                startCounting = false;
            }
        }
    }
    public void CleanupHalfQueueVisitors()
    {
        List<GameObject> toRemove = new();

        if (visitorsQueue.Count == 0) return;
        for (int i = 0; i < visitorsQueue.Count/2; i++)
        {
            var nv = visitorsQueue[i].GetComponent<NeedsBasedVisitor>();
            if (nv == null || nv.debugCurrentlyDoing != "Queueing")
            {
                if (nv != null) nv.ReleaseFromQueue(ThisMerchant.Type);
                toRemove.Add(visitorsQueue[i]);
            }
        }

        foreach (var v in toRemove)
        {
            visitorsQueue.Remove(v);
            arrivedVisitors.Remove(v);
        }

        foreach (GameObject o in visitorsQueue)
        {
            if (o != null)
                o.GetComponent<NeedsBasedVisitor>().MoveInQueue();
        }
    }


    public void CleanupInvalidQueueVisitors()
    {
        List<GameObject> toRemove = new();

        foreach (var v in visitorsQueue)
        {
            var nv = v.GetComponent<NeedsBasedVisitor>();
            if (nv == null || nv.debugCurrentlyDoing != "Queueing")
            {
                if (nv != null) nv.ReleaseFromQueue(ThisMerchant.Type);
                toRemove.Add(v);
            }
        }

        foreach (var v in toRemove)
        {
            visitorsQueue.Remove(v);
            arrivedVisitors.Remove(v);
        }

        foreach (GameObject o in visitorsQueue)
        {
            if (o != null)
                o.GetComponent<NeedsBasedVisitor>().MoveInQueue();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Visitor"))
        {
            if (!servingQueue.Contains(other.gameObject))
            {
                servingQueue.Add(other.gameObject);
            }
        }
    }

    private void StartPreparingVisitor()
    {
        preparingVisitor = true;
        prepareTimer = 0f;
        prepareDuration = Random.Range(minimumQueueTime, maximumQueueTime);
    }

    public int GetQueueLength()
    {
        return visitorsQueue.Count;
    }

    private void ReleaseCurrentVisitor()
    {
        startCounting = true;
        if (servingQueue.Count == 0) return;

        GameObject visitorObj = servingQueue[0];
        NeedsBasedVisitor visitor = visitorObj.GetComponent<NeedsBasedVisitor>();

        if (visitor != null)
        {
            visitor.ReleaseFromQueue(ThisMerchant.Type);
            visitorsQueue.Remove(visitorObj);
            arrivedVisitors.Remove(visitorObj);

            foreach (GameObject o in visitorsQueue)
            {
                if (o != null)
                    o.GetComponent<NeedsBasedVisitor>().MoveInQueue();
            }
        }
        else Debug.Log("something wong here");

        servingQueue.RemoveAt(0); // Remove the visitor that was just served
        preparingVisitor = false;
    }

    public void NotifyArrival(GameObject visitor)
    {
        if (!arrivedVisitors.Contains(visitor))
        {
            arrivedVisitors.Add(visitor);
            QueueIsRunning = true;
        }
    }

    public Vector3 Queueposition(GameObject visitor)
    {
        if (!visitorsQueue.Contains(visitor))
        {
            Debug.LogError($"Visitor {visitor.name} not in queue!");
            visitor.GetComponent<NeedsBasedVisitor>().ReleaseFromQueue(ThisMerchant.Type);
            return Vector3.zero;
        }
        int index;
        if (arrivedVisitors.Contains(visitor)) index = arrivedVisitors.IndexOf(visitor);
        else index = arrivedVisitors.Count;
        Vector3 basePosition = queuePoint.transform.position;
        Vector3 offset = Vector3.zero;
        Vector3 randomJitter = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));

        switch (direction)
        {
            case QueueDirection.North:
                offset = Vector3.back * QueueingDistance * index;
                break;
            case QueueDirection.East:
                offset = Vector3.right * QueueingDistance * index;
                break;
            case QueueDirection.South:
                offset = Vector3.forward * QueueingDistance * index;
                break;
            case QueueDirection.West:
                offset = Vector3.left * QueueingDistance * index;
                break;
        }

        return basePosition + offset + randomJitter;
    }

    //Editor only
    void OnDrawGizmos()
    {
        if (queuePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(queuePoint.transform.position, 0.5f);

            Vector3 basePosition = queuePoint.transform.position;
            Vector3 directionVector = Vector3.zero;

            switch (direction)
            {
                case QueueDirection.North:
                    directionVector = -Vector3.forward;
                    break;
                case QueueDirection.East:
                    directionVector = Vector3.right;
                    break;
                case QueueDirection.South:
                    directionVector = Vector3.forward;
                    break;
                case QueueDirection.West:
                    directionVector = -Vector3.right;
                    break;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                basePosition,
                basePosition + directionVector * QueueingDistance * visitorsQueue.Count
            );
        }
    }
}