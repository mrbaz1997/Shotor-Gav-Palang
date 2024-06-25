using MyBox;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Levels")]
public class LevelsConfig : ScriptableObject
{
    private HashSet<LevelController> levels;

    internal LevelController GetLevel(int currentLevel)
    {
        if(levels.IsNullOrEmpty())
            levels = Resources.LoadAll<LevelController>("Levels/").ToHashSet();
        levels.RemoveWhere(x => x == null);
        return levels.FirstOrDefault(x => x.levelId == currentLevel);

    }
}
