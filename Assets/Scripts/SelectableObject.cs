using DG.Tweening;
using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    public OperationBase operation;
    private Tweener shakeTween;
    private Transform target;

    protected virtual void Update()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.transform.parent != transform)
            {
                if (hit.CompareTag("Killer"))
                {
                    GetComponent<Collider2D>().enabled = false;
                    UpdateKillerComponent(hit);
                    Debug.Log("Collided With Killer");
                    Destroy(gameObject);
                    break;
                }
            }
        }
    }

    private static void UpdateKillerComponent(Collider2D hit)
    {
        if (hit.TryGetComponent(out LaserOperation laser))
            laser.Setup();
    }

    internal void AssignOperatorObject(OperationBase operation)
    {
        if (operation == null)
            return;

        operation.transform.SetParent(transform, false);
        operation.Setup();
    }

    internal void OnSelect()
    {
        target = Instantiate(Resources.Load<Transform>("ActiveSelector"), transform);
        target.gameObject.SetActive(true);
        shakeTween = target.DOShakeScale(1f, 0.2f, 2, 0f, randomnessMode: ShakeRandomnessMode.Harmonic).SetLoops(-1);
    }

    internal void OnUnselect()
    {
        shakeTween?.Kill();
        Destroy(target.gameObject);
    }
}
