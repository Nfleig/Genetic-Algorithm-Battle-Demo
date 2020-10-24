using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AILabel : MonoBehaviour
{
    public AI ai;
    private Text label;
    // Start is called before the first frame update
    void Start()
    {
        label = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        string team = "Blue";
        if(ai.Enemy){
            team = "Orange";
        }
        string data = team + " AI:\n" + ai.getData();
        label.text = data;
    }
}
