using System.Collections;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    public Camera mainCamera;

    public GameObject heroPrefab;

    public GameObject targetPrefab;

    public int numHeroes = 500;

    public int numTargets = 5000;

    private void Awake()
    {
        StartCoroutine(Startup());
    }

    private IEnumerator Startup()
    {
        for (var i = 0; i < 10; ++i)
            yield return null;

        for (var i = 0; i < numHeroes; ++i)
        {
            Instantiate(heroPrefab, mainCamera.GetRandomPosition(), Quaternion.identity);
        }

        for (var i = 0; i < numTargets; ++i)
        {
            Instantiate(targetPrefab, mainCamera.GetRandomPosition(), Quaternion.identity);
        }
    }
}