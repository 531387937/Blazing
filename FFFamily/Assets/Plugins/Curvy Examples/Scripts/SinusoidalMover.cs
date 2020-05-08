// =====================================================================
// Copyright 2013-2019 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;

/// <summary>
/// Modifies a Transform's position in a sinusoidal way
/// </summary>
public class SinusoidalMover : MonoBehaviour
{
    public float MovementAmplitude = 11;

    void Update()
    {
        transform.position = new Vector3(MovementAmplitude * Mathf.Sin(Time.time), transform.position.y, MovementAmplitude * Mathf.Sin(Time.time * 2));
    }
}
