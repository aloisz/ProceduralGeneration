using System.Threading.Tasks;
using UnityEngine;

public class testAsync : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Start");

        Exemple();
    }

    async void Exemple()
    {
        float t = await Task.Run(ExpensiveMethod);
        
        Debug.Log(t);
    }

    float ExpensiveMethod()
    {
        float size = 0;

        for (int i = 0; i < 100000000; i++)
        {
            size = Mathf.Pow(size, 1.5f) + Mathf.Sqrt(i);
        }

        return size;
    }
}
