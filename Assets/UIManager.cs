using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public NewVisitorSpawner visitorSpawner;
    public ToggleTactile TactileToggle;
    public NavPoints points;

    public float Wachttijd;
    private int timeCount;
    public List<int> OmzetPerSec = new List<int>();
    public int omzet;
    public float omzetGem;
    public float normalizedOmzet;
    public float speed;

    [Header("Hud Elements")]
    public GameObject CoinsImage;
    public GameObject CashlessImage;
    public TMP_Text CurrentStateHUD;
    public TMP_Text VisitorsHUD;
    public GameObject TactileText;
    public GameObject Coinstext;
    public Image SliderBackground;

    public Color GreenColor;
    public Color GreyColor;

    public List<Image> WachttijdCircles = new List<Image>();
    public List<Image> GelukCircles = new List<Image>();
    public List<Image> OmzetCircles = new List<Image>();

    public TMP_Text TurnOnTextHUD;
    public Slider Toggle;

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timeCount++;
        if (timeCount == 61) timeCount = 1;
        if (ToggleTactile.TactileActive)
        {
            if(CoinsImage.gameObject.activeInHierarchy) CoinsImage.SetActive(false);
            if (!CashlessImage.gameObject.activeInHierarchy) CashlessImage.SetActive(true);
            CurrentStateHUD.text = "Cashless";
            if (!TactileText.gameObject.activeInHierarchy) TactileText.SetActive(true);
            if (Coinstext.gameObject.activeInHierarchy) Coinstext.SetActive(false);
        }
        else
        {
            if (!CoinsImage.gameObject.activeInHierarchy) CoinsImage.SetActive(true);
            if (CashlessImage.gameObject.activeInHierarchy) CashlessImage.SetActive(false);
            CurrentStateHUD.text = "Munten";
            if (!TactileText.gameObject.activeInHierarchy) TactileText.SetActive(false);
            if (Coinstext.gameObject.activeInHierarchy) Coinstext.SetActive(true);
        }
        VisitorsHUD.text = visitorSpawner.CurrentVisitors.Count.ToString();

        //CalculateWaitTime
        if (visitorSpawner.CurrentVisitors.Count > 0)
        {
            float rawWait = (float)(points.BarRow + points.FoodRow + points.CoinRow) / Mathf.Max(visitorSpawner.CurrentVisitors.Count, 1);
            Wachttijd = Mathf.Clamp01(rawWait / 1.0f);
        }
        if (ToggleTactile.TactileActive) Wachttijd *= 1;
        else Wachttijd *= 0.8f;
        VeranderWachttijdSlider(Wachttijd);


        //calculateGeluk
        if (timeCount == 20 || timeCount == 40 || timeCount == 60)
        {
            float geluk = ToggleTactile.TactileActive ? 0.6f : 0.4f;

            if (Wachttijd > 0.6f) geluk -= 0.1f;
            else if (Wachttijd > 0.3f) geluk += 0.1f;
            else geluk += 0.2f;

            geluk += UnityEngine.Random.Range(0.05f, 0.15f);

            geluk = Mathf.Clamp01(geluk);

            VeranderGelukSlider(geluk);
        }

        //calculateOmzet
        if (timeCount == 1)
        {
            omzet = points.CoinServing + points.FoodServing + points.BarServing;
            OmzetPerSec.Add(omzet);
        }

        if (OmzetPerSec.Count > 20)
            OmzetPerSec.RemoveAt(0);

        // Calculate average omzet
        omzetGem = 0;
        foreach (int i in OmzetPerSec)
            omzetGem += i;

        omzetGem /= Mathf.Max(OmzetPerSec.Count, 1);

        // Scale based on visitor count
        float expectedMaxOmzet = Mathf.Max(visitorSpawner.CurrentVisitors.Count * 0.15f, 1f);
        normalizedOmzet = Mathf.Clamp01((omzetGem / expectedMaxOmzet)*5);
        if (!ToggleTactile.TactileActive) normalizedOmzet *= 1.5f;

        VeranderOmzetSlider(normalizedOmzet);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ChangeTactileState();
        }
    }

    public void ChangeTactileState()
    {
        TactileToggle.ToggleObjects();
        if (ToggleTactile.TactileActive)
        {
            Toggle.value = 1;
            TurnOnTextHUD.text = "Zet cashless uit";
            SliderBackground.color = GreenColor;
        }
        else
        {
            Toggle.value = 0;
            TurnOnTextHUD.text = "Zet cashless aan";
            SliderBackground.color = GreyColor;
        }
    }

    void VeranderWachttijdSlider(float Newvalue)
    {
        foreach (Image img in WachttijdCircles)
        {
            img.fillAmount = Mathf.MoveTowards(img.fillAmount, Newvalue, speed * Time.deltaTime);
        }
        if (Newvalue > 0.75f)
        {
            if (WachttijdCircles[0].gameObject.activeInHierarchy) WachttijdCircles[0].gameObject.SetActive(false);
            if (WachttijdCircles[1].gameObject.activeInHierarchy) WachttijdCircles[1].gameObject.SetActive(false);
            if (!WachttijdCircles[2].gameObject.activeInHierarchy) WachttijdCircles[2].gameObject.SetActive(true);
        }
        else if (Newvalue > 0.5f)
        {
            if (WachttijdCircles[0].gameObject.activeInHierarchy) WachttijdCircles[0].gameObject.SetActive(false);
            if (!WachttijdCircles[1].gameObject.activeInHierarchy) WachttijdCircles[1].gameObject.SetActive(true);
            if (WachttijdCircles[2].gameObject.activeInHierarchy) WachttijdCircles[2].gameObject.SetActive(false);
        }
        else
        {
            if (!WachttijdCircles[0].gameObject.activeInHierarchy) WachttijdCircles[0].gameObject.SetActive(true);
            if (WachttijdCircles[1].gameObject.activeInHierarchy) WachttijdCircles[1].gameObject.SetActive(false);
            if (WachttijdCircles[2].gameObject.activeInHierarchy) WachttijdCircles[2].gameObject.SetActive(false);
        }
    }

    void VeranderGelukSlider(float Newvalue)
    {
        foreach (Image img in GelukCircles)
        {
            img.fillAmount = Mathf.MoveTowards(img.fillAmount, Newvalue, speed * Time.deltaTime);
        }
        if (Newvalue > 0.5f)
        {
            if (!GelukCircles[0].gameObject.activeInHierarchy) GelukCircles[0].gameObject.SetActive(true);
            if (GelukCircles[1].gameObject.activeInHierarchy) GelukCircles[1].gameObject.SetActive(false);
            if (GelukCircles[2].gameObject.activeInHierarchy) GelukCircles[2].gameObject.SetActive(false);
        }
        else if (Newvalue > 0.25f)
        {
            if (GelukCircles[0].gameObject.activeInHierarchy) GelukCircles[0].gameObject.SetActive(false);
            if (!GelukCircles[1].gameObject.activeInHierarchy) GelukCircles[1].gameObject.SetActive(true);
            if (GelukCircles[2].gameObject.activeInHierarchy) GelukCircles[2].gameObject.SetActive(false);
        }
        else
        {
            if (GelukCircles[0].gameObject.activeInHierarchy) GelukCircles[0].gameObject.SetActive(false);
            if (GelukCircles[1].gameObject.activeInHierarchy) GelukCircles[1].gameObject.SetActive(false);
            if (!GelukCircles[2].gameObject.activeInHierarchy) GelukCircles[2].gameObject.SetActive(true);
        }
    }

    void VeranderOmzetSlider(float Newvalue)
    {
        foreach (Image img in OmzetCircles)
        {
            img.fillAmount = Mathf.MoveTowards(img.fillAmount, Newvalue, speed * Time.deltaTime);
        }
        if (Newvalue > 0.66f)
        {
            if (!OmzetCircles[0].gameObject.activeInHierarchy) OmzetCircles[0].gameObject.SetActive(true);
            if (OmzetCircles[1].gameObject.activeInHierarchy) OmzetCircles[1].gameObject.SetActive(false);
            if (OmzetCircles[2].gameObject.activeInHierarchy) OmzetCircles[2].gameObject.SetActive(false);
        }
        else if (Newvalue > 0.33f)
        {
            if (OmzetCircles[0].gameObject.activeInHierarchy) OmzetCircles[0].gameObject.SetActive(false);
            if (!OmzetCircles[1].gameObject.activeInHierarchy) OmzetCircles[1].gameObject.SetActive(true);
            if (OmzetCircles[2].gameObject.activeInHierarchy) OmzetCircles[2].gameObject.SetActive(false);
        }
        else
        {
            if (OmzetCircles[0].gameObject.activeInHierarchy) OmzetCircles[0].gameObject.SetActive(false);
            if (OmzetCircles[1].gameObject.activeInHierarchy) OmzetCircles[1].gameObject.SetActive(false);
            if (!OmzetCircles[2].gameObject.activeInHierarchy) OmzetCircles[2].gameObject.SetActive(true);
        }
    }
}
