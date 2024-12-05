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
        transform.DOScale(baseScale, Random.Range(0.75f,1.75f));
    }
}
