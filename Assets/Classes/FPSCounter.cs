using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text fpsText; // Assign your TMP text in the Inspector

    private int frameCount = 0;
    private float elapsedTime = 0.0f;
    private float updateInterval = 1.0f; // Update FPS display every 1 second

    void Update()
    {
        frameCount++;
        elapsedTime += Time.unscaledDeltaTime;

        if (elapsedTime >= updateInterval)
        {
            float fps = frameCount / elapsedTime;
            fpsText.text = "FPS: " + Mathf.RoundToInt(fps);

            // Reset counters
            frameCount = 0;
            elapsedTime = 0;
        }
    }
}
