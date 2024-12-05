using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Airplane
{
    public class UI_Cursor : MonoBehaviour
    {
        [FormerlySerializedAs("sprite")] [SerializeField] private RawImage CircleDir;

        [SerializeField] private PlaneController planeDir;
        [SerializeField] private RawImage AimDir;
        private RectTransform aimDirRectTransform;


        private void Start()
        {
            aimDirRectTransform = AimDir.GetComponent<RectTransform>();
        }

        private void Update()
        {
            CircleDir.transform.position = Input.mousePosition;
        }
        private void FixedUpdate()
        {
            aimDirRectTransform.position = planeDir.GetAimPosVector2();
        }
    }

}

