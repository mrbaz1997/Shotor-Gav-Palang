using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    public int levelId;
    [SerializeField] private uint width, height;
    private Camera mainCamera;
    SelectableObject selectedCharacter;
    private bool checkLightedAreas;
    private Light[] lights;
    private Action _onFinish;
    //private SelectableObject[] allSeletables;

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
        SetupCamera();
        SetupResetButton(resetAction);
        _onFinish = onFinish;
    }

    private void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        mainCamera.transform.position = new Vector3((width / 2f) - 0.5f, 0.5f - (height / 2f), mainCamera.transform.position.z);
        float levelAspect = (float)width / height;
        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect > 1f)
        {
            if (levelAspect > screenAspect)
                mainCamera.orthographicSize = width / screenAspect;
            else
                mainCamera.orthographicSize = height / screenAspect;
        }
        else
        {
            if (levelAspect < screenAspect)
                mainCamera.orthographicSize = (height / 2f) + 0.5f;
            else
                mainCamera.orthographicSize = height / screenAspect;
        }
    }

    private void SetupResetButton(Action resetAction)
    {
        var resetCanvas = Resources.Load<GameObject>("ResetUI");
        resetCanvas = Instantiate(resetCanvas, transform);
        var resetButton = resetCanvas.GetComponentInChildren<Button>();
        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(() => resetAction());
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

    public void CheckFinish()
    {
        if (!GetComponentsInChildren<Box>().Any(x => x.isDetecting))
            _onFinish();
    }

    public void SetData(int levelId, uint width, uint height)
    {
        this.levelId = levelId;
        this.width = width;
        this.height = height;
    }
}