using UnityEngine;

[RequireComponent(typeof(Light))]
public class LampOperation : OperationBase
{
    [SerializeField] private float lightRange;

    private void OnEnable()
    {
        GetComponent<Light>().range = lightRange;
    }
}
