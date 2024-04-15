/****************************************************
  文件：CustomPostProcessPassBase.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2024年04月15日 16:22:50
  功能：
*****************************************************/

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GameMain
{
    public abstract class CustomPostProcessPassBase : ScriptableRenderPass
    {
        #region 字段
        //接取屏幕原图的属性名
        protected static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        //暂存贴图的属性名
        protected static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");

        //CommandBuffer的名称
        protected string cmdName;
        //继承VolumeComponent的组件（父装子）
        protected CustomVolumeBase volume;
        //当前Pass使用的材质
        protected Material m_Material;
        //当前渲染的目标
        protected RTHandle m_CameraColorHandle;
        protected RTHandle m_CameraDepthHandle;
        //sampler
        private ProfilingSampler m_ProfilingSampler;
        #endregion

        #region 函数
        //-------------------------构造------------------------------------
        /// <summary>
        /// 构造函数，用来初始化RenderPass
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="shader"></param>
        public CustomPostProcessPassBase(RenderPassEvent evt, Shader shader, string profileTag = "")
        {
            cmdName = this.GetType().Name + "_cmdName";
            renderPassEvent = evt;//设置渲染事件位置
            //不存在则返回
            if (shader == null)
            {
                Debug.LogError("不存在" + this.GetType().Name + "shader");
                return;
            }
            m_Material = CoreUtils.CreateEngineMaterial(shader);//新建材质
            if (string.IsNullOrEmpty(profileTag)) profileTag = this.GetType().Name;
            m_ProfilingSampler = new ProfilingSampler(profileTag);
        }

        //----------------------子类继承但禁止重写---------------------------
        /// <summary>
        /// 返回Volume
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetVolume<T>() where T : CustomVolumeBase
        {
            return volume as T;
        }

        /// <summary>
        /// 重写 Execute 
        /// 此函数相当于OnRenderImage，每帧都会被执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="renderingData"></param>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            //是否是Game视图
            if (cameraData.camera.cameraType != CameraType.Game)
                return;
            //材质是否存在
            if (m_Material == null)
            {
                Debug.LogError("材质初始化失败");
                return;
            }
            //摄像机关闭后处理
            if (!cameraData.postProcessEnabled)
            {
                Debug.LogError("相机后处理是关闭的！！！");
                return;
            }
            if (volume == null)
            {
                Debug.LogError("没有找到CustomBase！！！");
                return;
            }
            if (!volume.IsActive()) return;
            
            //特殊注入点需要额外配置
            if (renderPassEvent == RenderPassEvent.AfterRenderingPostProcessing)
            {
                SetTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
            }
            var cmd = CommandBufferPool.Get(cmdName);//从池中获取CMD
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                OnRender(cmd, ref renderingData);//将该Pass的渲染指令写入到CMD中
            }
            context.ExecuteCommandBuffer(cmd);//执行CMD
            CommandBufferPool.Release(cmd);//释放CMD
        }

        //-----------------------子类必须或选择性重写----------------------------------
        /// <summary>
        /// 设置渲染目标
        /// </summary>
        /// <param name="renderingData"></param>
        public virtual void SetTarget(RTHandle colorHandle, RTHandle depthHandle = null)
        {
            m_CameraColorHandle = colorHandle;
            m_CameraDepthHandle = depthHandle;
        }
        
        /// <summary>
        /// 虚方法，供子类重写，需要将该Pass的具体渲染指令写入到CMD中
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="renderingData"></param>
        protected abstract void OnRender(CommandBuffer cmd, ref RenderingData renderingData);
        
        public virtual void Dispose()
        {
            if (m_Material != null)
            {
                CoreUtils.Destroy(m_Material);
                m_ProfilingSampler = null;
            }
        }
        #endregion

    }
}