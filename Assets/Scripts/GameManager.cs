using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private LevelsConfig levelsConfig;
    [SerializeField] private int currentLevelId;
    private LevelController currentLevelObject;

    private void Start()
    {
        StartNewLevel();
    }

    private void StartNewLevel()
    {
        var levelPrefab = levelsConfig.GetLevel(currentLevelId);

        if (levelPrefab == null)
        {
            Debug.Log("No more level to load!");
        }
        else
        {
            currentLevelObject = Instantiate(levelPrefab);
            currentLevelObject.Setup(ResetLevel, GoNextLevel);
        }
    }

    private void ResetLevel()
    {
        Destroy(currentLevelObject.gameObject);
        StartNewLevel();
    }

    private void GoNextLevel()
    {
        currentLevelId++;
        StartCoroutine(DelayStartNextLevel());
    }

    private IEnumerator DelayStartNextLevel()
    {
        yield return new WaitForSeconds(0.25f);
        Destroy(currentLevelObject.gameObject);
        yield return new WaitForEndOfFrame();
        StartNewLevel();
    }
}
