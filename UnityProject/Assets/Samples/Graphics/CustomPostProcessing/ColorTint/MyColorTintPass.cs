using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyColorTintPass : CustomPostProcessPassBase
{
    
    public MyColorTintPass(RenderPassEvent evt, Shader shader, string profileTag = "") : base(evt, shader, profileTag)
    {
        volume = VolumeManager.instance.stack.GetComponent<MyColorTint>();
    }

    protected override void OnRender(CommandBuffer cmd, ref RenderingData renderingData)
    {
        m_Material.SetFloat("_Intensity", ((MyColorTint)volume).Intensity.value);
        Blitter.BlitCameraTexture(cmd, m_CameraColorHandle, m_CameraColorHandle, m_Material, 0);
    }
}
