using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;


public class NeedsBasedVisitor : MonoBehaviour
{
    public enum VisitorState
    {
        Wandering,
        GoingToFood,
        GoingToBar,
        GoingToEntertainment,
        Entertaining,
        GettingCoins,
        Queueing
    }

    public float hunger;
    public float thirst;
    public float entertainment;
    public int coins;

    private float hungerDecayRate;
    private float thirstDecayRate;
    private float entertainmentDecayRate;

    private NavMeshAgent agent;
    private VisitorState currentState;
    private bool HasEntertainmentGoal;
    private bool lastTactileState;
    private Vector3 lasPosCheck;
    private float queueingTimer;

    public NavPoints points;
    public Merchant currentMerchantGoal;
    private GameObject visitorInFront;

    public string debugCurrentlyDoing;

    [Header("Base Agent Settings")]
    public float baseSpeed = 3.5f;
    public float baseAcceleration = 200f;
    public float baseAngularSpeed = 120f;
    public float baseStoppingDistance = 0.5f;

    void Start()
    {
        queueingTimer = 0;
        agent = GetComponent<NavMeshAgent>();

        // Initialize needs with random values
        hunger = Random.Range(0.5f, 1f);
        thirst = Random.Range(0.5f, 1f);
        entertainment = Random.Range(0.2f, 0.8f);

        // Set different decay rates
        hungerDecayRate = Random.Range(0.02f, 0.05f);
        thirstDecayRate = Random.Range(0.02f, 0.05f);
        entertainmentDecayRate = Random.Range(0.02f, 0.03f); // decays fastest

        //SetAgentSettings
        baseSpeed = agent.speed;
        baseAcceleration = agent.acceleration;
        //baseAngularSpeed = agent.angularSpeed;
        baseStoppingDistance = agent.stoppingDistance;

        lastTactileState = ToggleTactile.TactileActive;
    }

    void UpdateAgentSettings()
    {
        float currentSpeed = EventSettings.speed;
        float speedScale = currentSpeed / baseSpeed;

        agent.speed = currentSpeed;
        agent.acceleration = baseAcceleration * speedScale;
        //agent.angularSpeed = baseAngularSpeed * speedScale;
        agent.stoppingDistance = baseStoppingDistance * Mathf.Clamp(speedScale, 0.5f, 2f);
    }

    void Update()
    {
        UpdateAgentSettings();
        TickNeeds();
        changeTactileState();
        UpdateState();
        HandleState();
        CheckQueueingLength();

        if (visitorInFront == null && currentState == VisitorState.Queueing) MoveInQueue();
    }

    void CheckQueueingLength()
    {
        if (currentState == VisitorState.Queueing)
        {
            queueingTimer += Time.deltaTime;
            if (queueingTimer > 5f)
            {
                currentMerchantGoal.QueueStall.RemoveVisitor(this.gameObject);
                currentMerchantGoal = null;
                currentState = VisitorState.Wandering;
                queueingTimer = 0;
            }


        }
    }

    void changeTactileState()
    {
        if (lastTactileState != ToggleTactile.TactileActive)
        {
            if (ToggleTactile.TactileActive)
            {
                coins = 0;
                if(currentState == VisitorState.Queueing)
                {
                    currentMerchantGoal?.QueueStall?.RemoveVisitor(this.gameObject);
                    currentMerchantGoal = null;
                    currentState = VisitorState.GoingToEntertainment;
                }
            }
            else
            {
                if (currentState == VisitorState.Queueing)
                {
                    currentMerchantGoal?.QueueStall?.RemoveVisitor(this.gameObject);
                    currentMerchantGoal = null;
                    currentState = VisitorState.GettingCoins;
                }
            }
            lastTactileState = ToggleTactile.TactileActive;
        }
    }
    void TickNeeds()
    {
        hunger -= hungerDecayRate * Time.deltaTime *(EventSettings.speed*0.01f);
        thirst -= thirstDecayRate * Time.deltaTime * (EventSettings.speed * 0.01f);
        entertainment -= entertainmentDecayRate * Time.deltaTime * (EventSettings.speed * 0.01f);

        hunger = Mathf.Clamp01(hunger);
        thirst = Mathf.Clamp01(thirst);
        entertainment = Mathf.Clamp01(entertainment);
    }

    void UpdateState()
    {
        if (currentState == VisitorState.Queueing || currentState == VisitorState.GoingToBar || currentState == VisitorState.GoingToFood || currentState == VisitorState.GettingCoins) return;
        if (currentState == VisitorState.Entertaining)
        {
            if (entertainment > 0.85f)
            {
                currentState = VisitorState.Wandering;
                return;
            }
        }
        if (!ToggleTactile.TactileActive && coins < 1 && (hunger < entertainment || thirst < entertainment))
        {
            if (currentState == VisitorState.GoingToFood || currentState == VisitorState.GoingToBar)
            {
                currentMerchantGoal?.QueueStall?.RemoveVisitor(this.gameObject);
                currentMerchantGoal = null;
            }
            currentState = VisitorState.GettingCoins;
            return;
        }

        if (hunger < thirst && hunger < entertainment)
            currentState = VisitorState.GoingToFood;
        else if (thirst < hunger && thirst < entertainment)
            currentState = VisitorState.GoingToBar;
        else if (entertainment < hunger && entertainment < thirst && currentState != VisitorState.Entertaining)
        {
            if (entertainment < 0.85)
                currentState = VisitorState.GoingToEntertainment;
            else
                currentState = VisitorState.Wandering;
        }
        else if (entertainment == 0) currentState = VisitorState.GoingToEntertainment;
    }


    void HandleState()
    {
        debugCurrentlyDoing = currentState.ToString();
        switch (currentState)
        {
            case VisitorState.GoingToFood:
                if (currentMerchantGoal == null || currentMerchantGoal.Type != "Food") MoveTo("Food");
                if (currentMerchantGoal.QueueStall == null) MoveTo("Food");
                else if (Vector3.Distance(currentMerchantGoal.QueueStall.Queueposition(this.gameObject), this.gameObject.transform.position) < 2f)
                {
                    currentMerchantGoal.QueueStall.NotifyArrival(this.gameObject);
                    currentState = VisitorState.Queueing;
                    queueingTimer = 0;
                }
                else
                {
                    MoveInQueue();
                    //agent.SetDestination(currentMerchantGoal.QueueStall.Queueposition(this.gameObject));
                }
                break;
            case VisitorState.GoingToBar:
                if (currentMerchantGoal == null || currentMerchantGoal.Type != "Bar") MoveTo("Bar");
                if (currentMerchantGoal.QueueStall == null) MoveTo("Bar");
                else if (Vector3.Distance(currentMerchantGoal.QueueStall.Queueposition(this.gameObject), this.gameObject.transform.position) < 2f)
                {
                    currentMerchantGoal.QueueStall.NotifyArrival(this.gameObject);
                    currentState = VisitorState.Queueing;
                    queueingTimer = 0;
                }
                else
                {
                    MoveInQueue();
                    //agent.SetDestination(currentMerchantGoal.QueueStall.Queueposition(this.gameObject));
                }
                break;
            case VisitorState.GettingCoins:
                if (currentMerchantGoal == null || currentMerchantGoal.Type != "Coin") MoveTo("Coin");
                if (currentMerchantGoal.QueueStall == null) MoveTo("Coin");
                else if (Vector3.Distance(currentMerchantGoal.QueueStall.Queueposition(this.gameObject), this.gameObject.transform.position) < 2f)
                {
                    currentMerchantGoal.QueueStall.NotifyArrival(this.gameObject);
                    currentState = VisitorState.Queueing;
                    queueingTimer = 0;
                }
                else
                {
                    MoveInQueue();
                    //agent.SetDestination(currentMerchantGoal.QueueStall.Queueposition(this.gameObject));
                }
                break;
            case VisitorState.GoingToEntertainment:
                if (!HasEntertainmentGoal)
                MoveTo("Entertainment");

                // Check if close enough to start being entertained
                if (agent.remainingDistance < 0.5f)
                {
                    currentState = VisitorState.Entertaining;
                    HasEntertainmentGoal = false;
                }
                break;
            case VisitorState.Wandering:
                Wander();
                break;
            case VisitorState.Entertaining:
                entertainment += Time.deltaTime * 0.1f * (EventSettings.speed * 0.01f); // adjust speed as needed
                entertainment = Mathf.Clamp01(entertainment);
                break;

            case VisitorState.Queueing:
                if (visitorInFront != null)
                {
                    MoveInQueue();
                }
                break;
        }
    }

    void MoveTo(string targetTag)
    {
        if (targetTag == "Bar")
        {
            List<Merchant> bars = points.Merchants.FindAll(m => m.Type == "Bar");
            if (bars.Count == 0)
            {
                Debug.Log("WTF"); return;
            }

            currentMerchantGoal = bars[Random.Range(0, bars.Count)];
            currentMerchantGoal.QueueStall.AddVisitor(this.gameObject);
            MoveInQueue();
            //agent.SetDestination(m.QueueStall.queuePoint.transform.position);
            //Debug.Log("Currently going to " + currentMerchantGoal);
            return;
        }

        if (targetTag == "Food")
        {
            List<Merchant> foods = points.Merchants.FindAll(m => m.Type == "Food");
            if (foods.Count == 0) return;

            Merchant m = foods[Random.Range(0, foods.Count)];

            currentMerchantGoal = m;
            currentMerchantGoal.QueueStall.AddVisitor(this.gameObject);
            MoveInQueue();
            //agent.SetDestination(m.QueueStall.queuePoint.transform.position);
            return;
        }
        if (targetTag == "Coin")
        {
            List<Merchant> Coins = points.Merchants.FindAll(m => m.Type == "Coin");
            currentMerchantGoal = Coins.OrderBy(c => c.QueueStall.GetQueueLength()).First();
            currentMerchantGoal.QueueStall.AddVisitor(this.gameObject);
            MoveInQueue();
            //agent.SetDestination(currentMerchantGoal.QueueStall.queuePoint.transform.position);
            return;
        }

        if (targetTag == "Entertainment")
        {
            int entertainmentAreaMask = 1 << NavMesh.GetAreaFromName("EntertainmentZone");

            Vector3 randomDirection = points.WanderPoints[0].position + UnityEngine.Random.insideUnitSphere * 30f;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomDirection, out hit, 40f, entertainmentAreaMask))
            {
                Vector3 newPos = hit.position;
                if (Vector3.Distance(lasPosCheck, newPos) > 0.2f)
                {
                    agent.SetDestination(hit.position);
                    lasPosCheck = newPos;
                }
            }
            HasEntertainmentGoal = true;
        }

    }

    public void MoveInQueue()
    {
        if (currentState == VisitorState.Queueing && currentMerchantGoal.QueueStall != null)
        {
            Vector3 newPos = currentMerchantGoal.QueueStall.Queueposition(gameObject);
            if (Vector3.Distance(lasPosCheck, newPos) > 0.2f)
            {
                GetComponent<NavMeshAgent>().SetDestination(newPos);
                lasPosCheck = newPos;
            }
        }
        else if (currentMerchantGoal != null)
        {
            if (currentMerchantGoal.QueueStall == null) return;
            Vector3 newPos = currentMerchantGoal.QueueStall.Queueposition(gameObject);
            GetComponent<NavMeshAgent>().SetDestination(newPos);
            lasPosCheck = newPos;
        }
        if (currentMerchantGoal != null)
        {
            if (currentMerchantGoal.QueueStall != null)
            {
                Vector3 targetPos = currentMerchantGoal.QueueStall.transform.position;

                // Keep current Y position to avoid vertical tilting
                targetPos.y = transform.position.y;

                transform.LookAt(targetPos);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Visitor"))
        {
            if (currentMerchantGoal == null || currentMerchantGoal.QueueStall == null) return;
            if (currentMerchantGoal.QueueStall.visitorsQueue.Contains(other.gameObject))
            visitorInFront = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == visitorInFront) visitorInFront = null;
    }

    public void ReleaseFromQueue(string Queuetype)
    {
        //Debug.Log("i should move now");
        if (Queuetype == "Food")
        {
            hunger = 1;
            if (!ToggleTactile.TactileActive) coins--;
        }
        if (Queuetype == "Bar")
        {
            thirst = 1;
            if (!ToggleTactile.TactileActive) coins--;
        }
        if (Queuetype == "Coin") coins = 2;
        currentMerchantGoal = null;
        currentState = VisitorState.Wandering;
    }

    void Wander()
    {
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 basePos = points.WanderPoints[Random.Range(1, 2)].transform.position;
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 10f;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                Vector3 newPos = hit.position;
                if (Vector3.Distance(lasPosCheck, newPos) > 0.2f)
                {
                    agent.SetDestination(newPos);
                    lasPosCheck = newPos;
                }
            }
        }
    }


    //Editor only
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, agent.destination);
    }
}
