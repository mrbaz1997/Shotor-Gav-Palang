using MyBox;
using UnityEngine;

public class LaserOperation : DirectionalOperationBase
{
    private Light light1, light2;
    [SerializeField] private float light1Range, light2Range;

    private void CalculateLaserSize()
    {
        var lights = GetComponentsInChildren<Light>();
        light1 = lights[0];
        light2 = lights[1];
        light1.range = light1Range;
        light2.range = light2Range;
        Vector2 direction = GetDirection();
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, 100, 1 << 7);

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.name.Equals("Wall"))
            {
                DrawLaser(ref direction, hit.distance);
                return;
            }
        }

        DrawLaser(ref direction, 100);
    }

    private void DrawLaser(ref Vector2 direction, float distance)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.size = spriteRenderer.size.SetY(distance);
        light2.transform.localPosition = new Vector3(0f, -spriteRenderer.size.y, -0.5f);
        transform.localEulerAngles = GetEulerAngles(ref direction);
        var collider = GetComponent<BoxCollider2D>();
        collider.size = new Vector2(spriteRenderer.size.x, (spriteRenderer.size.y - 0.5f) * 0.95f);
        collider.enabled = collider.size.y > 0.01f;
        collider.offset = new Vector2(0f, -spriteRenderer.size.y + (collider.size.y / 2f));
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
        CalculateLaserSize();
    }
}
