using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Visitor : MonoBehaviour
{
    public string VisitorID;
    [SerializeReference]
    public List<GameEvent> events = new List<GameEvent>();



    public NavMeshAgent agent;
    public NavPoints points;

    private float timer;
    private float currentTime;
    private bool isExiting;

    void Start()
    {
        /**/
        agent = gameObject.GetComponent<NavMeshAgent>();
        timer = 1; //Random.value(1, 10);
        /**/
    }

    // Update is called once per frame
    void Update()
    {
        /**/
        currentTime += Time.deltaTime;
        if (currentTime > timer)
        {
            currentTime = 0;
            int destination = Random.Range(1, 11);
            if (isExiting)
            {
                Object.Destroy(gameObject);
                return;
            }
            if (destination < 7)
            {
                agent.SetDestination(GetRandomPosition(points.PodiumArea.transform.position, points.podiumRadius));
                timer = Random.Range(5, 15);
                print("Podium");
            }
            else if (destination > 7 && destination < 10)
            {
                agent.SetDestination(points.Bar.transform.position);
                timer = Random.Range(5, 10);
                print("Bar");
            }
            else if(destination == 10)
            {
                agent.SetDestination(points.Exit.transform.position);
                timer = 20;
                isExiting = true;
                print("Exit");
            }
        }
        /**/
    }
    Vector3 GetRandomPosition(Vector3 origin, float range)
    {
        Vector3 randomOffset = Random.insideUnitSphere * range;
        return origin + new Vector3(randomOffset.x, 0, randomOffset.z); // Keeps Y constant
    }

}
