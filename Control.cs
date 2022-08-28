using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control : MonoBehaviour
{
    // Start is called before the first frame update
    public void OnClick()
    {
        GameObject.Find("GameObject").GetComponent<Position>().enabled = true;        //打开脚本
    }


}
