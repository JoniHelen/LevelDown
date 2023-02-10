using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderToCustomTexture : MonoBehaviour
{
    [SerializeField] private RenderTexture texture;
    private Camera cam;

    public void UpdateTexture()
    {
        texture.width = Screen.width;
        texture.height = Screen.height;

        if (cam == null)
            cam = GetComponent<Camera>();

        cam.Render();
    }
}
