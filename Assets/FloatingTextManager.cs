using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextManager : Singleton<FloatingTextManager>
{
    public GameObject floatingTextObj;


    public void addText(string text, Vector3 pos, Color color)
    {
        var obj = Instantiate(floatingTextObj);
        obj.GetComponent<FloatingText>().init(text, pos, color);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
