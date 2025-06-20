using UnityEngine;
using System.Collections.Generic;

public class ToggleTactile: MonoBehaviour
{
    static public bool TactileActive = true;

    public NavPoints points;

    [SerializeField] GameObject[] tokenObjects;

    private void Start()
    {
        tokenObjects = FindAllWithTagIncludingInactive("Token");
        foreach (GameObject obj in tokenObjects)
        {
            obj.SetActive(false);
        }
    }

    static public GameObject[] FindAllWithTagIncludingInactive(string tag)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        List<GameObject> matching = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag(tag) && obj.hideFlags == HideFlags.None && obj.scene.IsValid())
            {
                matching.Add(obj);
            }
        }

        return matching.ToArray();
    }

    public void ToggleObjects()
    {
        GameObject[] tactileObjects = FindAllWithTagIncludingInactive("Tactile");

        bool tactileActive = !TactileActive;
        bool tokensActive = TactileActive;

        foreach (GameObject obj in tactileObjects)
        {
            obj.SetActive(tactileActive);
        }

        foreach (GameObject obj in tokenObjects)
        {
            obj.SetActive(tokensActive);
        }

        TactileActive = !TactileActive; // toggle for next click

        if (tactileActive) DisableCoinStatus();
    }

    void DisableCoinStatus()
    {
        foreach (Merchant m in points.Merchants)
        {
            if(m.Type == "Coin")
            {
                m.QueueStall.CleanupInvalidQueueVisitors();
            }
        }
    }
}
