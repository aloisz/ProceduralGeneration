using DG.Tweening;
using UnityEngine;

public class ObjSpawing : MonoBehaviour
{
    private Vector3 baseScale;


    public void SetBaseScale(float scale)
    {
        baseScale = new Vector3(scale, scale, scale);
    }
    
    void Start()
    {
        transform.DOScale(0, 0);
        transform.DOScale(baseScale, 1.25f);
    }
}
