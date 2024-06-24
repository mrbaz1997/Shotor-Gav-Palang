using MyBox;
using UnityEngine;

public class LaserOperation : DirectionalOperationBase
{
    [SerializeField] private Light light1, light2;
    private void Start()
    {
        Vector2 direction = GetDirection();
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, 100, 1 << 7);

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject != transform.parent.gameObject)
            {
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                spriteRenderer.size = spriteRenderer.size.SetY(Vector3.Distance(transform.position, hit.collider.transform.position) - 0.5f);
                light2.transform.localPosition = new Vector3(0f, -spriteRenderer.size.y, -0.5f);
                transform.localEulerAngles = GetEulerAngles(ref direction);
                var collider = GetComponent<BoxCollider2D>();
                collider.size = new Vector2(spriteRenderer.size.x, (spriteRenderer.size.y - 0.5f) * 0.95f);
                collider.enabled = collider.size.y > 0.01f;
                collider.offset = new Vector2(0f, -spriteRenderer.size.y + (collider.size.y / 2f));
                break;
            }
        }
    }

    private Vector3 GetEulerAngles(ref Vector2 direction)
    {
        Vector3 coeff = Vector3.forward;
        if (direction == Vector2.down)
            return coeff * 0;
        else if (direction == Vector2.right)
            return coeff * 90;
        else if (direction == Vector2.up)
            return coeff * 180;
        else
            return coeff * 270;
    }

    public override void Setup()
    {
        Start();
    }
}
