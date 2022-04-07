using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignChildrenToTriangle : MonoBehaviour
{
    List<Transform> children = new List<Transform>();
    [SerializeField] float distance;
    float movingDistance;

    private void Awake()
    {
        foreach (Transform t in transform)
        {
            children.Add(t);
        }

        children[0].localPosition = new Vector3(0, 0, 1f / 3f * Mathf.Sqrt(3) * distance);
        children[1].localPosition = new Vector3(-0.5f * distance, 0, -Mathf.Sqrt(3) / 6 * distance);
        children[2].localPosition = new Vector3(0.5f * distance, 0, -Mathf.Sqrt(3) / 6 * distance);
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0, 360 * Time.deltaTime, 0));
        movingDistance = (((Mathf.Sin(Time.time * 5) + 1) / 2) + 1) * distance;

        children[0].localPosition = new Vector3(0, 0, 1f / 3f * Mathf.Sqrt(3) * movingDistance);
        children[1].localPosition = new Vector3(-0.5f * movingDistance, 0, -Mathf.Sqrt(3) / 6 * movingDistance);
        children[2].localPosition = new Vector3(0.5f * movingDistance, 0, -Mathf.Sqrt(3) / 6 * movingDistance);
    }
}
