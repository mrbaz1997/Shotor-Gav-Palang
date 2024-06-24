using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    //private SelectableObject[] allSeletables;
    private Camera mainCamera;
    SelectableObject selectedCharacter;
    private bool checkLightedAreas;
    private Light[] lights;
    public int levelId;
    private Action _onFinish;

    private void Start()
    {
        //allSeletables = GetComponentsInChildren<SelectableObject>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        else if (Input.GetMouseButtonDown(0))
            OnSelectObject();
    }

    private void OnEnable()
    {
        if (TryGetComponent(out Light component) && component.enabled)
        {
            checkLightedAreas = false;
        }
        else
        {
            lights = GetComponentsInChildren<Light>();
            checkLightedAreas = true;
        }
    }

    public void Setup(Action resetAction, Action onFinish)
    {
        var resetCanvas = Resources.Load<GameObject>("ResetUI");
        var resetButton = resetCanvas.GetComponentInChildren<Button>();
        Instantiate(resetButton, transform);
        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(() => resetAction());
        _onFinish = onFinish;
    }

    private void OnSelectObject()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null && CheckLightedAreas(mousePosition) && hit.collider.TryGetComponent(out SelectableObject character))
        {
            if (selectedCharacter == null)
            {
                selectedCharacter = character;
                character.OnSelect();
            }
            else
            {
                character.AssignOperatorObject(selectedCharacter.operation);
                selectedCharacter.AssignOperatorObject(character.operation);
                (character.operation, selectedCharacter.operation) = (selectedCharacter.operation, character.operation);
                selectedCharacter.OnUnselect();
                selectedCharacter = null;
            }
        }
    }

    private bool CheckLightedAreas(Vector2 selectedPosition)
    {
        if (checkLightedAreas)
        {
            foreach (var light in lights)
            {
                if (light.type == LightType.Point && Vector2.Distance(light.transform.position, selectedPosition) <= light.range)
                    return true;
            }
            return false;
        }
        else
        {
            return true;
        }
    }

    public void Finish()
    {
        _onFinish();
    }
}