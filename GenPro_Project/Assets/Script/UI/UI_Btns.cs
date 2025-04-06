using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Btns : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private AnimationCurve InHover;
    [SerializeField] private AnimationCurve OutHover;
    [SerializeField] private float timer = 1.25f;
    [SerializeField] private float scaleMult = 1.25f;

    private Vector3 baseScale;

    private void Start()
    {
        baseScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(baseScale.x * scaleMult, timer).SetEase(InHover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(baseScale.x, timer).SetEase(OutHover);
    }
}
