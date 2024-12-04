using System;
using UnityEngine;

public class RotatingPlanet : MonoBehaviour
{
    [SerializeField] private Vector3 rotatingVector = Vector3.up;
    [SerializeField] private float speed = 10;

    private void Update()
    {
        transform.Rotate(rotatingVector, Time.deltaTime * speed);
    }
}
