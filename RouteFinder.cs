 using System.Collections.Generic;  
using UnityEngine; 
public class RouteFinder : MonoBehaviour
{
    public Route targetRoute;
    public List<Route> routes = new List<Route>();
    public bool gizmoView;
    public float leapInterval;
    public float threshold;
    Vector3 tempVec; 
    void Update()
    { 
        if (Input.GetMouseButtonDown(1))
        {
            tempVec = Vector3.zero;
        }
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
        var pointList = new List<Vector3>();
        if (gizmoView)
        {
            for (int i = 0; i < routes.Count; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(routes[i].Start, routes[i].End);
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(routes[i].Start, 1f);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(routes[i].End, 1f);
            }
            Gizmos.color = Color.black;
            for (int i = 0; i < routes.Count; i++)
            {
                var vec = routes[i].End - routes[i].Start;
                var distance = vec.magnitude;
                var dir = vec / distance;
                var count = Vector3.Distance(routes[i].Start, routes[i].End) / leapInterval;
                for (int ii = 0; ii < count; ii++)
                {
                    var point = routes[i].Start + dir * ii * leapInterval;
                    Gizmos.DrawSphere(point, 0.5f);
                    pointList.Add(point);
                    //Gizmos.DrawSphere(Vector3.Lerp(routes[i].Start, routes[i].End, (ii * leapInterval) * 0.01f), 0.5f);
                }
            }
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetRoute.Start, 2f);
            Gizmos.DrawSphere(NearPoint(pointList, targetRoute.Start, threshold), 2f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetRoute.End, 2f);
            Gizmos.DrawSphere(NearPoint(pointList, targetRoute.End, threshold), 2f);
            for (int i = 0; i < pointList.Count; i++)
            {

            } 
        }
    } 
    private Vector3 NearPoint(List<Vector3> pointList, Vector3 targetPoint, float threshold)
    {
        var vec = Vector3.zero; 
        vec = pointList.Find(x => threshold > Vector3.Distance(x, targetPoint)); 
        //foreach (var point in pointList)
        //{
        //    var distance = Vector3.Distance(point, targetPoint);
        //    if (threshold == 0 || threshold < distance)
        //    { 
        //        vec = point;
        //    }
        //}
        return vec;
    }
}
