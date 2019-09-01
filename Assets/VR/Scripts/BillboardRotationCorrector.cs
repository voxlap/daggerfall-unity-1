// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2019 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Vincent Wing (vincentwing00@gmail.com)
// Contributors:    
// 
// Notes:
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;

public class BillboardRotationCorrector : MonoBehaviour
{
    private void LateUpdate()
    {
        CorrectRotation();
    }

    public void CorrectRotation()
    {
        Vector3 lookAtPos = GameManager.Instance.MainCamera.transform.position;
        lookAtPos.y = transform.position.y;
        transform.LookAt(lookAtPos);
    }
}
