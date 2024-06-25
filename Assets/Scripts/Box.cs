using DG.Tweening;
using MyBox;
using System;
using UnityEngine;

public class Box : SelectableObject /* Mover*/
{
    [SerializeField] private Transform targetPrefab;
    [NonSerialized] public bool isDetecting;

    protected override void Update()
    {
        if (!isDetecting)
            return;
        base.Update();
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector3.forward, 10f, 1 << 6);

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject != gameObject && hit.transform.name == targetPrefab.name)
            {
                isDetecting = false;
                operation.enabled = false;
                gameObject.layer = 0;
                var targetPos = hit.transform.position.SetZ(transform.position.z);
                transform.DOMove(targetPos, 2f).OnComplete(FindObjectOfType<LevelController>().CheckFinish);
                break;
            }
        }
    }

    private void Start()
    {
        isDetecting = true;
    }
}
