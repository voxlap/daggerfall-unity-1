using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CopyCameraCullingMask : MonoBehaviour
{
    private Camera myCamera;
    private Camera theirCamera;

    private bool copyClearFlags;

    private void Start()
    {
        myCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (theirCamera)
        {
            myCamera.cullingMask = theirCamera.cullingMask;
            if (copyClearFlags)
                myCamera.clearFlags = theirCamera.clearFlags;
        }
    }

    public void SetCameraToCopy(Camera cam, bool copyClearFlagsToo = false)
    {
        theirCamera = cam;
        copyClearFlags = copyClearFlagsToo;
    }

    /// <summary>
    /// Adds a CopyCameraCullingMask component to a camera, and sets it up
    /// to continually copy another camera
    /// </summary>
    /// <param name="copyingCamera">Camera whose culling mask will be updated</param>
    /// <param name="cameraToCopy">Camera whose culling mask will be copied</param>
    /// <param name="copyClearFlagsToo">Should we also copy clear flags</param>
    public static void AddComponentToCamera(Camera copyingCamera, Camera cameraToCopy, bool copyClearFlagsToo = false)
    {
        CopyCameraCullingMask copier = copyingCamera.gameObject.AddComponent<CopyCameraCullingMask>();
        copier.SetCameraToCopy(cameraToCopy, copyClearFlagsToo);
    }
}
