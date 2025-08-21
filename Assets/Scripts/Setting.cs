using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Setting : MonoBehaviour
{
    public AISettings.SettingID ID;
    public string settingName;
    public string inputString;
    public float value;
    public bool boolValue;
    public int numValue;
    public Slider slider;
    public Toggle toggle;
    public InputField input;
    public Dropdown dropdown;

    private Text label;
    public GameObject errorText;
    // Start is called before the first frame update
    void Start()
    {
        label = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(slider){
            value = slider.value;
            label.text = settingName + ":\n" + value;
        }
        else if(toggle){
            boolValue = toggle.isOn;
        }
        else if(input){
            try{
                numValue = Convert.ToInt32(input.text);
                if (numValue < 0)
                {
                    throw new FormatException();
                }
                errorText.SetActive(false);
            }
            catch(FormatException){
                errorText.SetActive(true);
            }
        }
        else if(dropdown){
            numValue = dropdown.value;
        }
    }
}
