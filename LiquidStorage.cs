using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidStorage : MonoBehaviour
{
    public Dictionary<Vector3, LiquidSystem> storage; 
    private void Awake()
    {
        storage = new Dictionary<Vector3, LiquidSystem>();
    } 
}
