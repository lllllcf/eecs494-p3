using UnityEngine;
using UnityEngine.InputSystem;

public class FoodControl : MonoBehaviour
{
    private Camera _camera;
    private Vector2 pos;
    private bool color_changed;
    private bool isDialogBlocking;
    private GameObject banner;

    private void Awake()
    {
        EventBus.Subscribe<DialogBlockingEvent>(e => isDialogBlocking = e.status);
        _camera = Camera.main;
    }

    private void Start()
    {
        pos = new Vector2(Mathf.CeilToInt(transform.position.x), Mathf.CeilToInt(transform.position.y));
        color_changed = false;
        isDialogBlocking = false;
    }

    private void Update()
    {
        if (GameObject.Find("GrowthDemoController").GetComponent<GrowthDemo>().Position2Growthed(pos) ||
            GameObject.Find("GrowthDemoController").GetComponent<GrowthDemo>().FakeGrowthed(pos))
        {
            if (!color_changed)
            {
                color_changed = true;
                EventBus.Publish(new DisplayBannerEvent(
                    transform, -4, Color.red + Color.yellow / 2, StringPool.infestPromptBanner
                ));
            }
        }

        DetectObjectWithRaycast();
    }

    public void changeColor()
    {
        GetComponent<SpriteRenderer>().color = Color.green;
    }

    public Vector2 getPos()
    {
        return pos;
    }

    public void DetectObjectWithRaycast()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && !isDialogBlocking)
        {
            Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.name == "cherry" && color_changed)
                {
                    changeColor();
                    Destroy(GetComponentInChildren<BannerCanvas>().gameObject);
                }
            }
        }
    }
}