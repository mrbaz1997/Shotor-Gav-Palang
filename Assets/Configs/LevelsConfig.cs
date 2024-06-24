using MyBox;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Levels")]
public class LevelsConfig : ScriptableObject
{
    [SerializeField] private List<LevelController> levels;

    internal LevelController GetLevel(int currentLevel)
    {
        if(levels.IsNullOrEmpty())
            levels = Resources.LoadAll<LevelController>("Levels/").ToList();
        levels.RemoveAll(x => x == null);
        return levels.FirstOrDefault(x => x.levelId == currentLevel);

    }
}
