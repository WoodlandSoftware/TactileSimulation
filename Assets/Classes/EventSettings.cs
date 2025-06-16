using UnityEngine;
using UnityEngine.UI;

public class EventSettings : MonoBehaviour
{
    //public string EventStartDateTime;
    //public string EventEndDateTime;
    public static float speed;
    public float StartSpeedForGUI;
    
    //public Slider SpeedSlider;

    private void Start()
    {
        speed = StartSpeedForGUI;
        //SpeedSlider.value = speed;
    }
    public void ChangeSpeed(float value)
    {
        speed = value;
    }
}
