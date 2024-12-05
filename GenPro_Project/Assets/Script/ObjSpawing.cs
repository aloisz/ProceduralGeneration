using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjSpawing : MonoBehaviour
{
    private Vector3 baseScale;

    public void SetBaseScale(float scale)
    {
        baseScale = new Vector3(scale, scale, scale);
    }

    IEnumerator Start()
    {
        transform.DOScale(0, 0);
        yield return new WaitForSeconds(Random.Range(0.5f, 1));
        transform.DOScale(baseScale, Random.Range(0.75f,1.25f)).SetEase(Ease.OutBounce);
        
    }
}
