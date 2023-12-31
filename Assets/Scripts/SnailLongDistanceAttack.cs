using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnailLongDistanceAttack : MonoBehaviour
{
    Vector3 baseCarDirection;
    public GridManager gridManager;

    float attackRange = 3.0f;
    bool canSpit = false;
    int damage = 1;

    bool attacklock = false;
    private float maxCoolDownTime;
    private float remainCoolDownTime;
    [SerializeField] private Image coolDownFog;

    private void Awake()
    {
        EventBus.Subscribe<BaseCarDirectionEvent>(e => baseCarDirection = e.direction);
        EventBus.Subscribe<SnailSpitEvent>(_ => Spit());
    }
    // Start is called before the first frame update
    void Start()
    {
        coolDownFog.fillAmount = 0;
        maxCoolDownTime = 2.5f;
        remainCoolDownTime = 0;
    }

    public void EnableSpit()
    {
        canSpit = true;
    }

    public void setDamage(int _damage)
    {
        damage = _damage;
    }

    public int getDamage()
    {
        return damage;
    }

    private void Spit()
    {
        if (canSpit && !attacklock)
        {
            StartCoroutine(StartAttack());
            StartCoroutine(CoolDown());
        }
    }

    IEnumerator StartAttack()
    {
        attacklock = true;
        float progress = 0.0f;
        float speed = 5.0f;
        bool attacked = false;

        yield return null;
        GameObject longRangeMucus = Instantiate(Resources.Load<GameObject>("Prefabs/Spit"), transform.position,
            Quaternion.identity);
        AudioClip clip = Resources.Load<AudioClip>("Audio/Spit");
        AudioSource.PlayClipAtPoint(clip, AudioListenerManager.audioListenerPos);
        longRangeMucus.transform.eulerAngles = new Vector3(0.0f, 0.0f, setRotation(baseCarDirection.normalized.x, baseCarDirection.normalized.y));
        Vector3 init_pos = transform.position;
        Vector3 dest_pos = transform.position + baseCarDirection.normalized * attackRange;
        print(dest_pos);
        while (progress < 1)
        {
            progress += Time.deltaTime * speed;

            Vector3 new_position = Vector3.Lerp(init_pos, dest_pos, progress);
            if (new_position.x > 49.0f || new_position.y > 49.0f || new_position.x < 0.0f || new_position.y < 0.0f)
                progress = 1.1f;
            else
            {
                longRangeMucus.transform.position = new_position;
            }
            yield return new WaitForEndOfFrame();

            GameObject target = findTarget(longRangeMucus.transform.position);
            if (target != null)
            {
                target.GetComponentInChildren<HitHealth>().GetDamage(damage);
                progress = 1.1f;
                Destroy(longRangeMucus);
                attacked = true;
            }   
        }

        if (!attacked)
        {
            Destroy(longRangeMucus);
            Vector2 pos = new Vector2(Mathf.Clamp(Mathf.FloorToInt(dest_pos.x), 0, 49), Mathf.Clamp(Mathf.CeilToInt(dest_pos.y), 0, 49)); // may be uncorrect
            gridManager.GetTileAtPosition(pos).GetComponentInChildren<GroundTileManager>().SetMucus();
            gridManager.GetTileAtPosition(pos).GetComponentInChildren<GroundTileManager>().RemoveGrowthed();
        }
    }

    private GameObject findTarget(Vector3 pos)
    {
        float detect_dis = 2.0f;
        GameObject nearestBuilding = BuildingController.NearestBuilding(pos);
        float building_dis;

        if (nearestBuilding == null)
            building_dis = 100.0f;
        else
        {
            if(nearestBuilding.name == "Mushroom")
                building_dis = Vector3.Distance(new Vector3(pos.x, pos.y, 0.0f), nearestBuilding.transform.position);
            else
                building_dis = Vector3.Distance(new Vector3(pos.x, pos.y, -2.0f), nearestBuilding.transform.position);
        }


        //print(nearestBuilding.transform.position);

        GameObject nearestCitizen = CitizenControl.NearestCitizen(pos);
        //GameObject nearestCitizen = null;
        float citizen_dis;

        if (nearestCitizen == null)
            citizen_dis = 100.0f;
        else
            citizen_dis = Vector3.Distance(new Vector3(pos.x, pos.y, -2.0f), nearestCitizen.transform.position);

        if (building_dis < citizen_dis)
        {
            if (building_dis < detect_dis)
                return nearestBuilding;
            else
                return null;
        }
        else
        {
            if (citizen_dis < detect_dis)
                return nearestCitizen;
            else
                return null;
        } 
    }

    private float setRotation(float x, float y)
    {
        if (x == 0) { return y * 90.0f; }
        else if (y == 0) { if (x < 0) { return 180.0f; } }
        else if (x * y > 0) { return x > 0 ? 45.0f : -135.0f; }
        else { return x > 0 ? -45.0f : 135.0f; }

        return 0.0f;
    }

    private IEnumerator CoolDown()
    {
        remainCoolDownTime = maxCoolDownTime;
        while (remainCoolDownTime > 0)
        {
            remainCoolDownTime -= Time.deltaTime;
            coolDownFog.fillAmount = remainCoolDownTime / maxCoolDownTime;
            yield return null;
        }
        
        attacklock = false;
        coolDownFog.fillAmount = 0;
        remainCoolDownTime = 0;
    }
}
