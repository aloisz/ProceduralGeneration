using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : GenericSingletonClass<GameManager>
{
    public List<IcosahedronGen> planets = new List<IcosahedronGen>();


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (var planet in planets)
            {
                planet.Generate();
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            SceneManager.LoadScene(0);
        }
    }
}
