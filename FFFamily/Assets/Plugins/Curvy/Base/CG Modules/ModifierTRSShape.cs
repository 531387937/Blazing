// =====================================================================
// Copyright 2013-2018 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Modifier/TRS Shape", ModuleName="TRS Shape", Description = "Transform,Rotate,Scale a Shape")]
    [HelpURL(CurvySpline.DOCLINK + "cgtrsshape")]
    public class ModifierTRSShape : TRSModuleBase, IOnRequestPath
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGShape), Name = "Shape A", ModifiesData = true)]
        public CGModuleInputSlot InShape = new CGModuleInputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGShape))]
        public CGModuleOutputSlot OutShape = new CGModuleOutputSlot();

        #region ### Public Properties ###

        [Obsolete("IOnRequestPath.PathLength and CGDataRequestRasterization.SplineAbsoluteLength are no more needed. SplineInputModuleBase.getPathLength is used instead")]
        public float PathLength
        {
            get
            {
                return (IsConfigured) ? InShape.SourceSlot().OnRequestPathModule.PathLength : 0;
            }
        }

        public bool PathIsClosed
        {
            get
            {
                return (IsConfigured) ? InShape.SourceSlot().PathProvider.PathIsClosed : false;
            }
        }

        #endregion

        #region ### IOnRequestProcessing ###

        public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
        {
            if (requestedSlot == OutShape)
            {
                var data = InShape.GetData<CGShape>(requests);

                if(data)
                {
                    Matrix4x4 mat = Matrix;
                    Matrix4x4 scaleLessMatrix = Matrix4x4.TRS(Transpose, Quaternion.Euler(Rotation), Vector3.one);

                    for (int i = 0; i < data.Count; i++)
                    {
                        data.Position[i] = mat.MultiplyPoint3x4(data.Position[i]);
                        data.Normal[i] = scaleLessMatrix.MultiplyVector(data.Normal[i]);
                    }

                    data.Recalculate();
                }
                return new CGData[1] { data };

            }
            return null;
        }

        #endregion



      
        
    }
}
