using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hints : MonoBehaviour
{
    public GameObject circle;
    public Button button1;
    public Text my_text;
    public Text circle_text;
    // Start is called before the first frame update
    void Start()
    {
        my_text.text = "Hints will be here";
        circle.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
    }

    // Update is called once per frame
    public void onClick()
    {
        my_text.text = "You are in controlling now";
        circle.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        circle_text.text = "On";
    }
}
