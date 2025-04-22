using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueStall : MonoBehaviour
{
    public enum QueueDirection { North, East, South, West }
    public Merchant ThisMerchant;

    [SerializeField] private float minimumQueueTime;
    [SerializeField] private float MaximumQueueTime;

    [SerializeField] private GameObject queuePoint;
    [SerializeField] public float QueueingDistance = 2f;
    [SerializeField] private QueueDirection direction = QueueDirection.North;

    private bool QueueIsRunning = false;


    [SerializeField] private List<GameObject> visitorsQueue = new();
    private void Update()
    {
        if (visitorsQueue.Count > 0 && QueueIsRunning)
        {
            StartCoroutine(QueueRunning()); // this is the fix
        }
    }

    private IEnumerator QueueRunning()
    {
        QueueIsRunning = false;
        yield return new WaitForSeconds(Random.Range(minimumQueueTime/ EventSettings.speed, MaximumQueueTime / EventSettings.speed));
        visitorsQueue[0].GetComponent<Visitor>().ReleaseFromQueue();
        visitorsQueue.RemoveAt(0);
        foreach(GameObject g in visitorsQueue)
        {
            if (g == null) visitorsQueue.Remove(g);
            else
            g.GetComponent<Visitor>().MoveInQueue();
        }
        if (visitorsQueue.Count > 0) QueueIsRunning = true;
    }

    public void AddVisitor(GameObject visitor)
    {
        if (visitorsQueue.Count == 0 && QueueIsRunning == false) QueueIsRunning = true;
        if (!visitorsQueue.Contains(visitor))
        {
            visitorsQueue.Add(visitor);
        }
        
    }

    public Vector3 Queueposition(GameObject visitor)
    {
        if (!visitorsQueue.Contains(visitor))
            {
                //Debug.LogError($"Visitor {visitor.name} not in queue!");
                visitor.GetComponent<Visitor>().ReleaseFromQueue();
                return Vector3.zero;
            }

            int index = visitorsQueue.IndexOf(visitor);
            Vector3 basePosition = queuePoint.transform.position;
            Vector3 offset = Vector3.zero;

            Vector3 RandomBehaviour = new Vector3(0, 0, 0);
            RandomBehaviour.x += Random.Range(-0.3f, 0.3f);
            RandomBehaviour.z += Random.Range(-0.3f, 0.3f);

            switch (direction)
            {
                case QueueDirection.North:
                    offset = Vector3.forward * -QueueingDistance * index;
                    break;
                case QueueDirection.East:
                    offset = Vector3.right * QueueingDistance * index;
                    break;
                case QueueDirection.South:
                    offset = Vector3.forward * QueueingDistance * index;
                    break;
                case QueueDirection.West:
                    offset = Vector3.right * -QueueingDistance * index;
                    break;
            }

            //return basePosition;
            return basePosition + offset + RandomBehaviour;

            /*Semi circle model*
            if (!visitorsQueue.Contains(visitor))
            {
                visitor.GetComponent<Visitor>().ReleaseFromQueue();
                return Vector3.zero;
            }

            int index = visitorsQueue.IndexOf(visitor);
            Vector3 basePos = queuePoint.transform.position;

            // Parameters
            float radiusStep = QueueingDistance;
            int visitorsPerRow = 6; // number of visitors in the first row
            int row = Mathf.FloorToInt(index / (float)visitorsPerRow);
            int indexInRow = index % visitorsPerRow;

            float radius = radiusStep * (1 + row);
            float angleStep = 180f / visitorsPerRow; // degrees
            float angle = -90f + angleStep * indexInRow; // semi-circle from -90 to +90 degrees

            // Convert angle to radians for sin/cos
            float rad = angle * Mathf.Deg2Rad;
            Vector3 localOffset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;

            // Rotate offset based on queue direction
            Quaternion rotation = Quaternion.identity;
            switch (direction)
            {
                case QueueDirection.North: rotation = Quaternion.Euler(0, 0, 0); break;
                case QueueDirection.East: rotation = Quaternion.Euler(0, 90, 0); break;
                case QueueDirection.South: rotation = Quaternion.Euler(0, 180, 0); break;
                case QueueDirection.West: rotation = Quaternion.Euler(0, 270, 0); break;
            }

            Vector3 finalOffset = rotation * localOffset;
            Vector3 jitter = new Vector3(Random.Range(-0.15f, 0.15f), 0, Random.Range(-0.15f, 0.15f));
            return basePos + finalOffset + jitter;
            /**/
        }



    void OnDrawGizmos()
    {
        if (queuePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(queuePoint.transform.position, 0.5f);

            // Draw the queue line
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