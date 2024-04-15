using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GameMain
{
    public abstract class CustomVolumeBase : VolumeComponent, IPostProcessComponent
    {
        public abstract bool IsActive();

        public virtual bool IsTileCompatible() => false;
        
        public void Dispose() {  
            Dispose(true);  
            GC.SuppressFinalize(this);  
        }
	
        public virtual void Dispose(bool disposing) {
            
        }  
    }
}

