using System.Collections;
using UnityEngine;

public class SpreadBuilding : MonoBehaviour
{
    [SerializeField] private SpriteRenderer healthBar;
    private VitalityController vitality;
    SpreadBuildingDrag spreadBuilding;

    float spread_time = 0.0f;
    bool finishSpread = false, isBuilderTutorialActive = false, total = false;
    float cooldownTimer = 0;

    private GameObject buildingController;
    private GrowthDemo growthDemo;
    NewBuilderTutorialController gd;
    private float original_bar_length;

    Vector2 pos;
    GameObject building;
    [SerializeField] private Animator _animator;

    private void Awake()
    {
        EventBus.Subscribe<StartBuilderTutorialEvent>(_ => isBuilderTutorialActive = true);
    }

    private void Start()
    {
        original_bar_length = healthBar.size.x;
        healthBar.size = new Vector2(0, healthBar.size.y);

        spreadBuilding = GameObject.Find("BuildingCanvas").transform.Find("Building0").transform.Find("BuildingIconHolder1").transform.Find("Spread").GetComponent<SpreadBuildingDrag>();
        buildingController = GameObject.Find("BuildingCanvas");
        if (!isBuilderTutorialActive)
        {
            growthDemo = GameObject.Find("GrowthDemoController").GetComponent<GrowthDemo>();
        }
        else
        {
            gd = GameObject.Find("BuilderTutorial").GetComponent<NewBuilderTutorialController>();
        }

        vitality = GameObject.Find("VitalityController").GetComponent<VitalityController>();
        vitality.decreaseVitality(500);
        vitality.decreaseVitalityGrowth(15);

        pos = spreadBuilding.getPos();
        building = spreadBuilding.getBuilding();

        if (!isBuilderTutorialActive)
        {
            spread_time = calculateSpreadTime(growthDemo.getInitPos(), pos);
        }
        else
        {
            spread_time = 5.0f;
        }

        StartCoroutine(StartSpread());
        AudioClip clip = Resources.Load<AudioClip>("Audio/PlaceSpore");
        AudioSource.PlayClipAtPoint(clip, AudioListenerManager.audioListenerPos, 0.7f);
    }

    private void Update()
    {
        if (finishSpread)
        {
            buildingController.GetComponent<BuildingController>().register_one_building(pos, building);
            if (!isBuilderTutorialActive)
            {
                growthDemo.Position2GroundManager(pos).SetGrowthed();
                growthDemo.AddToEdge(pos);
            }
            else
            {
                gd.Position2GroundManager(pos).SetGrowthed();
            }
            finishSpread = false;
        }
    }

    private void OnDestroy()
    {
        if (vitality != null)
            vitality.increaseVitalityGrowth(15);
        if (GameObject.Find("BuildingCanvas") != null)
            GameObject.Find("BuildingCanvas").GetComponent<BuildingController>().deregister_one_building(gameObject);

        if (!DestoryBuildingDrag.selfDestory)
        {
            EventBus.Publish(new AddExpEvent(5));
        }
        else
        {
            DestoryBuildingDrag.selfDestory = false;
        }
    }

    IEnumerator StartSpread()
    {
        cooldownTimer = spread_time;
        while (cooldownTimer > 0)
        {
            cooldownTimer -= SimulationSpeedControl.GetSimulationSpeed() * Time.deltaTime;
            healthBar.size = new Vector2(( 1 - cooldownTimer / spread_time) * original_bar_length, healthBar.size.y); 
            yield return null;
        }
        gameObject.transform.Find("SpreadBar").gameObject.SetActive(false);
        _animator.SetBool("SpreadBuildingEstablished", true);
        finishSpread = true;
        total = true;
    }

    private float calculateSpreadTime(Vector2 pos1, Vector2 pos2)
    {
        // 5 -> 10, 25 -> 30
        return Vector2.Distance(pos1, pos2) + 5.0f;
    }
}