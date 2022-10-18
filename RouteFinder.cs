using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
public class RouteFinder : MonoBehaviour
{
    public Transform startTrans, endTrans;

    public List<Vector3> resultRoute;
    public Route targetRoute;
    public List<Route> routes = new List<Route>();
    public bool gizmoView;
    public float leapInterval;
    public float threshold;
    Vector3 tempVec;
    [SerializeField]
    private GUIStyle gUIStyle;

    public float nearRadius;
    [Range(1, 1000)]
    public int Step;
    void Update()
    {
        if (Input.GetMouseButtonDown(1)) { tempVec = Vector3.zero; }
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit) && hit.collider)
            {
                tempVec = hit.point;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit) && hit.collider &&
                tempVec != Vector3.zero)
            {
                var route = new Route();
                route.Start = tempVec;
                route.End = hit.point;
                routes.Add(route);
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (startTrans != null &&
            endTrans != null)
        {
            targetRoute.Start = startTrans.position;
            targetRoute.End = endTrans.position;
        }
        if (gizmoView)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(targetRoute.Start, 1f);
            Gizmos.DrawSphere(targetRoute.End, 1f);
            var globalPointList = new List<Vector3>();
            for (int i = 0; i < routes.Count; i++)
            {
                Gizmos.color = Color.black; Gizmos.DrawLine(routes[i].Start, routes[i].End);
                Gizmos.color = Color.green; Gizmos.DrawSphere(routes[i].Start, 0.15f);
                Gizmos.color = Color.red; Gizmos.DrawSphere(routes[i].End, 0.15f);
                for (int ii = 0; ii < routes.Count; ii++)
                {
                    var inter = Vector3.zero;
                    if (Intersection(routes[i], routes[ii], ref inter))
                    {
                        routes[i].Intersections.Add(inter);
                        globalPointList.Add(inter);
                    }
                }
            }
            for (int i = 0; i < routes.Count; i++)
            {
                var vec = routes[i].End - routes[i].Start;
                var distance = vec.magnitude;
                var dir = vec / distance;
                var count = Vector3.Distance(routes[i].Start, routes[i].End) / leapInterval;
                routes[i].wayPoints = new List<WayPoint>();
                var id = 0;
                for (int ii = 0; ii < count; ii++)
                {
                    var point = routes[i].Start + dir * ii * leapInterval;
                    if (0 < routes[i].Intersections.Count)
                    {
                        var nearIntersection = routes[i].Intersections.ToList().Find(x => Vector3.Distance(x, point) < threshold);
                        if (Vector3.Distance(point, nearIntersection) < threshold)
                        {
                            point = nearIntersection;
                        }
                    }
                    if (!routes[i].wayPoints.Exists(x => x.point == (point)))
                    {
                        globalPointList.Add(point);
                        var wayPoint = new WayPoint();
                        wayPoint.id = id;
                        wayPoint.point = point;
                        routes[i].wayPoints.Add(wayPoint);
                        id++;
                    }
                }
            }
            if (0 == globalPointList.Count) { return; }
            var targetStartNearPoint = NearPoint(globalPointList, targetRoute.Start, threshold);
            var targetEndNearPoint = NearPoint(globalPointList, targetRoute.End, threshold);
            if (targetStartNearPoint.Equals(Vector3.zero)) { return; }
            Gizmos.DrawWireSphere(targetStartNearPoint, threshold);
            Gizmos.DrawWireSphere(targetEndNearPoint, threshold);
            var wayPointList = new List<WayPoint>();
            for (int i = 0; i < routes.Count; i++)
            {
                for (int ii = 0; ii < routes[i].wayPoints.Count; ii++)
                {
                    var wayPoint = routes[i].wayPoints[ii];
                    var point = wayPoint.point;
                    var fromStartDistance = Vector3.Distance(point, targetStartNearPoint);
                    var toEndDistance = Vector3.Distance(point, targetEndNearPoint);
                    var totalCost = fromStartDistance + toEndDistance;
                    wayPoint.cost = (int)totalCost;
                    wayPointList.Add(wayPoint);
                    var dbg_str = wayPoint.id + ":" + wayPoint.cost.ToString();
                    Handles.Label(point + Vector3.up * 2, $"<color=#000000>{dbg_str}</color>", gUIStyle);
                }
            }
            resultRoute = new List<Vector3>();
            var currentPoint = targetStartNearPoint;
            for (int i = 0; i < Step; i++)//‰½‚Å‚à‚¢‚¢
            {
                var nearWayPointList = new List<WayPoint>();
                if (TryGetNearRoute(currentPoint, out Route currentRoute, out WayPoint currentWayPoint))
                {
                    if (currentWayPoint.id != 0 ||
                        currentWayPoint.id != currentRoute.wayPoints.Count)
                    {
                        var nextWayPoint = currentRoute.wayPoints[currentWayPoint.id + 1];
                        var previousWayPoint = currentRoute.wayPoints[currentWayPoint.id - 1];
                    }
                    if (IsIntersection(currentPoint, currentRoute))
                    {

                    }
                    // nextWayPont.point 
                    //var  currentRoute.wayPoints[currentWayPoint.id]; 
                }
                for (int ii = 0; ii < globalPointList.Count; ii++)
                {
                    if (Vector3.Distance(globalPointList[ii], currentPoint) < nearRadius &&
                       !resultRoute.Exists(x => x == globalPointList[ii]))
                    {
                        var nearWayPoint = wayPointList.Find(x => x.point == globalPointList[ii]);
                        nearWayPointList.Add(nearWayPoint);
                    }
                }
                if (0 < nearWayPointList.Count)
                {
                    var nearestCost = nearWayPointList.Min(x => x.cost);
                    var nearestWayPoints = nearWayPointList.FindAll(x => x.cost == nearestCost);
                    if (0 < nearestWayPoints.Count)
                    {
                        WayPoint nearestWayPoint = null;
                        var tempDistance = 0f;
                        var nearDistance = 0f;
                        foreach (var nWP in nearestWayPoints)
                        {
                            tempDistance = Vector3.Distance(nWP.point, targetStartNearPoint);
                            if (nearDistance != 0 && nearDistance > tempDistance)
                            {
                                nearDistance = tempDistance;
                                nearestWayPoint = nWP;
                            }
                        }
                        if (nearestWayPoint != null)
                        {
                            var nearestPoint = nearestWayPoint.point;
                            currentPoint = nearestPoint;
                            if (!resultRoute.Exists(x => x == currentPoint))
                            {
                                resultRoute.Add(currentPoint);
                            }
                        }
                    }
                }
                if (Vector3.Distance(currentPoint, targetEndNearPoint) < threshold)
                {
                    break;
                }
            }
            for (int i = 0; i < resultRoute.Count; i++)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(resultRoute[i], 0.25f);
            }
        }
    }
    private Vector3 NearPoint(List<Vector3> pointList, Vector3 t, float threshold)
    {
        var vec = pointList.Find(x => Vector3.Distance(x, t) < threshold);
        return vec;
    }
    private bool TryGetNearRoutes(Vector3 currentPoint, out List<Route> hitRouteList, out WayPoint wayPoint)
    {
        bool isHit = false;
        Route hitRoute = null;
        WayPoint hitWayPoint = null;
        List<Route> tempHitRouteList = new List<Route> ();
        for (int i = 0; i < routes.Count; i++)
        { 
            for (int ii = 0; ii < routes[i].wayPoints.Count; ii++)
            {
                if (currentPoint.Equals(routes[i].wayPoints[ii]))
                {
                    isHit = true;
                    hitRoute = routes[i];
                    hitWayPoint = routes[i].wayPoints[ii];
                    tempHitRouteList.Add(hitRoute); 
                }
            }
        }
        hitRouteList = tempHitRouteList;
        wayPoint = hitWayPoint;
        return isHit;
    }
    private bool IsIntersection(Vector3 currentPoint, Route currentRoute)
    {
        bool isHit = false;
        for (int i = 0; i < currentRoute.Intersections.Count; i++)
        {
            if (currentPoint.Equals(currentRoute.Intersections[i]))
            {
                isHit = true;
                break;
            }
        }
        return isHit;
    }
    bool Intersection(Route a, Route b, ref Vector3 intersection)
    {
        Vector2 vecvalue = Vector2.zero;
        var p1 = new Vector2(a.Start.x, a.Start.z);
        var p2 = new Vector2(a.End.x, a.End.z);
        var p3 = new Vector2(b.Start.x, b.Start.z);
        var p4 = new Vector2(b.End.x, b.End.z);
        if (LineIntersection(p1, p2, p3, p4, ref vecvalue))
        {
            intersection = new Vector3(vecvalue.x, 0, vecvalue.y);
            return true;
        }
        else
        {
            return false;
        }
    }
    bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection)
    {
        float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num;
        float x1lo, x1hi, y1lo, y1hi;
        Ax = p2.x - p1.x;
        Bx = p3.x - p4.x;
        if (Ax < 0) { x1lo = p2.x; x1hi = p1.x; } else { x1hi = p2.x; x1lo = p1.x; }
        if (Bx > 0) { if (x1hi < p4.x || p3.x < x1lo) return false; } else { if (x1hi < p3.x || p4.x < x1lo) return false; }
        Ay = p2.y - p1.y;
        By = p3.y - p4.y;
        if (Ay < 0) { y1lo = p2.y; y1hi = p1.y; } else { y1hi = p2.y; y1lo = p1.y; }
        if (By > 0) { if (y1hi < p4.y || p3.y < y1lo) return false; } else { if (y1hi < p3.y || p4.y < y1lo) return false; }
        Cx = p1.x - p3.x;
        Cy = p1.y - p3.y;
        d = By * Cx - Bx * Cy;
        f = Ay * Bx - Ax * By;
        if (f > 0) { if (d < 0 || d > f) return false; } else { if (d > 0 || d < f) return false; }
        e = Ax * Cy - Ay * Cx;
        if (f > 0) { if (e < 0 || e > f) return false; } else { if (e > 0 || e < f) return false; }
        if (f == 0) return false;
        num = d * Ax;
        intersection.x = p1.x + num / f;
        num = d * Ay;
        intersection.y = p1.y + num / f;
        return true;
    }
}
