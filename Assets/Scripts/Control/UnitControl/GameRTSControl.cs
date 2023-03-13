using System.Collections.Generic;
using CodeMonkey.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameRTSControl : MonoBehaviour
{
    [SerializeField] private Transform selectedAreaTransform;
    private Vector3 startPosition;
    private List<UnitRTS> selectedUnitRTSList;

    private void Awake()
    {
        selectedUnitRTSList = new List<UnitRTS>();
        selectedAreaTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = UtilsClass.GetMouseWorldPosition();
            selectedAreaTransform.gameObject.SetActive(true);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 currentMousePosition = UtilsClass.GetMouseWorldPosition();
            Vector3 lowerLeft = new Vector3(Mathf.Min(startPosition.x, currentMousePosition.x),
                Mathf.Min(startPosition.y, currentMousePosition.y), -2);
            Vector3 upperRight = new Vector3(Mathf.Max(startPosition.x, currentMousePosition.x),
                Mathf.Max(startPosition.y, currentMousePosition.y));
            selectedAreaTransform.position = lowerLeft;
            selectedAreaTransform.localScale = upperRight - lowerLeft;
        }

        if (Input.GetMouseButtonUp(0) || EventSystem.current.IsPointerOverGameObject())
        {
            selectedAreaTransform.gameObject.SetActive(false);

            Collider2D[] collider2DArray = Physics2D.OverlapAreaAll(startPosition, UtilsClass.GetMouseWorldPosition());
            foreach (var unitRTS in selectedUnitRTSList)
            {
                unitRTS.SetSelectedActive(false);
            }

            selectedUnitRTSList.Clear();
            foreach (Collider2D collider2D in collider2DArray)
            {
                UnitRTS unitRTS = collider2D.GetComponent<UnitRTS>();
                if (unitRTS)
                {
                    unitRTS.SetSelectedActive(true);
                    selectedUnitRTSList.Add(unitRTS);
                }
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            Vector3 movetoPosition = UtilsClass.GetMouseWorldPosition();
            // List<Vector3> targetPositionList = GetPositionListAround(movetoPosition, 1f, 5);
            List<Vector3> targetPositionList =
                GetPositionListAround(movetoPosition, new float[] { 1f, 2f, 3f }, new int[] { 5, 10, 20 });
            int targetPositionListIndex = 0;
            foreach (UnitRTS unitRTS in selectedUnitRTSList)
            {
                unitRTS.MoveTo(targetPositionList[targetPositionListIndex]);
                targetPositionListIndex = (targetPositionListIndex + 1) % targetPositionList.Count;
            }
        }
    }

    private List<Vector3> GetPositionListAround(Vector3 startPosition, float[] ringDistanceArray,
        int[] ringPositionCountArray)
    {
        List<Vector3> positionList = new List<Vector3>();
        positionList.Add(startPosition);
        for (int i = 0; i < ringDistanceArray.Length; i++)
        {
            positionList.AddRange(GetPositionListAround(startPosition, ringDistanceArray[i],
                ringPositionCountArray[i]));
        }

        return positionList;
    }

    private List<Vector3> GetPositionListAround(Vector3 startPosition, float distance, int positionCount)
    {
        List<Vector3> positionList = new List<Vector3>();
        for (int i = 0; i < positionCount; i++)
        {
            float angle = i * (360f / positionCount);
            Vector3 dir = UtilsClass.ApplyRotationToVector(new Vector3(1, 0), angle);
            Vector3 position = startPosition + dir * distance;
            positionList.Add(position);
        }

        return positionList;
    }
}