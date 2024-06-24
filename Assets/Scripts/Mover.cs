using UnityEngine;

public class Mover : DirectionalOperationBase
{
    [SerializeField] private float speed = 2f;

    private void FixedUpdate()
    {
        if (direction == DiretionType.Nothing)
            return;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.parent.position, GetDirection(), 0.5f, 1 << 7);

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject != transform.parent.gameObject)
            {
                OnHitForgienObjects(hit.collider);
                break;
            }
        }

        Move();
    }

    protected virtual void OnHitForgienObjects(Collider2D hit)
    {
        if (hit.transform.CompareTag("Impassable"))
            OnChangeSide();
    }

    private void Move()
    {
        transform.parent.Translate(speed * Time.fixedDeltaTime * GetDirection());
    }
}