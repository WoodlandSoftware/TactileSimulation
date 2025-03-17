using UnityEngine;

public class ExitCollidor : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Visitor")
        {
            Visitor v = other.GetComponent<Visitor>();
            if (v.visitorData.events.Count == 0) { Destroy(other.gameObject); return; }
            if (v.visitorData.events[0] is CheckInOutEvent checkEvent)
            {
                if (checkEvent.action == CheckAction.CheckIn)
                {
                    return;
                }
                else if (checkEvent.action == CheckAction.CheckOut)
                {
                    if (v.visitorData.events.Count > 1)
                    {
                        v.visitorData.events.RemoveAt(0);
                        v.DisableVisitor();
                    }
                    else if (v.visitorData.events.Count < 2)
                    {
                        Destroy(other.gameObject);
                    }
                    else
                    {
                        Debug.Log("Something is wrong with the visitor exit");
                    }
                }
            }
        }
        else Debug.Log("Different Tag");
    }
}
