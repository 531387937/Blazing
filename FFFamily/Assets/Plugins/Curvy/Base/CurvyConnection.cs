// =====================================================================
// Copyright 2013-2018 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Connection component
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL(CurvySpline.DOCLINK + "curvyconnection")]
    public class CurvyConnection : MonoBehaviour, ISerializationCallbackReceiver
    {
        #region ### Serialized Fields ###

        [SerializeField, Hide]
        List<CurvySplineSegment> m_ControlPoints = new List<CurvySplineSegment>();

        #endregion

        #region ### Private Fields ###

        private ReadOnlyCollection<CurvySplineSegment> readOnlyControlPoints;
        /// <summary>
        /// The coordinates of the connection the last time synchronisation was processed
        /// </summary>
        Couple<Vector3, Quaternion> processedConnectionCoordinates;
        /// <summary>
        /// The coordinates of the connection's control points the last time synchronisation was processed
        /// </summary>
        Dictionary<CurvySplineSegment, Couple<Vector3, Quaternion>> processedControlPointsCoordinates = new Dictionary<CurvySplineSegment, Couple<Vector3, Quaternion>>();

        #endregion

        #region ### Public Properties ###


        /// <summary>
        /// The list of connected control points
        /// </summary>
        public ReadOnlyCollection<CurvySplineSegment> ControlPointsList
        {
            //TODO apply the same TODOs than CurvySpline.ControlPointsList
            get
            {
                if (readOnlyControlPoints == null)
                    readOnlyControlPoints = m_ControlPoints.AsReadOnly();
                return readOnlyControlPoints;
            }
        }
        /// <summary>
        /// Gets the number of Control Points being part of this connection
        /// </summary>
        public int Count
        {
            get { return m_ControlPoints.Count; }
        }

        /// <summary>
        /// Gets a certain Control Point by index
        /// </summary>
        /// <param name="idx">index of the Control Point</param>
        /// <returns>a Control Point</returns>
        public CurvySplineSegment this[int idx]
        {
            get
            {
                return m_ControlPoints[idx];
            }
        }

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */



        void OnEnable()
        {
            Transform cachedTransform = transform;
            processedConnectionCoordinates = new Couple<Vector3, Quaternion>(cachedTransform.position, cachedTransform.rotation);
            processedControlPointsCoordinates.Clear();
            for (int index = 0; index < m_ControlPoints.Count; index++)
            {
                CurvySplineSegment controlPoint = m_ControlPoints[index];
                processedControlPointsCoordinates[controlPoint] = new Couple<Vector3, Quaternion>(controlPoint.transform.position, controlPoint.transform.rotation);
            }
#if UNITY_EDITOR
            EditorApplication.update += EditorUpdate;
#endif
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }

#if UNITY_EDITOR
        void EditorUpdate()
        {
            DoUpdate();
        }
#endif


        void Update()
        {
            if (Application.isPlaying)
                DoUpdate();
        }
        void LateUpdate()
        {
            if (Application.isPlaying)
                DoUpdate();
        }
        void FixedUpdate()
        {
            if (Application.isPlaying)
                DoUpdate();
        }

        void OnDestroy()
        {
            bool realDestroy = true;
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                realDestroy = false;
#endif
            if (realDestroy)
            {
                foreach (CurvySplineSegment cp in m_ControlPoints)
                    cp.ResetConnectionRelatedData();
                m_ControlPoints.Clear();
                processedControlPointsCoordinates.Clear();
            }
        }


        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Creates a connection and adds Control Points
        /// </summary>
        /// <param name="controlPoints">Control Points to add</param>
        /// <returns>the new connection</returns>
        public static CurvyConnection Create(params CurvySplineSegment[] controlPoints)
        {
            CurvyGlobalManager curvyGlobalManager = CurvyGlobalManager.Instance;
            if (curvyGlobalManager == null)
            {
                DTLog.LogError("[Curvy] Couldn't find Curvy Global Manager. Please raise a bug report.");
                return null;
            }

            CurvyConnection con = curvyGlobalManager.AddChildGameObject<CurvyConnection>("Connection");
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RegisterCreatedObjectUndo(con.gameObject, "Add Connection");
#endif
            if (!con)
                return null;
            if (controlPoints.Length > 0)
            {
                con.transform.position = controlPoints[0].transform.position;
                con.AddControlPoints(controlPoints);
            }

            return con;
        }

        /// <summary>
        /// Adds Control Points to this connection
        /// </summary>
        /// <param name="controlPoints">the Control Points to add</param>
        public void AddControlPoints(params CurvySplineSegment[] controlPoints)
        {
            foreach (CurvySplineSegment cp in controlPoints)
            {
                if (!cp.Connection)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        Undo.RecordObject(cp, "Add Connection");
#endif
#if CURVY_SANITY_CHECKS
                    Assert.IsFalse(m_ControlPoints.Contains(cp));
#endif
                    m_ControlPoints.Add(cp);
                    processedControlPointsCoordinates[cp] = new Couple<Vector3, Quaternion>(cp.transform.position, cp.transform.rotation);
                    cp.Connection = this;
                }
            }
            AutoSetFollowUp();
        }

        public void AutoSetFollowUp()
        {
            if (Count == 2)
            {
                CurvySplineSegment firstControlPoint = m_ControlPoints[0];
                CurvySplineSegment secondControlPoint = m_ControlPoints[1];
                if (firstControlPoint.transform.position == secondControlPoint.transform.position && firstControlPoint.ConnectionSyncPosition && secondControlPoint.ConnectionSyncPosition)
                {
                    if (firstControlPoint.FollowUp == null && firstControlPoint.Spline && firstControlPoint.Spline.CanControlPointHaveFollowUp(firstControlPoint))
                        firstControlPoint.SetFollowUp(secondControlPoint);
                    if (secondControlPoint.FollowUp == null && secondControlPoint.Spline && secondControlPoint.Spline.CanControlPointHaveFollowUp(secondControlPoint))
                        secondControlPoint.SetFollowUp(firstControlPoint);
                }
            }
        }

        /// <summary>
        /// Removes a Control Point from this connection
        /// </summary>
        /// <param name="controlPoint">the Control Point to remove</param>
        /// <param name="destroySelfIfEmpty">whether the connection should be destroyed when empty afterwards</param>
        public void RemoveControlPoint(CurvySplineSegment controlPoint, bool destroySelfIfEmpty = true)
        {
            controlPoint.Connection = null;
            m_ControlPoints.Remove(controlPoint);
            processedControlPointsCoordinates.Remove(controlPoint);
            if (m_ControlPoints.Count == 0 && destroySelfIfEmpty)
                Delete();
        }

        /// <summary>
        /// Deletes the connection
        /// </summary>
        public void Delete()
        {
            if (Application.isPlaying)
            {
                GameObject.Destroy(this.gameObject);
            }
#if UNITY_EDITOR
            else
                Undo.DestroyObjectImmediate(this.gameObject);
#endif
        }

        /// <summary>
        /// Gets all Control Points except the one provided
        /// </summary>
        /// <param name="source">the Control Point to filter out</param>
        /// <returns>list of Control Points</returns>
        public List<CurvySplineSegment> OtherControlPoints(CurvySplineSegment source)
        {
            List<CurvySplineSegment> res = new List<CurvySplineSegment>(m_ControlPoints);
            res.Remove(source);
            return res;
        }

        /// <summary>
        /// Synchronise all the connected control points to match the given position and rotation, based on their synchronisation options. Will update the CurvyConnection's game object's transform too.
        /// </summary>
        /// <remarks>Can dirty the splines of the updated control points</remarks>
        public void SetSynchronisationPositionAndRotation(Vector3 referencePosition, Quaternion referenceRotation)
        {
            Transform cachedTransform = transform;

            cachedTransform.position = referencePosition;
            cachedTransform.rotation = referenceRotation;
            cachedTransform.hasChanged = false;
            processedConnectionCoordinates.First = referencePosition;
            processedConnectionCoordinates.Second = referenceRotation;

            for (int i = 0; i < m_ControlPoints.Count; i++)
            {
                CurvySplineSegment controlPoint = m_ControlPoints[i];

                bool positionModified = controlPoint.ConnectionSyncPosition && controlPoint.transform.position.NotApproximately(referencePosition);
                bool rotationModified = controlPoint.ConnectionSyncRotation && controlPoint.transform.rotation.DifferentOrientation(referenceRotation);

                if (positionModified)
                    controlPoint.transform.position = referencePosition;
                if (rotationModified)
                    controlPoint.transform.rotation = referenceRotation;

                Couple<Vector3, Quaternion> processedControlPointCoordinates = processedControlPointsCoordinates[controlPoint];
                processedControlPointCoordinates.First = controlPoint.transform.position;
                processedControlPointCoordinates.Second = controlPoint.transform.rotation;

                if (positionModified || (rotationModified && controlPoint.OrientatinInfluencesSpline))
                    controlPoint.Spline.SetDirtyPartial(controlPoint
                        , positionModified == false ? SplineDirtyingType.OrientationOnly : SplineDirtyingType.Everything);
            }
        }


#if UNITY_EDITOR
        /// <summary>
        /// Gets the gizmo color based on the synchronization options of the connected control points
        /// </summary>
        public Color GetGizmoColor()
        {
            Color gizmoColor;

            if (ControlPointsList.Count == 0)
                gizmoColor = Color.black;
            else
            {
                bool allPositionsSynced = true;
                bool allRotationsSynced = true;
                foreach (CurvySplineSegment controlPoint in ControlPointsList)
                {
                    allPositionsSynced = allPositionsSynced && controlPoint.ConnectionSyncPosition;
                    allRotationsSynced = allRotationsSynced && controlPoint.ConnectionSyncRotation;

                    if (allPositionsSynced == false && allRotationsSynced == false)
                        break;
                }

                if (allPositionsSynced)
                    gizmoColor = allRotationsSynced
                        ? Color.white
                        : new Color(255 / 255f, 49 / 255f, 38 / 255f);
                else if (allRotationsSynced)
                    gizmoColor = new Color(1, 1, 0);
                else
                    gizmoColor = Color.black;
            }

            return gizmoColor;
        }
#endif


        #endregion

        #region ### Privates & Internals ###
        /*! \cond PRIVATE */

        void DoUpdate()
        {
            Transform cachedTransform = transform;

            bool synchronised;
            if (cachedTransform.hasChanged)
            {
                cachedTransform.hasChanged = false;
                if (cachedTransform.position.NotApproximately(processedConnectionCoordinates.First) ||
                    cachedTransform.rotation.DifferentOrientation(processedConnectionCoordinates.Second))
                {
                    SetSynchronisationPositionAndRotation(cachedTransform.position, cachedTransform.rotation);
                    synchronised = true;
                }
                else
                    synchronised = false;
            }
            else
                synchronised = false;

            if (synchronised == false)
            {
                Vector3? synchronisationPosition = null;
                Quaternion? synchronisationRotation = null;

                foreach (CurvySplineSegment controlPoint in m_ControlPoints)
                {
                    Couple<Vector3, Quaternion> processedControlPointsCoordinate = processedControlPointsCoordinates[controlPoint];
                    Transform controlPointTransform = controlPoint.transform;

                    if (controlPoint.ConnectionSyncPosition && controlPointTransform.position.NotApproximately(processedControlPointsCoordinate.First))
                        synchronisationPosition = controlPointTransform.position;

                    if (controlPoint.ConnectionSyncRotation && controlPointTransform.rotation.DifferentOrientation(processedControlPointsCoordinate.Second))
                        synchronisationRotation = controlPointTransform.rotation;

                    if (synchronisationPosition != null && synchronisationRotation != null)
                        break;
                }

                if (synchronisationPosition != null || synchronisationRotation != null)
                    SetSynchronisationPositionAndRotation(synchronisationPosition ?? transform.position, synchronisationRotation ?? transform.rotation);
            }
        }

        /*! \endcond */
        #endregion

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize()
        {
            m_ControlPoints.RemoveAll(cp => ReferenceEquals(cp, null));
        }

        public void OnAfterDeserialize()
        {
            m_ControlPoints.RemoveAll(cp => ReferenceEquals(cp, null));
        }

        #endregion

    }

}
