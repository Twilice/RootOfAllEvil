using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour
{
    // todo :: should baseRoot be it's own class?
    public static Root baseRoot;
    public Root parentRoot;
    public List<Root> subRoots;

    // length of longest subroot
    public int Length => throw new NotImplementedException(); 
    // summed length of all subroots
    public int TotalLength => throw new NotImplementedException();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
