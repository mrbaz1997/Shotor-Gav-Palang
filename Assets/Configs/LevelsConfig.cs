using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Levels")]
public class LevelsConfig : ScriptableObject
{
    private List<LevelController> levels;

    internal LevelController GetLevel(int currentLevel)
    {
        levels = Resources.LoadAll<LevelController>("Levels/").ToList();
        levels.OrderBy(x => x.levelId);
        if (currentLevel < levels.Count)
            return levels[currentLevel];
        else
            return null;
    }
}
