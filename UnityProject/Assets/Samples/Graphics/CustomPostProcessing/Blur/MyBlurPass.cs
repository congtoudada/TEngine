using System.Collections;
using System.Collections.Generic;
using GameMain;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyBlurPass : CustomPostProcessPassBase
{
    //声明RTHandle用于存储对临时模糊纹理的引用的字段。
    private RTHandle blurTextureHandle;
    private static readonly int horizontalBlurId =
        Shader.PropertyToID("_HorizontalBlur");
    private static readonly int verticalBlurId =
        Shader.PropertyToID("_VerticalBlur");
    private RenderTextureDescriptor blurTextureDescriptor;
    
    public MyBlurPass(RenderPassEvent evt, Shader shader, string profileTag = "") : base(evt, shader, profileTag)
    {
        blurTextureDescriptor = new RenderTextureDescriptor(Screen.width,
            Screen.height, RenderTextureFormat.Default, 0);
        volume = VolumeManager.instance.stack.GetComponent<MyBlur>();
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        base.OnCameraSetup(cmd, ref renderingData);
        blurTextureDescriptor.width = renderingData.cameraData.cameraTargetDescriptor.width;
        blurTextureDescriptor.height = renderingData.cameraData.cameraTargetDescriptor.height;
        //Check if the descriptor has changed, and reallocate the RTHandle if necessary.
        RenderingUtils.ReAllocateIfNeeded(ref blurTextureHandle, blurTextureDescriptor);
    }

    protected override void OnRender(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var volume = GetVolume<MyBlur>();
        m_Material.SetFloat(horizontalBlurId, volume.horizontalBlur.value);
        m_Material.SetFloat(verticalBlurId, volume.verticalBlur.value);
        // Blit from the camera target to the temporary render texture,
        // using the first shader pass.
        Blit(cmd, m_CameraColorHandle, blurTextureHandle, m_Material, 0);
        // Blit from the temporary render texture to the camera target,
        // using the second shader pass.
        Blit(cmd, blurTextureHandle, m_CameraColorHandle, m_Material, 1);
        //Blit(cmd, m_CameraColorTarget, m_CameraColorTarget);
    }

    public override void Dispose()
    {
        if (blurTextureHandle != null) blurTextureHandle.Release();
        base.Dispose();
    }
}
