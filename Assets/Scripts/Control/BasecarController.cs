using UnityEngine;
using UnityEngine.InputSystem;

public class BasecarController : MonoBehaviour
{
    [SerializeField] private bool isChosen = false;
    [SerializeField] private float speed = 4f;

    private Controls controls;
    private Controls.PlayerActions playerActions;
    private bool isDialogBlocking;

    private void Awake()
    {
        EventBus.Subscribe<DialogBlockingEvent>(e => isDialogBlocking = e.status);
        controls = new Controls();
        playerActions = controls.Player;
    }

    private void Start()
    {
        isDialogBlocking = false;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        // Move the player in the direction of the input
        Vector3 direction = playerActions.MoveBaseCar.ReadValue<Vector2>();
        transform.position += direction.normalized * (
            speed * SimulationSpeedControl.GetSimulationSpeed() * Time.deltaTime
        );

        // growth
        if (Mouse.current.leftButton.wasPressedThisFrame && !isDialogBlocking)
        {
            GrowthDemo growthDemo = GameObject.Find("GrowthDemoController").GetComponent<GrowthDemo>();
            Vector2 position = transform.position;
            position = new Vector2(
                Mathf.FloorToInt(position.x + 0.5f), Mathf.FloorToInt(position.y + 0.5f)
            );
            if (!growthDemo.Position2Growthed(position) && !growthDemo.FakeGrowthed(position))
            {
                Instantiate(Resources.Load<GameObject>("Prefabs/Objects/Food"),
                    new Vector3(position.x, position.y, -2.0f), Quaternion.identity);
                growthDemo.Position2GroundManager(position).SetGrowthed();
            }
        }
    }
}