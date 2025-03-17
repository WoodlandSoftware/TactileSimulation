using UnityEngine;

public class VisitorSpawner : MonoBehaviour
{
    public GameObject visitorPrefab;
    public GameObject spawnLocation;
    public NavPoints points;
    public int VisitorAmount;
    public float Spawninterval;

    private float timepassed;
    private int spawnedObj;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timepassed += Time.deltaTime;
        if (timepassed > Spawninterval && spawnedObj < VisitorAmount)
        {
            GameObject vis = Instantiate(visitorPrefab, spawnLocation.transform.position, spawnLocation.transform.rotation);
            vis.GetComponent<Visitor>().points = points;
            timepassed = 0;
            vis.name = spawnedObj.ToString();
            spawnedObj++;
        }
    }
}
