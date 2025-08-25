using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Setting : MonoBehaviour
{
    public AISettingsManager.SettingID ID;
    public string settingName;
    public string inputString;
    public float floatValue;
    public bool boolValue;
    public int numValue;
    public Slider slider;
    public Toggle toggle;
    public InputField input;
    public Dropdown dropdown;

    private Text label;
    private AISettingsManager settingsManager;
    public GameObject errorText;
    // Start is called before the first frame update
    void Start()
    {
        label = GetComponent<Text>();
        settingsManager = GameObject.FindWithTag("SettingsManager").GetComponent<AISettingsManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (slider)
        {
            floatValue = slider.value;
            label.text = settingName + ":\n" + floatValue;
        }
        else if (toggle)
        {
            boolValue = toggle.isOn;
        }
        else if (input)
        {
            try
            {
                numValue = Convert.ToInt32(input.text);
                errorText.SetActive(false);
            }
            catch (FormatException)
            {
                errorText.SetActive(true);
            }
        }
        else if (dropdown)
        {
            numValue = dropdown.value;
        }
    }

    public void SetValue(string value)
    {
        try
        {
            numValue = Convert.ToInt32(input.text);
            errorText.SetActive(false);
        }
        catch (FormatException)
        {
            errorText.SetActive(true);
        }
        settingsManager.UpdateSetting(this);
    }

    public void SetValue(float floatValue)
    {
        this.floatValue = floatValue;
        settingsManager.UpdateSetting(this);
    }

    public void SetValue(int numValue)
    {
        this.numValue = numValue;
        settingsManager.UpdateSetting(this);
    }

    public void SetValueWithoutNotify(float floatValue)
    {
        this.floatValue = floatValue;
        if (slider)
        {
            slider.SetValueWithoutNotify((float)floatValue);
        }
    }

    public void SetValueWithoutNotify(int numValue)
    {
        this.numValue = numValue;
        if (input)
        {
            input.SetTextWithoutNotify("" + numValue);
        }
        else if (dropdown)
        {
            dropdown.SetValueWithoutNotify(numValue);
        }
    }
}
