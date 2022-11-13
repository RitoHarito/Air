using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LiquidSystem : MonoBehaviour
{
    public enum LiquidType { Production, Pipe, Consumption }//生産、伝達、消費
    public LiquidType liquidType;
    public float maxCapacity;
    public float currentCapacity;
    private float timeElapsed;
    [SerializeField] private float timeOut;
    private LiquidStorage liquidStorage;
    public Vector3 currentPosition;
    public float targetCapacity;
    [HideInInspector] private List<LiquidSystem> LiquidSystemGroupeList;//生成ノードのみ使用
    private Dictionary<Vector3, bool> LiquidSystemDictionary;
    private LiquidType _liquidType;
    public Transform liquidObject;
    private void Start()
    {
        liquidStorage = transform.parent.GetComponent<LiquidStorage>();
        liquidStorage.storage.Add(transform.position, this);
        currentPosition = transform.position;
        _liquidType = LiquidType.Pipe;
    }
    private void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed >= timeOut)
        {
            TypeChenged();
            Action value = liquidType switch
            {
                LiquidType.Production => LiquidProduction,
                LiquidType.Pipe => LiquidPipe,
                LiquidType.Consumption => LiquidConsumption,
                _ => null,
            };
            if (value != null) { value.Invoke(); }
            LiquidView();
            timeElapsed = 0.0f;
        }
    }
    private IEnumerator Grouping()
    {
        LiquidSystemDictionary = new Dictionary<Vector3, bool>();
        Exist4Direction(currentPosition, out LiquidSystemGroupeList);
        LiquidSystemDictionary.Add(currentPosition, this);
        yield return null;
        for (int ii = 0; ii < LiquidSystemGroupeList.Count; ii++)
        {
            Exist4Direction(LiquidSystemGroupeList[ii].currentPosition, out List<LiquidSystem> resultLiquidSystems);
            foreach (var resultLiquidSystem in resultLiquidSystems)
            {
                if (!LiquidSystemDictionary.ContainsKey(resultLiquidSystem.currentPosition))
                {
                    LiquidSystemDictionary.Add(resultLiquidSystem.currentPosition, true);
                    LiquidSystemGroupeList.Add(resultLiquidSystem);
                }
            }
        }
    }
    public void Exist4Direction(Vector3 position, out List<LiquidSystem> resultLiquidSystems)
    {
        resultLiquidSystems = new List<LiquidSystem>();
        var liquidSystem = new LiquidSystem();
        if (CheckStorage(position + new Vector3(0, 0, 0), ref liquidSystem)) { resultLiquidSystems.Add(liquidSystem); }
        if (CheckStorage(position + new Vector3(1, 0, 0), ref liquidSystem)) { resultLiquidSystems.Add(liquidSystem); }
        if (CheckStorage(position + new Vector3(0, 0, 1), ref liquidSystem)) { resultLiquidSystems.Add(liquidSystem); }
        if (CheckStorage(position + new Vector3(0, 0, -1), ref liquidSystem)) { resultLiquidSystems.Add(liquidSystem); }
        if (CheckStorage(position + new Vector3(-1, 0, 0), ref liquidSystem)) { resultLiquidSystems.Add(liquidSystem); }
    }
    private bool CheckStorage(Vector3 position, ref LiquidSystem outLiquidSystem)
    {
        if (liquidStorage.storage.TryGetValue(position, out outLiquidSystem)) { return true; }
        else { outLiquidSystem = null; return false; }
    }
    private void TypeChenged()
    {
        if (_liquidType != liquidType)
        {
            _liquidType = liquidType;
            if (liquidType == LiquidType.Production ||
                 liquidType == LiquidType.Consumption)
            {
                StartCoroutine(Grouping());
            }
        }
    }
    private void LiquidProduction()
    {
        if (currentCapacity == maxCapacity) { return; }
        if (currentCapacity < maxCapacity)
        {
            currentCapacity++;
        }
        var capacity = 0f;
        foreach (var LiquidSystem in LiquidSystemGroupeList)
        {
            capacity += LiquidSystem.currentCapacity;
        }
        foreach (var LiquidSystem in LiquidSystemGroupeList)
        {
            LiquidSystem.currentCapacity = capacity / LiquidSystemGroupeList.Count;
            if (LiquidSystem.currentCapacity > maxCapacity)
            {
                LiquidSystem.currentCapacity = maxCapacity;
            }
        }
    }
    private void LiquidPipe()
    {
    }
    private void LiquidConsumption()
    {
        if (currentCapacity == 0) { return; }
        if (0 < currentCapacity)
        {
            currentCapacity--;
        }
        var capacity = 0f;
        foreach (var LiquidSystem in LiquidSystemGroupeList)
        {
            capacity += LiquidSystem.currentCapacity;
        }
        foreach (var LiquidSystem in LiquidSystemGroupeList)
        {
            LiquidSystem.currentCapacity = capacity / LiquidSystemGroupeList.Count;
            if (0 > LiquidSystem.currentCapacity)
            {
                LiquidSystem.currentCapacity = 0;
            }
        }
    }
    private void LiquidView()
    {
        liquidObject.localScale = new Vector3(1, currentCapacity / maxCapacity, 1);
    }
    private void OnDrawGizmos()
    {
        //if (liquidType == LiquidType.Production &&
        //    LiquidSystemGroupeList != null)
        //{
        //    foreach (var liquidSystem in LiquidSystemGroupeList)
        //    {
        //        Gizmos.DrawSphere(liquidSystem.currentPosition + Vector3.up, 0.1f);
        //    }
        //}
    }
}