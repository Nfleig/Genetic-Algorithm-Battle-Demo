using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Setting : MonoBehaviour
{
    public int ID;
    public string name;
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
            label.text = name + ":\n" + value;
        }
        else if(toggle){
            boolValue = toggle.isOn;
        }
        else if(input){
            try{
                numValue = System.Convert.ToInt32(input.text);
                errorText.SetActive(false);
            }
            catch(FormatException e){
                errorText.SetActive(true);
            }
        }
        else if(dropdown){
            numValue = dropdown.value;
        }
    }
}
