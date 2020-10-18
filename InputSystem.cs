using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
public class InputSystem : MonoBehaviour
{

    PlayerInput playerInput;
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }
    public static Vector2 L_Stick;
    public static Vector2 R_Stick;
    public static bool A_Button;
    public static bool B_Button;
    public static bool X_Button;
    public static bool Y_Button;
    public static bool Select;
    public static bool LB, RB;
    public static float LT, RT;

    int aa, bb, xx, yy;
    private void Update()
    {
        float a_value = playerInput.actions["A"].ReadValue<float>();
        if (a_value == 1) { aa++; }
        else { aa = 0; A_Button = false; }
        if (aa == 1) { A_Button = true; }
        else if (1 < aa) { A_Button = false; }

        float b_value = playerInput.actions["B"].ReadValue<float>();
        if (b_value == 1) { bb++; }
        else { bb = 0; B_Button = false; }
        if (bb == 1) { B_Button = true; }
        else if (1 < bb) { B_Button = false; }

        float x_value = playerInput.actions["X"].ReadValue<float>();
        if (x_value == 1) { xx++; }
        else { xx = 0; X_Button = false; }
        if (xx == 1) { X_Button = true; }
        else if (1 < xx) { X_Button = false; }

        float y_value = playerInput.actions["Y"].ReadValue<float>();
        if (y_value == 1) { yy++; }
        else { yy = 0; Y_Button = false; }
        if (yy == 1) { Y_Button = true; }
        else if (1 < yy) { Y_Button = false; }


        L_Stick = playerInput.actions["L_Stick"].ReadValue<Vector2>();
        R_Stick = playerInput.actions["R_Stick"].ReadValue<Vector2>();
        LB = playerInput.actions["LB"].ReadValue<float>() > 0;
        RB = playerInput.actions["RB"].ReadValue<float>() > 0;
        LT = playerInput.actions["LT"].ReadValue<float>();
        RT = playerInput.actions["RT"].ReadValue<float>();

    }
