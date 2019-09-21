// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2019 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Vincent Wing (vincentwing00@gmail.com)
// Contributors: InconsolableCellist
// 
// Notes:
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingUI : MonoBehaviour
{
    public RawImage cursor;
    public string cursorResourcePath = "Cursor";

    protected RawImage rawImage;
    protected Canvas canvas;
    protected RectTransform rectTF;
    protected BoxCollider boxCollider;
    protected Vector2 virtualMousePos = Vector2.zero;
    protected Vector2 offscreenMouse = new Vector2(-1, -1);
    protected bool _init = false;

    protected int lastActivatedFrame = 0;

    protected virtual void Start()
    {
        // Get references
        rawImage = GetComponentInChildren<RawImage>();
        canvas = GetComponent<Canvas>();
        rectTF = GetComponent<RectTransform>();
        boxCollider = GetComponent<BoxCollider>();

        //spawn cursor
        if (cursor && !string.IsNullOrEmpty(cursorResourcePath))
        {
            cursor.texture = Resources.Load<Texture2D>(cursorResourcePath);
        }
        else
        {
            Debug.LogError("Unable to create a cursor in the floating VR UI.");
        }

        //set event camera
        if (ViveControllerInput.Instance)
            canvas.worldCamera = ViveControllerInput.Instance.ControllerCamera;

        _init = true;
    }

    protected virtual void LateUpdate()
    {
        if (cursor.gameObject.activeSelf && lastActivatedFrame != Time.frameCount)
            SetCursorActive(true);
    }
    protected virtual void SetCursorActive(bool active)
    {
        cursor.gameObject.SetActive(active);
    }

    public virtual void ResizeCanvas(Vector2 size)
    {
        if (!_init)
            return;

        Vector2 oldSize = rectTF.sizeDelta;
        //resize rect and box collider
        rectTF.sizeDelta = size;
        boxCollider.size = new Vector3(size.x, size.y, .1f);
        boxCollider.center = Vector3.forward * .05f;
        //resize transform
        Vector3 tfScale = (oldSize/size) * (Vector2)transform.localScale;
        tfScale.z = 1f;
        transform.localScale = tfScale;
    }

    public virtual void HandlePointer(Vector3 point)
    {
        if (!_init)
            return;
        lastActivatedFrame = Time.frameCount;
        RepositionCursor(point);
    }

    public virtual void HandleClick(Vector3 point)
    {
        HandlePointer(point);
    }

    public virtual void HandleUnclick(Vector3 point)
    {
        HandlePointer(point);
    }

    protected virtual void RepositionCursor(Vector2 point)
    {
        if (!cursor.gameObject.activeSelf)
           SetCursorActive(true);
        cursor.transform.localPosition = point;
    }
}
