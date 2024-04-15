using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("Custom/ColorTint")]
public class MyColorTint : CustomVolumeBase
{
    public FloatParameter Intensity = new FloatParameter(1);
    
    public override bool IsActive()
    {
        return Intensity.overrideState;
    }
}
