/****************************************************
  文件：CustomPostProcessFeature.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2024年04月15日 16:22:50
  功能：
*****************************************************/
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GameMain
{
    public class CustomPostProcessFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            public string RenderPassName;
            //指定该RendererFeature在渲染流程的哪个时机插入
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            //指定一个shader
            public Shader shader;
            //profileSampler名称
            public string profileTag = "";
            //是否需要深度信息
            public bool isDepth = false;
            //是否开启
            public bool activeff;
            public CustomPostProcessPassBase PostProcessPassBase;
        }
        public List<Settings> settings = new List<Settings>();//开放设置

        /// <summary>
        /// 当RenderFeature参数面板修改时调用，利用类名 + 反射实例化RenderPass
        /// </summary>
        public override void Create()
        {
            if(settings != null && settings.Count > 0)
            {
                for(int i = 0; i < settings.Count; i++)
                {
                    if (settings[i].activeff && settings[i].shader != null)
                    {
                        try
                        {
                            if (settings[i].PostProcessPassBase != null)
                            {
                                settings[i].PostProcessPassBase.Dispose();
                            }
                            settings[i].PostProcessPassBase = Activator.CreateInstance(Type.GetType(settings[i].RenderPassName), 
                                settings[i].renderPassEvent, settings[i].shader, settings[i].profileTag) as CustomPostProcessPassBase;
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.Message + "后处理C#脚本名有误，请检查RenderPassName   :" + settings[i].RenderPassName);
                        }
                    }
                }
            }
        }
        
        //渲染目标初始化后的回调。这允许在目标创建和准备好之后从RenderFeature访问目标。
        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                // Calling ConfigureInput with the ScriptableRenderPassInput.Color argument
                // ensures that the opaque texture is available to the Render Pass.
                for (int i = 0; i < settings.Count; i++)
                {
                    // settings[i].PostProcessPassBase?.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
                    // settings[i].PostProcessPassBase?.SetTarget(renderingData.cameraData.renderer.cameraColorTargetHandle,
                    //     renderingData.cameraData.renderer.cameraDepthTargetHandle);
                    if (settings[i].isDepth)
                    {
                        settings[i].PostProcessPassBase
                            ?.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
                        settings[i].PostProcessPassBase?.SetTarget(renderingData.cameraData.renderer.cameraColorTargetHandle, 
                            renderingData.cameraData.renderer.cameraDepthTargetHandle);
                    }
                    else
                    {
                        settings[i].PostProcessPassBase?.ConfigureInput(ScriptableRenderPassInput.Color);
                        settings[i].PostProcessPassBase?.SetTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
                    }
                }
            }
        }

        /// <summary>
        /// 将RenderPass注入到Render中
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="renderingData"></param>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                if(settings != null)
                {
                    for (int i = 0; i < settings.Count; i++)
                    {
                        if(settings[i].activeff && settings[i].PostProcessPassBase != null)
                        {
                            // Debug.Log("注入" + i);
                            renderer.EnqueuePass(settings[i].PostProcessPassBase);   //注入Render的渲染队列
                        }
                    }
                }
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            for (int i = 0; i < settings.Count; i++)
            {
                settings[i].PostProcessPassBase.Dispose();
            }
        }
    }
}
