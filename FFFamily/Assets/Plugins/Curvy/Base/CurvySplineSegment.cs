// =====================================================================
// Copyright 2013-2018 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Utils;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine.Serialization;
using System.Reflection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#endif
using JetBrains.Annotations;
using UnityEngine.Assertions;


namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Class covering a Curvy Spline Segment / ControlPoint
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL(CurvySpline.DOCLINK + "curvysplinesegment")]
    public partial class CurvySplineSegment : MonoBehaviour, IPoolable
    {
        /// <summary>
        /// The color used in Gizmos to draw a segment's tangents
        /// </summary>
        public static readonly Color GizmoTangentColor = new Color(0, 0.7f, 0);

        #region ### Public Properties ###

        /// <summary>
        /// List of precalculated interpolations
        /// </summary>
        /// <remarks>Based on Spline's CacheDensity</remarks>
        [NonSerialized]
        public Vector3[] Approximation = new Vector3[0];

        /// <summary>
        /// List of precalculated distances
        /// </summary>
        /// <remarks>Based on Spline's CacheDensity</remarks>
        [NonSerialized]
        public float[] ApproximationDistances = new float[0];

        /// <summary>
        /// List of precalculated Up-Vectors
        /// </summary>
        /// <remarks>Based on Spline's CacheDensity</remarks>
        [NonSerialized]
        public Vector3[] ApproximationUp = new Vector3[0];

        /// <summary>
        /// List of precalculated Tangent-Normals
        /// </summary>
        /// <remarks>Based on Spline's CacheDensity</remarks>
        [NonSerialized]
        public Vector3[] ApproximationT = new Vector3[0];

        /// <summary>
        /// If set, Control Point's rotation will be set to the calculated Up-Vector3
        /// </summary>
        /// <remarks>This is particularly useful when connecting splines</remarks>
        public bool AutoBakeOrientation
        {
            get { return m_AutoBakeOrientation; }
            set
            {
                if (m_AutoBakeOrientation != value)
                {
                    m_AutoBakeOrientation = value;
                }
            }
        }

        /// <summary>
        /// The serialized value of OrientationAnchor. This value is ignored in some cases (invisible control points, first and last visible control points). Use Spline.IsOrientationAnchor() to get the correct value.
        /// </summary>
        public bool SerializedOrientationAnchor
        {
            get { return m_OrientationAnchor; }
            set
            {
                if (m_OrientationAnchor != value)
                {
                    m_OrientationAnchor = value;
                    Spline.SetDirty(this, SplineDirtyingType.OrientationOnly);
                    Spline.InvalidateControlPointsRelationshipCacheINTERNAL();
                }
            }
        }

        /// <summary>
        /// Swirling Mode
        /// </summary>
        public CurvyOrientationSwirl Swirl
        {
            get { return m_Swirl; }
            set
            {
                if (m_Swirl != value)
                {
                    m_Swirl = value;
                    Spline.SetDirty(this, SplineDirtyingType.OrientationOnly);
                }
            }
        }

        /// <summary>
        /// Turns to swirl
        /// </summary>
        public float SwirlTurns
        {
            get { return m_SwirlTurns; }
            set
            {
                if (m_SwirlTurns != value)
                {
                    m_SwirlTurns = value;
                    Spline.SetDirty(this, SplineDirtyingType.OrientationOnly);
                }
            }
        }

        #region --- Bezier ---
        //TODO Make sure that every place in the code setting Handles respects the constraints of Sync length, Sync direction and Sync connections

        /// <summary>
        /// Left B-Spline Handle in local coordinates
        /// </summary>
        public Vector3 HandleIn
        {
            get
            { return m_HandleIn; }
            set
            {
                if (m_HandleIn != value)
                {
                    m_HandleIn = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Right B-Spline Handle in local coordinates
        /// </summary>
        public Vector3 HandleOut
        {
            get { return m_HandleOut; }
            set
            {
                if (m_HandleOut != value)
                {
                    m_HandleOut = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        //BUG this doesn't handle scaled splines. Go through all similar situations and fix them, or add as a constraint that scaled splines should be normalized (scale set back to 1) before doing any operations on them
        /// <summary>
        /// Left B-Spline Handle in world coordinates
        /// </summary>
        public Vector3 HandleInPosition
        {
            get
            {
                return transform.position + Spline.transform.rotation * HandleIn;
            }
            set
            {
                HandleIn = Spline.transform.InverseTransformDirection(value - transform.position);
            }
        }

        /// <summary>
        /// Right B-Spline Handle in world coordinates
        /// </summary>
        public Vector3 HandleOutPosition
        {
            get
            {
                return transform.position + Spline.transform.rotation * HandleOut;
            }
            set
            {
                HandleOut = Spline.transform.InverseTransformDirection(value - transform.position);
            }
        }
        /// <summary>
        /// Gets or Sets Auto Handles. When setting it the value of connected control points is also updated
        /// </summary>
        public bool AutoHandles
        {
            get { return m_AutoHandles; }
            set
            {
                if (SetAutoHandles(value))
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
            }
        }

        public float AutoHandleDistance
        {
            get { return m_AutoHandleDistance; }
            set
            {
                if (m_AutoHandleDistance != value)
                {
                    float clampedDistance = Mathf.Clamp01(value);
                    if (m_AutoHandleDistance != clampedDistance)
                    {
                        m_AutoHandleDistance = clampedDistance;
                        Spline.SetDirty(this, SplineDirtyingType.Everything);
                    }
                }
            }
        }

        #endregion

        #region --- TCB ---

        /// <summary>
        /// Keep Start/End-TCB synchronized
        /// </summary>
        /// <remarks>Applies only to TCB Interpolation</remarks>
        public bool SynchronizeTCB
        {
            get { return m_SynchronizeTCB; }
            set
            {
                if (m_SynchronizeTCB != value)
                {
                    m_SynchronizeTCB = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Whether local Tension should be used
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Tension</remarks>
        public bool OverrideGlobalTension
        {
            get { return m_OverrideGlobalTension; }
            set
            {
                if (m_OverrideGlobalTension != value)
                {
                    m_OverrideGlobalTension = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Whether local Continuity should be used
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Continuity</remarks>
        public bool OverrideGlobalContinuity
        {
            get { return m_OverrideGlobalContinuity; }
            set
            {
                if (m_OverrideGlobalContinuity != value)
                {
                    m_OverrideGlobalContinuity = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Whether local Bias should be used
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Bias</remarks>
        public bool OverrideGlobalBias
        {
            get { return m_OverrideGlobalBias; }
            set
            {
                if (m_OverrideGlobalBias != value)
                {
                    m_OverrideGlobalBias = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Start Tension
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Tension</remarks>
        public float StartTension
        {
            get { return m_StartTension; }
            set
            {
                if (m_StartTension != value)
                {
                    m_StartTension = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Start Continuity
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Continuity</remarks>
        public float StartContinuity
        {
            get { return m_StartContinuity; }
            set
            {
                if (m_StartContinuity != value)
                {
                    m_StartContinuity = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Start Bias
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Bias</remarks>
        public float StartBias
        {
            get { return m_StartBias; }
            set
            {
                if (m_StartBias != value)
                {
                    m_StartBias = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// End Tension
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Tension</remarks>
        public float EndTension
        {
            get { return m_EndTension; }
            set
            {
                if (m_EndTension != value)
                {
                    m_EndTension = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// End Continuity
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Continuity</remarks>
        public float EndContinuity
        {
            get { return m_EndContinuity; }
            set
            {
                if (m_EndContinuity != value)
                {
                    m_EndContinuity = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// End Bias
        /// </summary>
        ///<remarks>This only applies to interpolation methods using Bias</remarks>
        public float EndBias
        {
            get { return m_EndBias; }
            set
            {
                if (m_EndBias != value)
                {
                    m_EndBias = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }


        #endregion
        /*
#region --- CG ---

        /// <summary>
        /// Material ID (used by PCG)
        /// </summary>
        public int CGMaterialID
        {
            get
            {
                return m_CGMaterialID;
            }
            set
            {
                if (m_CGMaterialID != Mathf.Max(0, value))
                    m_CGMaterialID = Mathf.Max(0, value);
            }
        }

        /// <summary>
        /// Whether to create a hard edge or not (used by PCG)
        /// </summary>
        public bool CGHardEdge
        {
            get { return m_CGHardEdge; }
            set
            {
                if (m_CGHardEdge != value)
                    m_CGHardEdge = value;
            }
        }
        /// <summary>
        /// Maximum vertex distance when using optimization (0=infinite)
        /// </summary>
        public float CGMaxStepDistance
        {
            get
            {
                return m_CGMaxStepDistance;
            }
            set
            {
                if (m_CGMaxStepDistance != Mathf.Max(0, value))
                    m_CGMaxStepDistance = Mathf.Max(0, value);
            }
        }

#endregion
        */
        #region --- Connections ---
        /// <summary>
        /// Gets the connected Control Point that is set as "Head To"
        /// </summary>
        public CurvySplineSegment FollowUp
        {
            get
            {
                return m_FollowUp;
            }
            private set
            {
                if (m_FollowUp != value)
                {
                    m_FollowUp = value;
                    if (mSpline != null)
                        mSpline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }


        /// <summary>
        /// Gets or sets the heading toward the "Head To" segment
        /// </summary>
        public ConnectionHeadingEnum FollowUpHeading
        {
            get { return m_FollowUpHeading; }
            set
            {
                if (m_FollowUpHeading != value)
                {
                    m_FollowUpHeading = value;
                    if (mSpline != null)
                        mSpline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// When part of a <see cref="CurvyConnection"/>, this defines whether the connection's position is applied to this Control Point
        /// The synchronization process is applied by <see cref="CurvyConnection"/> at each frame in its update methods. So if you modify the value of this property, and want the synchronization to happen right away, you will have to call the connection's <see cref="CurvyConnection.SetSynchronisationPositionAndRotation(Vector3, Quaternion)"/> with the connection's position and rotation as parameters
        /// </summary>
        public bool ConnectionSyncPosition
        {
            get { return m_ConnectionSyncPosition; }
            set
            {
                if (m_ConnectionSyncPosition != value)
                {
                    m_ConnectionSyncPosition = value;
                    //DESIGN think about removing the code that handles ConnectionSyncPosition and ConnectionSyncRotation, and replace it with a code that always runs in Refresh, and make that code happen by calling SetDirty() here;
                }
            }
        }

        /// <summary>
        /// When part of a <see cref="CurvyConnection"/>, this defines whether the connection's rotation is applied to this Control Point
        /// The synchronization process is applied by <see cref="CurvyConnection"/> at each frame in its update methods. So if you modify the value of this property, and want the synchronization to happen right away, you will have to call the connection's <see cref="CurvyConnection.SetSynchronisationPositionAndRotation(Vector3, Quaternion)"/> with the connection's position and rotation as parameters
        /// </summary>
        public bool ConnectionSyncRotation
        {
            get { return m_ConnectionSyncRotation; }
            set
            {
                if (m_ConnectionSyncRotation != value)
                {
                    m_ConnectionSyncRotation = value;
                    //DESIGN think about removing the code that handles ConnectionSyncPosition and ConnectionSyncRotation, and replace it with a code that always runs in Refresh, and make that code happen by calling SetDirty() here;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the connection handler this Control Point is using (if any)
        /// </summary>
        /// <remarks>If set to null, FollowUp wil be set to null to</remarks>
        public CurvyConnection Connection
        {
            get { return m_Connection; }
            internal set
            {
                if (SetConnection(value))
                    if (mSpline != null)
                        mSpline.SetDirty(this, SplineDirtyingType.Everything);
            }
        }

        #endregion


        /// <summary>
        /// Gets the number of individual cache points of this segment
        /// </summary>
        /// <remarks>The actual approximations arrays' size is CacheSize + 1</remarks>
        public int CacheSize
        {
            get
            {
#if CURVY_SANITY_CHECKS
                Assert.IsTrue(cacheSize >= 0, "[Curvy] CurvySplineSegment has unintialized cache");
#endif
                return cacheSize;
            }
            private set
            {
#if CONTRACTS_FULL
                Contract.Requires(value >= 0);
#endif
                cacheSize = value;
            }
        }

        /// <summary>
        /// Gets this segment's bounds in world space
        /// </summary>
        public Bounds Bounds
        {
            get
            {
                if (!mBounds.HasValue)
                {
                    Bounds result;
                    if (Approximation.Length == 0)
                        result = new Bounds(transform.position, Vector3.zero);
                    else
                    {
                        Matrix4x4 mat = Spline.transform.localToWorldMatrix;
                        result = new Bounds(mat.MultiplyPoint3x4(Approximation[0]), Vector3.zero);
                        int u = Approximation.Length;
                        for (int i = 1; i < u; i++)
                            result.Encapsulate(mat.MultiplyPoint(Approximation[i]));
                    }

                    mBounds = result;
                }
                return mBounds.Value;
            }
        }

        /// <summary>
        /// Gets the length of this spline segment
        /// </summary>
        public float Length { get; private set; }

        /// <summary>
        /// Gets the distance from spline start to the first control point (localF=0) 
        /// </summary>
        public float Distance { get; internal set; }

        /// <summary>
        /// Gets the TF of this Control Point
        /// </summary>
        /// <remarks>This is a shortcut to LocalFToTF(0)</remarks>
        public float TF
        {
            get { return LocalFToTF(0); }
        }

        /// <summary>
        /// Gets whether this Control Point is the first IGNORING closed splines
        /// </summary>
        public bool IsFirstControlPoint
        {
            get
            {
                return (Spline.GetControlPointIndex(this) == 0);
            }
        }

        /// <summary>
        /// Gets whether this Control Point is the last IGNORING closed splines
        /// </summary>
        public bool IsLastControlPoint
        {
            get
            {
                return (Spline.GetControlPointIndex(this) == Spline.ControlPointCount - 1);
            }
        }

        /// <summary>
        /// The Metadata components added to this GameObject
        /// </summary>
        [Obsolete("Use MetaDataSet instead")]
        public List<Component> MetaData
        {
            get
            {
                return MetaDataSet.ToList();
            }
        }

        /// <summary>
        /// The Metadata components added to this GameObject
        /// </summary>
        public HashSet<Component> MetaDataSet
        {
            get
            {
                return mMetaData;
            }
        }

        /// <summary>
        /// Gets the parent spline
        /// </summary>
        public CurvySpline Spline
        {
            get
            {
                return mSpline;
            }
        }


        /// <summary>
        /// Returns true if the local position is different than the last one used in the segment approximations cache computation
        /// </summary>
        public bool HasUnprocessedLocalPosition { get { return transform.localPosition != lastProcessedLocalPosition; } }
        /// <summary>
        /// Returns true if the local orientation is different than the last one used in the segment approximations cache computation
        /// </summary>
        public bool HasUnprocessedLocalOrientation { get { return transform.localRotation.DifferentOrientation(lastProcessedLocalRotation); } }
        /// <summary>
        /// Returns wheter the orientation of this Control Point influences the orientation of its containing spline's approximation points.
        /// Returns false if control point is not part of a spline
        /// </summary>
        public bool OrientatinInfluencesSpline { get { return mSpline != null && (mSpline.Orientation == CurvyOrientation.Static || mSpline.IsControlPointAnOrientationAnchor(this)); } }

        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Sets Bezier HandleIn
        /// </summary>
        /// <param name="position">HandleIn position</param>
        /// <param name="space">Local or world space</param>
        /// <param name="mode">Handle synchronization mode</param>
        public void SetBezierHandleIn(Vector3 position, Space space = Space.Self, CurvyBezierModeEnum mode = CurvyBezierModeEnum.None)
        {
            if (space == Space.Self)
                HandleIn = position;
            else
                HandleInPosition = position;

            bool syncDirections = (mode & CurvyBezierModeEnum.Direction) == CurvyBezierModeEnum.Direction;
            bool syncLengths = (mode & CurvyBezierModeEnum.Length) == CurvyBezierModeEnum.Length;
            bool syncConnectedCPs = (mode & CurvyBezierModeEnum.Connections) == CurvyBezierModeEnum.Connections;

            if (syncDirections)
                HandleOut = HandleOut.magnitude * (HandleIn.normalized * -1);
            if (syncLengths)
                HandleOut = HandleIn.magnitude * ((HandleOut == Vector3.zero) ? HandleIn.normalized * -1 : HandleOut.normalized);
            if (Connection && syncConnectedCPs && (syncDirections || syncLengths))
            {
                ReadOnlyCollection<CurvySplineSegment> connectionControlPoints = Connection.ControlPointsList;
                for (int index = 0; index < connectionControlPoints.Count; index++)
                {
                    CurvySplineSegment connectedCp = connectionControlPoints[index];
                    if (connectedCp == this)
                        continue;

                    if (connectedCp.HandleIn.magnitude == 0)
                        connectedCp.HandleIn = HandleIn;

                    if (syncDirections)
                        connectedCp.SetBezierHandleIn(connectedCp.HandleIn.magnitude * HandleIn.normalized * Mathf.Sign(Vector3.Dot(HandleIn, connectedCp.HandleIn)), Space.Self, CurvyBezierModeEnum.Direction);
                    if (syncLengths)
                        connectedCp.SetBezierHandleIn(connectedCp.HandleIn.normalized * HandleIn.magnitude, Space.Self, CurvyBezierModeEnum.Length);
                }
            }
        }

        /// <summary>
        /// Sets Bezier HandleOut
        /// </summary>
        /// <param name="position">HandleOut position</param>
        /// <param name="space">Local or world space</param>
        /// <param name="mode">Handle synchronization mode</param>
        public void SetBezierHandleOut(Vector3 position, Space space = Space.Self, CurvyBezierModeEnum mode = CurvyBezierModeEnum.None)
        {
            if (space == Space.Self)
                HandleOut = position;
            else
                HandleOutPosition = position;

            bool syncDirections = (mode & CurvyBezierModeEnum.Direction) == CurvyBezierModeEnum.Direction;
            bool syncLengths = (mode & CurvyBezierModeEnum.Length) == CurvyBezierModeEnum.Length;
            bool syncConnectedCPs = (mode & CurvyBezierModeEnum.Connections) == CurvyBezierModeEnum.Connections;

            if (syncDirections)
                HandleIn = HandleIn.magnitude * (HandleOut.normalized * -1);
            if (syncLengths)
                HandleIn = HandleOut.magnitude * ((HandleIn == Vector3.zero) ? HandleOut.normalized * -1 : HandleIn.normalized);

            if (Connection && syncConnectedCPs && (syncDirections || syncLengths))
            {
                for (int index = 0; index < (Connection.ControlPointsList).Count; index++)
                {
                    CurvySplineSegment connectedCp = (Connection.ControlPointsList)[index];
                    if (connectedCp == this)
                        continue;

                    if (connectedCp.HandleOut.magnitude == 0)
                        connectedCp.HandleOut = HandleOut;

                    if (syncDirections)
                        connectedCp.SetBezierHandleOut(connectedCp.HandleOut.magnitude * HandleOut.normalized * Mathf.Sign(Vector3.Dot(HandleOut, connectedCp.HandleOut)), Space.Self, CurvyBezierModeEnum.Direction);
                    if (syncLengths)
                        connectedCp.SetBezierHandleOut(connectedCp.HandleOut.normalized * HandleOut.magnitude, Space.Self, CurvyBezierModeEnum.Length);
                }
            }
        }

        /// <summary>
        /// Automatically place Bezier handles relative to neighbour Control Points
        /// </summary>
        /// <param name="distanceFrag">how much % distance between neighbouring CPs are applied to the handle length?</param>
        /// <param name="setIn">Set HandleIn?</param>
        /// <param name="setOut">Set HandleOut?</param>
        /// <param name="noDirtying">If true, the Bezier handles will be modified without dirtying any spline</param>
        public void SetBezierHandles(float distanceFrag = -1, bool setIn = true, bool setOut = true, bool noDirtying = false)
        {
            Vector3 pIn = Vector3.zero;
            Vector3 pOut = Vector3.zero;
            if (distanceFrag == -1)
                distanceFrag = AutoHandleDistance;
            if (distanceFrag > 0)
            {
                CurvySpline spline = Spline;
                Transform cachedTransform = transform;

                CurvySplineSegment nextControlPoint = spline.GetNextControlPoint(this);
                Transform nextTt = nextControlPoint
                    ? nextControlPoint.transform
                    : cachedTransform;
                CurvySplineSegment previousControlPoint = spline.GetPreviousControlPoint(this);
                Transform previousTt = previousControlPoint
                    ? previousControlPoint.transform
                    : cachedTransform;


                Vector3 c = cachedTransform.localPosition;
                Vector3 p = previousTt.localPosition - c;
                Vector3 n = nextTt.localPosition - c;
                SetBezierHandles(distanceFrag, p, n, setIn, setOut, noDirtying);
            }
            else
            {
                // Fallback to zero
                if (setIn)
                    if (noDirtying)
                        m_HandleIn = pIn;
                    else
                        HandleIn = pIn;

                if (setOut)
                    if (noDirtying)
                        m_HandleOut = pOut;
                    else
                        HandleOut = pOut;
            }

        }

        /// <summary>
        /// Automatically place Bezier handles
        /// </summary>
        /// <param name="distanceFrag">how much % distance between neighbouring CPs are applied to the handle length?</param>
        /// <param name="p">Position the In-Handle relates to</param>
        /// <param name="n">Position the Out-Handle relates to</param>
        /// <param name="setIn">Set HandleIn?</param>
        /// <param name="setOut">Set HandleOut?</param>
        /// <param name="noDirtying">If true, the Bezier handles will be modified without dirtying any spline</param>
        public void SetBezierHandles(float distanceFrag, Vector3 p, Vector3 n, bool setIn = true, bool setOut = true, bool noDirtying = false)
        {
            float pLen = p.magnitude;
            float nLen = n.magnitude;
            Vector3 pIn = Vector3.zero;
            Vector3 pOut = Vector3.zero;

            if (pLen != 0 || nLen != 0)
            {
                Vector3 dir = ((pLen / nLen) * n - p).normalized;
                pIn = -dir * (pLen * distanceFrag);
                pOut = dir * (nLen * distanceFrag);
            }

            // Fallback to zero
            if (setIn)
                if (noDirtying)
                    m_HandleIn = pIn;
                else
                    HandleIn = pIn;

            if (setOut)
                if (noDirtying)
                    m_HandleOut = pOut;
                else
                    HandleOut = pOut;
        }


        /// <summary>
        /// Sets Follow-Up of this Control Point
        /// </summary>
        /// <param name="target">the Control Point to follow to</param>
        /// <param name="heading">the Heading on the target's spline</param>
        public void SetFollowUp(CurvySplineSegment target, ConnectionHeadingEnum heading = ConnectionHeadingEnum.Auto)
        {
#if CONTRACTS_FULL
            Contract.Requires(Spline);
            Contract.Requires(Spline.CanHaveFollowUp(this) || target == null);
#endif
            if (target == null || Spline.CanControlPointHaveFollowUp(this))
            {
                FollowUp = target;
                FollowUpHeading = heading;
            }
            else
                DTLog.LogError("[Curvy] Setting a Follow-Up to a Control Point that can't have one");
        }

        /// <summary>
        /// Resets the connections related data (Connection, FollowUp, etc) while updating the Connection object and dirtying relevant splines.
        /// </summary>
        public void Disconnect()
        {
            if (Connection)
                Connection.RemoveControlPoint(this);

            ResetConnectionRelatedData();
        }

        /// <summary>
        /// Resets the connections related data (Connection, FollowUp, etc)
        /// </summary>
        public void ResetConnectionRelatedData()
        {
            //TODO CONTRACT add checks (code contracts?) that verify the consistency of the members related to Connection and FollowUp
            Connection = null;
            FollowUp = null;
            FollowUpHeading = ConnectionHeadingEnum.Auto;
            ConnectionSyncPosition = false;
            ConnectionSyncRotation = false;
        }
        /// <summary>
        /// Interpolates position for a local F
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 Interpolate(float localF) { return Interpolate(localF, Spline.Interpolation); }

        /// <summary>
        /// Interpolates position for a local F
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <param name="interpolation">the interpolation to use</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 Interpolate(float localF, CurvyInterpolation interpolation)//TODO rename to overridenInterpolation?
        {
            Vector3 result;
            localF = Mathf.Clamp01(localF);
            CurvySplineSegment nextControlPoint = mSpline.GetNextControlPoint(this);
            Vector3 nextControlPointLocalPosition = nextControlPoint.transform.localPosition;
            Vector3 localPosition = transform.localPosition;

            //If you modify this, modify also the inlined version of this method in refreshCurveINTERNAL()
            switch (interpolation)
            {
                case CurvyInterpolation.Bezier:
                    result = CurvySpline.Bezier(localPosition + HandleOut,
                        localPosition,
                        nextControlPointLocalPosition,
                        nextControlPointLocalPosition + nextControlPoint.HandleIn,
                        localF);
                    break;
                case CurvyInterpolation.CatmullRom:
                case CurvyInterpolation.TCB:
                    {

                        CurvySplineSegment previousControlPoint = mSpline.GetPreviousControlPointUsingFollowUp(this);
                        CurvySplineSegment nextNextControlPoint = nextControlPoint.Spline.GetNextControlPointUsingFollowUp(nextControlPoint);
                        Vector3 previousPosition = previousControlPoint
                            ? previousControlPoint.transform.localPosition
                            : localPosition;
                        Vector3 nextNextPosition = nextNextControlPoint
                            ? nextNextControlPoint.transform.localPosition
                            : nextControlPointLocalPosition;

                        if (interpolation == CurvyInterpolation.TCB)
                        {
#if CONTRACTS_FULL
            Contract.Requires(Spline.IsSegment(this));
#endif
                            float t0 = StartTension; float t1 = EndTension;
                            float c0 = StartContinuity; float c1 = EndContinuity;
                            float b0 = StartBias; float b1 = EndBias;

                            if (!OverrideGlobalTension)
                                t0 = t1 = mSpline.Tension;
                            if (!OverrideGlobalContinuity)
                                c0 = c1 = mSpline.Continuity;
                            if (!OverrideGlobalBias)
                                b0 = b1 = mSpline.Bias;

                            result = CurvySpline.TCB(previousPosition,
                                localPosition,
                                nextControlPointLocalPosition,
                                nextNextPosition,
                                localF, t0, c0, b0, t1, c1, b1);
                        }
                        else
                            result = CurvySpline.CatmullRom(previousPosition,
                                localPosition,
                                nextControlPointLocalPosition,
                                nextNextPosition,
                                localF);
                    }
                    break;
                case CurvyInterpolation.Linear:
                    result = Vector3.LerpUnclamped(
                        localPosition,
                        (nextControlPoint
                            ? nextControlPointLocalPosition
                            : localPosition),
                        localF);
                    break;
                default:
                    DTLog.LogError("[Curvy] Invalid interpolation value " + interpolation);
                    return Vector3.zero;
            }

            return result;
        }

        /// <summary>
        /// Interpolates position for a local F using a linear approximation
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 InterpolateFast(float localF)
        {
            float frag;
            int idx = getApproximationIndexINTERNAL(localF, out frag);
            int idx2 = Mathf.Min(Approximation.Length - 1, idx + 1);
            return (Vector3.LerpUnclamped(Approximation[idx], Approximation[idx2], frag));
        }

        #region MetaData handling

        /// <summary>
        /// Rebuilds <see cref="MetaDataSet"/>
        /// </summary>
        public void ReloadMetaData()
        {
            MetaDataSet.Clear();
#pragma warning disable 618
            Component[] metaDataComponents = GetComponents(typeof(ICurvyMetadata));
#pragma warning restore 618
            foreach (Component component in metaDataComponents)
                MetaDataSet.Add(component);

            CheckAgainstMetaDataDuplication();
        }

        /// <summary>
        /// Adds a MetaData instance to <see cref="MetaDataSet"/>
        /// </summary>
        public void RegisterMetaData(CurvyMetadataBase metaData)
        {
            MetaDataSet.Add(metaData);
            CheckAgainstMetaDataDuplication();
        }

        /// <summary>
        /// Removes a MetaData instance from <see cref="MetaDataSet"/>
        /// </summary>
        public void UnregisterMetaData(CurvyMetadataBase metaData)
        {
            MetaDataSet.Remove(metaData);
        }

        /// <summary>
        /// Gets Metadata
        /// </summary>
        /// <param name="type">type implementing ICurvyMetadata</param>
        /// <param name="autoCreate">whether to create the Metadata component if it's not present</param>
        /// <returns>the Metadata component or null</returns>
        [Obsolete("Use GetMetadata<T> instead")]
        public Component GetMetaData(Type type, bool autoCreate = false)
        {
#if NETFX_CORE
            if (type.GetTypeInfo().IsSubclassOf(typeof(Component)) && typeof(ICurvyMetadata).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
#else
            if (type.IsSubclassOf(typeof(Component)) && typeof(ICurvyMetadata).IsAssignableFrom(type))
#endif
            {
                foreach (Component component in MetaDataSet)
                    if (component != null && component.GetType() == type)
                        return component;
            }
            Component newData = null;
            if (autoCreate)
            {
                newData = gameObject.AddComponent(type);
                MetaDataSet.Add(newData);
            }
            return newData;
        }

        /// <summary>
        /// Gets Metadata of this ControlPoint
        /// </summary>
        /// <typeparam name="T">Metadata type</typeparam>
        /// <param name="autoCreate">whether to create the Metadata component if it's not present</param>
        /// <returns>the Metadata component or null</returns>
#pragma warning disable 618
        public T GetMetadata<T>(bool autoCreate = false) where T : Component, ICurvyMetadata
#pragma warning restore 618
        {
#pragma warning disable 618
            return (T)GetMetaData(typeof(T), autoCreate);
#pragma warning restore 618
        }

        /// <summary>
        /// Gets an interpolated Metadata value for a certain F
        /// </summary>
        /// <typeparam name="T">Metadata type inheriting from CurvyInterpolatableMetadataBase</typeparam>
        /// <typeparam name="U">Metadata's Value type</typeparam>
        /// <param name="f">a local F in the range 0..1</param>
        /// <returns>The interpolated value. If no Metadata of specified type is present at the given tf, the default value of type U is returned</returns>
        public U GetInterpolatedMetadata<T, U>(float f) where T : CurvyInterpolatableMetadataBase<U>
        {
            T metaData = GetMetadata<T>();
            if (metaData != null)
            {
                CurvySplineSegment nextCp = Spline.GetNextControlPointUsingFollowUp(this);
                CurvyInterpolatableMetadataBase<U> nextMetaData = null;
                if (nextCp)
                    nextMetaData = nextCp.GetMetadata<T>();
                return metaData.Interpolate(nextMetaData, f);
            }
            return default(U);
        }

        /// <summary>
        /// Gets interpolated MetaData of this Segment
        /// </summary>
        /// <typeparam name="T">Metadata type implementing ICurvyInterpolatableMetadata</typeparam>
        /// <typeparam name="U">Metadata return value type</typeparam>
        /// <param name="f">a local F in the range 0..1</param>
        /// <returns>interpolated value</returns>
        [Obsolete("Use GetInterpolatedMetadata<T, U> instead")]
        public U InterpolateMetadata<T, U>(float f) where T : Component, ICurvyInterpolatableMetadata<U>
        {
            T ma = GetMetadata<T>();
            if (ma != null)
            {
                CurvySplineSegment ncp = Spline.GetNextControlPointUsingFollowUp(this);
                ICurvyInterpolatableMetadata<U> md = null;
                if (ncp)
                    md = ncp.GetMetadata<T>();
                return ma.Interpolate(md, f);
            }
            return default(U);
        }

        /// <summary>
        /// Gets interpolated MetaData of this Segment
        /// </summary>
        /// <param name="type">Metadata type implementing ICurvyInterpolatableMetadata</param>
        /// <param name="f">a local F in the range 0..1</param>
        /// <returns>interpolated value</returns>
        [Obsolete("Use GetInterpolatedMetadata<T, U> instead")]
        public object InterpolateMetadata(Type type, float f)
        {
            ICurvyInterpolatableMetadata ma = GetMetaData(type) as ICurvyInterpolatableMetadata;
            if (ma != null)
            {
                CurvySplineSegment ncp = Spline.GetNextControlPointUsingFollowUp(this);
                ICurvyInterpolatableMetadata md = null;
                if (ncp)
                {
                    md = ncp.GetMetaData(type) as ICurvyInterpolatableMetadata;
                    if (md != null)
                        return ma.InterpolateObject(md, f);
                }
            }
            return null;
        }

        /// <summary>
        /// Removes all Metadata components of this Control Point
        /// </summary>
        public void DeleteMetadata()
        {
            List<Component> metaDataList = MetaDataSet.ToList();
            for (int i = metaDataList.Count - 1; i >= 0; i--)
                metaDataList[i].Destroy();
        }
        #endregion

        /// <summary>
        /// Gets the interpolated Scale
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns>the interpolated value</returns>
        public Vector3 InterpolateScale(float localF)
        {
            CurvySplineSegment nextControlPoint = Spline.GetNextControlPoint(this);
            return nextControlPoint
                ? Vector3.Lerp(transform.lossyScale, nextControlPoint.transform.lossyScale, localF)
                : transform.lossyScale;
        }

        /// <summary>
        /// Gets the tangent for a local F
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <remarks>SmoothTangent won't get respected!</remarks>
        /// <returns>the tangent/direction</returns>
        public Vector3 GetTangent(float localF)
        {
            localF = Mathf.Clamp01(localF);
            Vector3 p = Interpolate(localF);
            return GetTangent(localF, p);
        }

        /// <summary>
        /// Gets the normalized tangent for a local F with the interpolated position for f known
        /// </summary>
        /// <remarks>This saves one interpolation if you already know the position. SmoothTangent won't get respected!</remarks>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <param name="position">the result of Interpolate(localF)</param>
        /// <returns></returns>
        public Vector3 GetTangent(float localF, Vector3 position)
        {
            CurvySpline curvySpline = Spline;
#if CONTRACTS_FULL
            Contract.Requires(curvySpline != null);
#endif

            Vector3 p2;
            int leave = 2;
            const float fIncrement = 0.01f;
            do
            {
                float f2 = localF + fIncrement;
                if (f2 > 1)
                {
                    CurvySplineSegment nSeg = curvySpline.GetNextSegment(this);
                    if (nSeg)
                        p2 = nSeg.Interpolate(f2 - 1);//return (NextSegment.Interpolate(f2 - 1) - position).normalized;
                    else
                    {
                        f2 = localF - fIncrement;
                        return (position - Interpolate(f2)).normalized;
                    }
                }
                else
                    p2 = Interpolate(f2); // return (Interpolate(f2) - position).normalized;

                localF += fIncrement;
            } while (p2 == position && --leave > 0);
            //Debug.Log(p2 + "-" + position + "=" + (p2 - position));
            return (p2 - position).normalized;
        }

        /// <summary>
        /// Gets the cached tangent for a certain F
        /// </summary>
        /// <remarks>SmoothTangent option will be respected</remarks>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns></returns>
        public Vector3 GetTangentFast(float localF)
        {
            float frag;
            int idx = getApproximationIndexINTERNAL(localF, out frag);
            int idx2 = Mathf.Min(ApproximationT.Length - 1, idx + 1);
            return (Vector3.SlerpUnclamped(ApproximationT[idx], ApproximationT[idx2], frag));
        }

        /// <summary>
        /// Combines a call to <see cref="Interpolate(float)"/> with a call to <see cref="GetTangent(float)"/> in an optimized manner
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <param name="localPosition">the corresponding position, relative to the spline's local space</param>
        /// <param name="localTangent">the corresponding tangent, relative to the spline's local space</param>
        public void InterpolateAndGetTangent(float localF, out Vector3 localPosition, out Vector3 localTangent)
        {
            localF = Mathf.Clamp01(localF);
            localPosition = Interpolate(localF, Spline.Interpolation);
            localTangent = GetTangent(localF, localPosition);
        }

        /// <summary>
        /// Combines a call to <see cref="InterpolateFast(float)"/> with a call to <see cref="GetTangentFast(float)"/> in an optimized manner
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <param name="localPosition">the corresponding position, relative to the spline's local space</param>
        /// <param name="localTangent">the corresponding tangent, relative to the spline's local space</param>
        public void InterpolateAndGetTangentFast(float localF, out Vector3 localPosition, out Vector3 localTangent)
        {
            float frag;
            int idx = getApproximationIndexINTERNAL(localF, out frag);
            int idx2 = Mathf.Min(Approximation.Length - 1, idx + 1);
            localPosition = Vector3.LerpUnclamped(Approximation[idx], Approximation[idx2], frag);
            localTangent = Vector3.SlerpUnclamped(ApproximationT[idx], ApproximationT[idx2], frag);
        }

        /// <summary>
        /// Gets a rotation looking to Tangent with the head upwards along the Up-Vector
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns>a rotation</returns>
        public Quaternion GetOrientationFast(float localF) { return GetOrientationFast(localF, false); }

        /// <summary>
        /// Gets a rotation looking to Tangent with the head upwards along the Up-Vector
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <param name="inverse">whether the orientation should be inversed or not</param>
        /// <returns>a rotation</returns>
        public Quaternion GetOrientationFast(float localF, bool inverse)
        {
            Vector3 view = GetTangentFast(localF);

            if (view != Vector3.zero)
            {
                if (inverse)
                    view *= -1;
                return Quaternion.LookRotation(view, GetOrientationUpFast(localF));
            }
            else
                return Quaternion.identity;
        }

        /// <summary>
        /// Gets the Up-Vector for a local F based on the splines' Orientation mode
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns>the Up-Vector</returns>
        public Vector3 GetOrientationUpFast(float localF)
        {
            float frag;
            int idx = getApproximationIndexINTERNAL(localF, out frag);
            int idx2 = Mathf.Min(ApproximationUp.Length - 1, idx + 1);
            return (Vector3.SlerpUnclamped(ApproximationUp[idx], ApproximationUp[idx2], frag));
        }

        /// <summary>
        /// Gets the f nearest to a certain point
        /// </summary>
        /// <param name="p">a point (in the space of the spline)</param>
        /// <returns>LocalF of the nearest position</returns>
        public float GetNearestPointF(Vector3 p)
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(CacheSize >= 0, "[Curvy] CurvySplineSegment has unintialized cache. Call Refresh() on the CurvySpline it belongs to.");
#endif


            int ui = CacheSize + 1;
            float nearestDistSqr = float.MaxValue;
            float distSqr;
            int nearestIndex = 0;
            // get the nearest index
            for (int i = 0; i < ui; i++)
            {
                Vector3 delta;
                delta.x = Approximation[i].x - p.x;
                delta.y = Approximation[i].y - p.y;
                delta.z = Approximation[i].z - p.z;
                distSqr = (delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
                if (distSqr <= nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearestIndex = i;
                }
            }
            // collide p against the lines build by the index
            int leftIdx = (nearestIndex > 0) ? nearestIndex - 1 : -1;
            int rightIdx = (nearestIndex < CacheSize) ? nearestIndex + 1 : -1;

            float lfrag = 0;
            float rfrag = 0;
            float ldistSqr = float.MaxValue;
            float rdistSqr = float.MaxValue;
            if (leftIdx > -1)
                ldistSqr = DTMath.LinePointDistanceSqr(Approximation[leftIdx], Approximation[nearestIndex], p, out lfrag);
            if (rightIdx > -1)
                rdistSqr = DTMath.LinePointDistanceSqr(Approximation[nearestIndex], Approximation[rightIdx], p, out rfrag);


            // return the nearest collision
            if (ldistSqr < rdistSqr)
                return getApproximationLocalF(leftIdx) + lfrag * mStepSize;
            else
                return getApproximationLocalF(nearestIndex) + rfrag * mStepSize;
        }

        /// <summary>
        /// Gets the local F by a distance within this line segment
        /// </summary>
        /// <param name="localDistance">local distance in the range 0..Length</param>
        /// <returns>a local F in the range 0..1</returns>
        public float DistanceToLocalF(float localDistance)
        {
            localDistance = Mathf.Clamp(localDistance, 0, Length);
            if (ApproximationDistances.Length <= 1 || localDistance == 0) return 0;
            if (Mathf.Approximately(localDistance, Length)) return 1;



            int lidx = Mathf.Min(ApproximationDistances.Length - 1, mCacheLastDistanceToLocalFIndex);

            if (ApproximationDistances[lidx] < localDistance)
                lidx = ApproximationDistances.Length - 1;
            while (ApproximationDistances[lidx] > localDistance)
                lidx--;

            mCacheLastDistanceToLocalFIndex = lidx + 1;

            float frag = (localDistance - ApproximationDistances[lidx]) / (ApproximationDistances[lidx + 1] - ApproximationDistances[lidx]);
            float lf = getApproximationLocalF(lidx);
            float uf = getApproximationLocalF(lidx + 1);
            return lf + (uf - lf) * frag;
        }

        /// <summary>
        /// Gets the local distance for a certain localF value
        /// </summary>
        /// <param name="localF">a local F value in the range 0..1</param>
        /// <returns>a distance in the range 0..Length</returns>
        public float LocalFToDistance(float localF)
        {
            localF = Mathf.Clamp01(localF);
            if (ApproximationDistances.Length <= 1 || localF == 0) return 0;
            if (Mathf.Approximately(localF, 1)) return Length;

            float frag;
            int idx = getApproximationIndexINTERNAL(localF, out frag);
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(idx >= 0);
            Assert.IsTrue(idx < ApproximationDistances.Length - 1);
#endif
            float d = ApproximationDistances[idx + 1] - ApproximationDistances[idx];
            return ApproximationDistances[idx] + d * frag;
        }

        /// <summary>
        /// Gets TF for a certain local F
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns>a TF value</returns>
        public float LocalFToTF(float localF)
        {
            return Spline.SegmentToTF(this, localF);
        }

        public override string ToString()
        {
            if (Spline != null)
                return Spline.name + "." + name;
            else
                return base.ToString();
        }

        /// <summary>
        /// Modify the control point's local rotation to match the segment's orientation
        /// </summary>
        public void BakeOrientationToTransform()
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(ApproximationUp.Length > 0);
#endif

            Quaternion orientation = GetOrientationFast(0);
            if (transform.localRotation.DifferentOrientation(orientation))
                SetLocalRotation(orientation);
        }

        /// <summary>
        /// Internal, gets the index of mApproximation by F and the remaining fragment
        /// </summary>
        public int getApproximationIndexINTERNAL(float localF, out float frag)
        {
            localF = Mathf.Clamp01(localF);
            if (localF == 1)
            {
                frag = 1;
                return Mathf.Max(0, Approximation.Length - 2);
            }
            float f = localF / mStepSize;
            int idx = (int)f;
            frag = f - idx;
            return idx;
        }

        public void LinkToSpline(CurvySpline spline)
        {
#if CURVY_SANITY_CHECKS
            //The following assertion is commented because, when dragging CPs from spline A to Spline B, we might have B's SyncSplineFromHierarchy executed before A's, and will call A's CP.LinkToSpline(B) while A's CP still has mSpline != null
            //Assert.IsTrue(mSpline == null, name);
#endif

            mSpline = spline;
        }

        public void UnlinkFromSpline()
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(mSpline != null);
#endif
            mSpline = null;
        }

#if CONTRACTS_FULL
        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            //Contract.Invariant(mSpline == null || transform.parent.GetComponent<CurvySpline>() == mSpline);
            //Contract.Invariant((mSpline == null) == (mControlPointIndex == -1));
            //Contract.Invariant(mSpline == null || mControlPointIndex < mSpline.ControlPointCount);
            //Contract.Invariant(mSpline == null || mSpline.ControlPoints.ElementAt(mControlPointIndex) == this);
            Contract.Invariant(Connection == null || Connection.ControlPoints.Contains(this));
            Contract.Invariant(Connection == null || Spline != null);
            Contract.Invariant(FollowUp == null || Connection != null);

            //TODO CONTRACT reactivate these if you find a way to call GetNextControlPoint and GetPreviousControlPoint without modifying the cache
            //Contract.Invariant(FollowUp == null || Spline.GetNextControlPoint(this) == null || Spline.GetPreviousControlPoint(this) == null);
        }
#endif

        #region Update position and rotation

        /// <summary>
        /// Sets the local position while dirtying the spline, dirtying the connected splines, and updating the connected control points' positions accordingly.
        /// </summary>
        /// <param name="newPosition"></param>
        public void SetLocalPosition(Vector3 newPosition)
        {
            Transform cachedTransform = transform;
            if (cachedTransform.localPosition != newPosition)
            {
                cachedTransform.localPosition = newPosition;
                Spline.SetDirtyPartial(this, SplineDirtyingType.Everything);
                if ((ConnectionSyncPosition || ConnectionSyncRotation) && Connection != null)
                    Connection.SetSynchronisationPositionAndRotation(
                        ConnectionSyncPosition ? cachedTransform.position : Connection.transform.position,
                        ConnectionSyncRotation ? cachedTransform.rotation : Connection.transform.rotation);
            }
        }

        /// <summary>
        /// Sets the global position while dirtying the spline, dirtying the connected splines, and updating the connected control points' positions accordingly.
        /// </summary>
        /// <param name="value"></param>
        public void SetPosition(Vector3 value)
        {
            Transform cachedTransform = transform;
            if (cachedTransform.position != value)
            {
                cachedTransform.position = value;
                Spline.SetDirtyPartial(this, SplineDirtyingType.Everything);
                if ((ConnectionSyncPosition || ConnectionSyncRotation) && Connection != null)
                    Connection.SetSynchronisationPositionAndRotation(
                        ConnectionSyncPosition ? cachedTransform.position : Connection.transform.position,
                        ConnectionSyncRotation ? cachedTransform.rotation : Connection.transform.rotation);
            }
        }

        /// <summary>
        /// Sets the local rotation while dirtying the spline, dirtying the connected splines, and updating the connected control points' rotations accordingly.
        /// </summary>
        /// <param name="value"></param>
        public void SetLocalRotation(Quaternion value)
        {
            Transform cachedTransform = transform;
            if (cachedTransform.localRotation != value)
            {
                cachedTransform.localRotation = value;
                if (OrientatinInfluencesSpline)
                    Spline.SetDirtyPartial(this, SplineDirtyingType.OrientationOnly);
                if ((ConnectionSyncPosition || ConnectionSyncRotation) && Connection != null)
                    Connection.SetSynchronisationPositionAndRotation(
                        ConnectionSyncPosition ? cachedTransform.position : Connection.transform.position,
                        ConnectionSyncRotation ? cachedTransform.rotation : Connection.transform.rotation);
            }
        }

        /// <summary>
        /// Sets the global rotation while dirtying the spline, dirtying the connected splines, and updating the connected control points' rotations accordingly.
        /// </summary>
        /// <param name="value"></param>
        public void SetRotation(Quaternion value)
        {
            Transform cachedTransform = transform;
            if (cachedTransform.rotation != value)
            {
                cachedTransform.rotation = value;
                if (OrientatinInfluencesSpline)
                    Spline.SetDirtyPartial(this, SplineDirtyingType.OrientationOnly);
                if ((ConnectionSyncPosition || ConnectionSyncRotation) && Connection != null)
                    Connection.SetSynchronisationPositionAndRotation(
                        ConnectionSyncPosition ? cachedTransform.position : Connection.transform.position,
                        ConnectionSyncRotation ? cachedTransform.rotation : Connection.transform.rotation);
            }
        }

        #endregion


        #endregion

        #region ### Interface Implementations ###

        //IPoolable
        public void OnBeforePush()
        {
            this.StripComponents();
            Disconnect();
            DeleteMetadata();
        }

        //IPoolable
        public void OnAfterPop()
        {
            Reset();
        }

        #endregion
    }
}
