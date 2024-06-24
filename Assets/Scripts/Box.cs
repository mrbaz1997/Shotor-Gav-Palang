using UnityEngine;

public class Box : SelectableObject /* Mover*/
{
    [SerializeField] private Transform targetPrefab;
    protected override void Update()
    {
        base.Update();
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector3.back, 10f, 1 << 7);

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject != gameObject)
            {
                if (hit.transform == targetPrefab)
                    FindObjectOfType<LevelController>().Finish();
                break;
            }
        }
    }
}
