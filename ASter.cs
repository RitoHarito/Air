using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ASter : MonoBehaviour
{
    [SerializeField] private GUIStyle gUIStyle;
    [Header("PointSetting")]
    public Transform startPoint;
    public Transform endPoint;
    private Vector2Int startGrid;
    private Vector2Int endGrid;
    [Header("Step")]
    [Range(1, 1000)] public int step;
    [Range(1, 1000)] public int step2;
    [Header("Gizmo")]
    [SerializeField] private float gizmoSize;
    private Grid nextGrid;
    private Vector2Int nextPath;
    private List<Grid> openGridList = new List<Grid>();
    private List<Grid> resultGridList = new List<Grid>();
    private List<Vector2Int> pathGridList = new List<Vector2Int>();
    private Dictionary<Vector2Int, Vector2Int> pathGridDictionary = new Dictionary<Vector2Int, Vector2Int>();
    [SerializeField] private float rayHeight;
    public bool finded;
    public bool complete; 
    [SerializeField] private bool viewPath;
    SimpleControls inputActions;
    public void Awake()
    {
        inputActions = new SimpleControls();
        inputActions.Default.Fire.performed += ctx =>
        {
            Initialize();
        };
    }
    private void Start()
    {
        Initialize();
    }
    public void OnEnable() { inputActions.Enable(); }
    public void OnDisable() { inputActions.Disable(); }
    private void Update()
    {
        if (!complete)
        {
            Search();
            PathFind();
        }
    }
    private void OnDrawGizmos()
    {
        if (viewPath)
        {
            Gizmos.DrawSphere(startPoint.position, gizmoSize);
            Gizmos.DrawSphere(endPoint.position, gizmoSize);
            for (int i = 0; i < pathGridList.Count; i++)
            {
                Gizmos.DrawSphere(V2IToV3(pathGridList[i]), gizmoSize);
            }
            for (int i = 0; i < resultGridList.Count; i++)
            {
                Gizmos.DrawWireSphere(V2IToV3(resultGridList[i].position), gizmoSize / 2);
            }
        }
    }
    private void Initialize()
    {
        step = 0;
        step2 = 0;
        finded = false;
        complete = false;
        nextGrid = new Grid();
        nextPath = new Vector2Int();
        openGridList = new List<Grid>();
        resultGridList = new List<Grid>();
        pathGridList = new List<Vector2Int>();
        pathGridDictionary = new Dictionary<Vector2Int, Vector2Int>();
        startGrid = V3ToV2I(startPoint.position);
        endGrid = V3ToV2I(endPoint.position);
    }
    private void Search()
    {
        if (finded) { return; }
        finded = nextGrid.position == endGrid;
        var previousGrid = nextGrid.position;
        if (step == 0) { nextGrid.position = startGrid; Check8Direction(nextGrid); }
        else { Check8Direction(nextGrid); }
        if (CheckNextGrid(openGridList, out nextGrid))
        {
            if (!resultGridList.Exists(x => x.position == nextGrid.position)) { resultGridList.Add(nextGrid); }
            else { nextGrid.parent = nextGrid.position; }
            pathGridDictionary.Add(nextGrid.position, nextGrid.parent);
        }
        step++;
    }
    private void Check8Direction(Grid thisGrid)
    {
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.up });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.up + Vector2Int.right });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.right });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.down + Vector2Int.right });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.down });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.down + Vector2Int.left });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.left });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.left + Vector2Int.up });
    }
    private void AddOpenGridList(Grid thisGrid)
    {
        var cost = CheckWall(V2IToV3(thisGrid.position)) ? -1 : 0;
        thisGrid.cost = cost;
        if (AllowGrid(thisGrid)) { openGridList.Add(thisGrid); }
    }
    private bool CheckNextGrid(List<Grid> grids, out Grid result)
    {
        var gridList = new List<Grid>();
        var costList = new List<float>();
        for (int i = 0; i < grids.Count; i++)
        {
            var thisGrid = grids[i];
            if (!resultGridList.Exists(x => x.position == thisGrid.position))
            {
                var cost = CalcCost(thisGrid);
                thisGrid.cost = cost;
                gridList.Add(thisGrid);
                costList.Add(cost);
            }
        }
        var lowestCost = Mathf.Min(costList.ToArray());
        result = gridList.Find(x => x.cost == lowestCost);
        return result != null;
    }
    private void PathFind()
    {
        if (!finded) { return; }
        if (step2 == 0) { nextPath = endGrid; }
        else
        {
            if (pathGridDictionary.ContainsKey(nextPath))
            {
                nextPath = pathGridDictionary[nextPath];
            }
        }
        pathGridList.Add(nextPath);
        step2++;
        complete = nextPath == startGrid;
    }
    private void SetCost(Grid thisGrid, int set)
    {
        thisGrid.cost = set;
    }
    private bool CheckWall(Vector3 thisWorldGrid)
    {
        var rayStart = thisWorldGrid + Vector3.up * rayHeight;
        var rayEnd = thisWorldGrid + Vector3.down;
        return Physics.Linecast(rayStart, rayEnd);
    }
    private bool IsWall(Grid thisGrid)
    {
        return thisGrid.cost == -1;
    }
    private bool IsClose(Grid thisGrid)
    {
        return thisGrid.cost != -1 && thisGrid.cost != 0;
    }
    private bool AllowGrid(Grid thisGrid)
    {
        return !IsWall(thisGrid) && !IsClose(thisGrid);
    }
    private float CalcCost(Grid thisGrid)
    {
        if (AllowGrid(thisGrid)) { thisGrid.cost = CostF(thisGrid.position); }
        return thisGrid.cost;
    }
    private float CostF(Vector2Int input)
    {
        float fromStartDistance = Vector2Int.Distance(input, startGrid);
        float toEndDistance = Vector2Int.Distance(input, endGrid);
        return fromStartDistance + toEndDistance;
    }
    private int Cost(Vector2Int input)
    {
        var _xS = input.x - startPoint.position.x; var _yS = input.y - startPoint.position.y;
        int fromStartDistance = (int)(_xS * _xS + _yS * _yS);
        var _xE = input.x - endPoint.position.x; var _yE = input.y - endPoint.position.y;
        int toEndDistance = (int)(_xE * _xE + _yE * _yE);
        return fromStartDistance + toEndDistance;
    }
    private Vector3 V2IToV3(Vector2Int input) { return new Vector3(input.x, 0, input.y); }
    private Vector2Int V3ToV2I(Vector3 input) { return new Vector2Int((int)input.x, (int)input.z); }
}
[System.Serializable]
public class Grid
{
    public float cost;
    public Vector2Int position;
    public Vector2Int parent;
}