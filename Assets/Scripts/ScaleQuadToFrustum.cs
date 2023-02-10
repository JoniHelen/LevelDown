using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniRx;

public class ScaleQuadToFrustum : MonoBehaviour
{
    [SerializeField] private Camera cam;

    private Vector3[] corners = new Vector3[4];

    void Start()
    {
        transform.ObserveEveryValueChanged(t => t.localPosition.z).Subscribe(v => SetQuadScale()).AddTo(this);
    }

    private void SetQuadScale()
    {
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), transform.localPosition.z, Camera.MonoOrStereoscopicEye.Mono, corners);
        transform.localScale = new Vector3(Vector3.Distance(corners[0], corners.First(x => x.y == corners[0].y && x != corners[0])), Vector3.Distance(corners[0], corners.First(x => x.x == corners[0].x && x != corners[0])));
    }

    private void OnValidate()
    {
        SetQuadScale();
    }
}
