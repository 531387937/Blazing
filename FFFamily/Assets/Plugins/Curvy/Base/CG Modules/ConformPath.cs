// =====================================================================
// Copyright 2013-2018 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Modifier/Conform Path", ModuleName = "Conform Path", Description = "Projects a path")]
    [HelpURL(CurvySpline.DOCLINK + "cgconformpath")]
    public class ConformPath : CGModule, IOnRequestPath
    {

        [HideInInspector]
        [InputSlotInfo(typeof(CGPath), Name = "Path", ModifiesData = true)]
        public CGModuleInputSlot InPath = new CGModuleInputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGPath))]
        public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

        [SerializeField]
        [VectorEx]
        [Tooltip("The direction to raycast in ")]
        Vector3 m_Direction = new Vector3(0, -1, 0);
        [SerializeField]
        [Tooltip("The maximum raycast distance")]
        float m_MaxDistance = 100;
        [SerializeField]
        [Tooltip("Defines an offset shift along the raycast direction")]
        float m_Offset;
        [SerializeField]
        [Tooltip("If enabled, the entire path is moved to the nearest possible distance. If disabled, each path point is moved individually")]
        bool m_Warp;
        [SerializeField]
        [Tooltip("The layers to raycast against")]
        LayerMask m_LayerMask;

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// The direction to raycast in 
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                return m_Direction;
            }
            set
            {
                if (m_Direction != value)
                    m_Direction = value;
                Dirty = true;
            }
        }

        /// <summary>
        /// The maximum raycast distance
        /// </summary>
        public float MaxDistance
        {
            get { return m_MaxDistance; }
            set
            {
                if (m_MaxDistance != value)
                    m_MaxDistance = value;
                Dirty = true;
            }
        }

        /// <summary>
        /// Defines an offset shift along the raycast direction
        /// </summary>
        public float Offset
        {
            get { return m_Offset; }
            set
            {
                if (m_Offset != value)
                    m_Offset = value;
                Dirty = true;
            }
        }

        /// <summary>
        /// If enabled, the entire path is moved to the nearest possible distance. If disabled, each path point is moved individually
        /// </summary>
        public bool Warp
        {
            get { return m_Warp; }
            set
            {
                if (m_Warp != value)
                    m_Warp = value;
                Dirty = true;
            }
        }

        /// <summary>
        /// The layers to raycast against
        /// </summary>
        public LayerMask LayerMask
        {
            get { return m_LayerMask; }
            set
            {
                if (m_LayerMask != value)
                    m_LayerMask = value;
                Dirty = true;
            }
        }

        #endregion

        #region ### Private Fields & Properties ###
        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected override void OnEnable()
        {
            base.OnEnable();
            //Properties.MinWidth = 250;
            Properties.LabelWidth = 80;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Direction = m_Direction;
            MaxDistance = m_MaxDistance;
            Offset = m_Offset;
            LayerMask = m_LayerMask;
        }
#endif

        public override void Reset()
        {
            base.Reset();
            Direction = new Vector3(0, -1, 0);
            MaxDistance = 100;
            Offset = 0;
            Warp = false;
            LayerMask = 0;
        }


        /*! \endcond */
        #endregion

        #region ### IOnRequestProcessing ###

        [Obsolete("IOnRequestPath.PathLength and CGDataRequestRasterization.SplineAbsoluteLength are no more needed. SplineInputModuleBase.getPathLength is used instead")]
        public float PathLength
        {
            get
            {
                if (OutPath.HasData)
                    return OutPath.GetData<CGPath>().Length;
                else
                    return (IsConfigured) ? InPath.SourceSlot().OnRequestPathModule.PathLength : 0;
            }
        }

        public bool PathIsClosed
        {
            get
            {
                return (IsConfigured) ? InPath.SourceSlot().PathProvider.PathIsClosed : false;
            }
        }

        public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
        {
            var raster = GetRequestParameter<CGDataRequestRasterization>(ref requests);
            if (!raster)
                return null;

            if(LayerMask == 0)//0 is Nothing
                UIMessages.Add("Please set a Layer Mask different than Nothing.");

            var Data = InPath.GetData<CGPath>(requests);
            return new CGData[1] { Conform(Generator.transform, Data, LayerMask, Direction, Offset, MaxDistance, Warp) };
        }

        /// <summary>
        /// Confirms a path by projecting it on top of objects (with a collider) of a specific layer
        /// </summary>
        /// <param name="pathTransform"></param>
        /// <param name="path"></param>
        /// <param name="layers"></param>
        /// <param name="projectionDirection"></param>
        /// <param name="offset"></param>
        /// <param name="rayLength"></param>
        /// <param name="warp">If true, the projected path will keep its shape</param>
        /// <returns></returns>
        public static CGPath Conform(Transform pathTransform, CGPath path, LayerMask layers, Vector3 projectionDirection, float offset, float rayLength, bool warp)
        {
            if (path == null)
                return null;

            int pathCount = path.Count;
            if (projectionDirection != Vector3.zero && rayLength > 0 && pathCount > 0)
            {
                RaycastHit raycastHit;

                if (warp)
                {
                    float minDist = float.MaxValue;

                    for (int i = 0; i < pathCount; i++)
                        if (Physics.Raycast(pathTransform.TransformPoint(path.Position[i]), projectionDirection, out raycastHit, rayLength, layers))
                            if (raycastHit.distance < minDist)
                                minDist = raycastHit.distance;
                    if (minDist != float.MaxValue)
                    {
                        Vector3 positionTranslation = projectionDirection * (minDist + offset);
                        for (int i = 0; i < path.Count; i++)
                            path.Position[i] += positionTranslation;
                    }
                    //path.Recalculate();
                }
                else
                {


                    for (int i = 0; i < pathCount; i++)
                        if (Physics.Raycast(pathTransform.TransformPoint(path.Position[i]), projectionDirection, out raycastHit, rayLength, layers))
                            path.Position[i] += projectionDirection * (raycastHit.distance + offset);
                    //path.Recalculate();

                }
            }
            return path;
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */


        /*! \endcond */
        #endregion



    }
}
