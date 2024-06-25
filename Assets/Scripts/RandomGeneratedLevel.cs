using MyBox;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomGeneratedLevel : LevelController
{
    private void OnEnable()
    {
        var allSelectables = GetComponentsInChildren<SelectableObject>();
        // 10, 11 => 8,9
        int length = allSelectables.Length;
        HashSet<Vector3> poses = new(length);
        int x, y;
        do
        {
            x = (int)Random.Range(1, width - 2);
            y = (int)Random.Range(1, height - 2);
            poses.Add(new Vector2(x, -y));
        }
        while (poses.Count < length);

        var posesList = poses.ToList();
        for (int i = 0; i < length; i++)
            allSelectables[i].transform.position = posesList[i].SetZ(allSelectables[i].transform.position.z);

        for (int i = 0; i < 6; i++)
            SwapOperations(allSelectables.GetRandom(), allSelectables.GetRandom());

        SetupCamera();
    }

    private void Update()
    {

    }
}