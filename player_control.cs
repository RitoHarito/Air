using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_control : MonoBehaviour
{
    public float speed;
    public float rot;
    public Transform Head;
    float angle;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var pos = transform.position;
        //pos += transform.right * Input.GetAxis("Horizontal") * XSpeed;
        pos += transform.forward * InputSystem.L_Stick.y * speed;
        transform.position = pos;
        var diff = Head.position - transform.position;
        var axis = Vector3.Cross(transform.forward, diff);
       angle = Vector3.Angle(transform.forward, diff) * (axis.y < 0 ? -1 : 1);
        if (45 < angle && angle <90)
        {
            transform.rotation =
                Quaternion.Euler(
                transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y + rot,
                transform.rotation.eulerAngles.z);
        }
        else if (-90 < angle && angle < -45)
        {
            transform.rotation =
               Quaternion.Euler(
               transform.rotation.eulerAngles.x,
               transform.rotation.eulerAngles.y - rot,
               transform.rotation.eulerAngles.z);
        }

        //transform.position = new Vector3(transform.position.x + InputSystem.L_Stick.x, transform.position.y, transform.position.z + InputSystem.L_Stick.y);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 100, 100), "This.Y = " + transform.rotation.eulerAngles.y.ToString());
        GUI.Label(new Rect(20, 40, 100, 100), "Head.Y = " + Head.transform.rotation.eulerAngles.y.ToString());
        GUI.Label(new Rect(20, 60, 100, 100), "Angle = " + angle.ToString());
    }
}
