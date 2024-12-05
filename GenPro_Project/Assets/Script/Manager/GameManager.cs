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
            RegeneratePlanet();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            LaunchGame();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LaunchMenu();
        }
    }


    public void RegeneratePlanet()
    {
        foreach (var planet in planets)
        {
            planet.Generate();
        }
        //StartCoroutine(nameof(RegeneratePlanetCoroutine));
    }

    private IEnumerator RegeneratePlanetCoroutine()
    {
        foreach (var planet in planets)
        {
            planet.Generate();
            yield return null;
        }
    }

    public void LaunchGame()
    {
        SceneManager.LoadScene(1);
    }

    public void LaunchMenu()
    {
        SceneManager.LoadScene(0);
    }
    
    public void Quit()
    {
        Application.Quit();
    }
}
