// =====================================================================
// Copyright 2013-2018 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using UnityEngine.Serialization;
using FluffyUnderware.DevTools.Extensions;
using System.Reflection;
using JetBrains.Annotations;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#endif
using UnityEngine.Assertions;


namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Curvy Spline class
    /// </summary>
    [HelpURL(CurvySpline.DOCLINK + "curvyspline")]
    [AddComponentMenu("Curvy/Curvy Spline", 1)]
    [ExecuteInEditMode]
    public partial class CurvySpline : DTVersionedMonoBehaviour
    {
        /*! \cond PRIVATE */
        //TODO move elsewhere
        public const string VERSION = "6.1.0";
        public const string APIVERSION = "610";
        public const string WEBROOT = "https://curvyeditor.com/";
        public const string DOCLINK = WEBROOT + "doclink/";
        /*! \endcond */

#if UNITY_EDITOR
        /// <summary>
        /// The number of precomputed spline names 
        /// </summary>
        //DESIGN Make this parametrable by users?
        const short CachedControlPointsNameCount = 250;
        /// <summary>
        /// A list of precomputed control point names
        /// </summary>
        static readonly private string[] controlPointNames = GetControlPointNames();

#endif

        public CurvySpline()
        {
            refreshCurveAction = controlPoint => controlPoint.refreshCurveINTERNAL(Interpolation, IsControlPointASegment(controlPoint), this);
            defaultSplineEventArgs = new CurvySplineEventArgs(this, this, null);
            defaultAddAfterEventArgs = new CurvyControlPointEventArgs(this, this, null, CurvyControlPointEventArgs.ModeEnum.AddAfter);
            defaultDeleteEventArgs = new CurvyControlPointEventArgs(this, this, null, CurvyControlPointEventArgs.ModeEnum.Delete);
        }

        #region ### Serialized fields ###

        /// <summary>
        /// The list of control points
        /// </summary>
        /// <remarks>The returned list should not be modified</remarks>
        [SerializeField, HideInInspector]
        private List<CurvySplineSegment> ControlPoints = new List<CurvySplineSegment>();

        /// <summary>
        /// Whether to show the Gizmos enabled in the view settings or not at all 
        /// </summary>
        [HideInInspector]
        public bool ShowGizmos = true;

        #endregion

        #region ### Public Properties ###

        #region --- General ---

        /// <summary>
        /// The interpolation method used by this spline
        /// </summary>
        /// <remarks>AutoEndTangents's value can be updated depending on Interpolation value</remarks>
        public CurvyInterpolation Interpolation
        {
            get { return m_Interpolation; }
            set
            {
                if (m_Interpolation != value)
                {
                    m_Interpolation = value;
                    InvalidateControlPointsRelationshipCacheINTERNAL();
                    SetDirtyAll(SplineDirtyingType.Everything, false);
                }

                //Since canHaveManualEndCP uses Interpolation, and is used by AutoEndTangents, we force the later's update
                AutoEndTangents = m_AutoEndTangents;
            }
        }

        /// <summary>
        /// Whether to restrict Control Points to the local X/Y plane
        /// </summary>
        public bool RestrictTo2D
        {
            get { return m_RestrictTo2D; }
            set
            {
                if (m_RestrictTo2D != value)
                {
                    m_RestrictTo2D = value;
                    SetDirtyAll(SplineDirtyingType.Everything, false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the default Handle distance for Bezier splines
        /// </summary>
        public float AutoHandleDistance
        {
            get { return m_AutoHandleDistance; }
            set
            {
                float clampedValue = Mathf.Clamp01(value);
                if (m_AutoHandleDistance != clampedValue)
                {
                    m_AutoHandleDistance = clampedValue;
                    SetDirtyAll(SplineDirtyingType.Everything, false);
                }
            }
        }

        /// <summary>
        /// Whether this spline is closed or not
        /// </summary>
        /// <remarks>AutoEndTangents's value can be updated depending on Close value</remarks>
        public bool Closed
        {
            get { return m_Closed; }
            set
            {
                if (m_Closed != value)
                {
                    m_Closed = value;
                    InvalidateControlPointsRelationshipCacheINTERNAL();
                    SetDirtyAll(SplineDirtyingType.Everything, true);
                }
                //Since canHaveManualEndCP uses Closed, and is used by AutoEndTangents, we force the later's update
                AutoEndTangents = m_AutoEndTangents;
            }
        }

        /// <summary>
        /// Whether the first/last Control Point should act as the end tangent, too.
        /// </summary>
        /// <remarks>Ignored by linear splines and Bezier ones</remarks>
        public bool AutoEndTangents
        {
            get { return m_AutoEndTangents; }
            set
            {
                bool v = canHaveManualEndCP() ? value : true;
                if (m_AutoEndTangents != v)
                {
                    m_AutoEndTangents = v;
                    InvalidateControlPointsRelationshipCacheINTERNAL();
                    SetDirtyAll(SplineDirtyingType.Everything, true);
                }
            }
        }

        /// <summary>
        /// Orientation mode
        /// </summary>
        public CurvyOrientation Orientation
        {
            get { return m_Orientation; }
            set
            {
                if (m_Orientation != value)
                {
                    m_Orientation = value;
                    SetDirtyAll(SplineDirtyingType.Everything, false);
                }
            }
        }

        public CurvyUpdateMethod UpdateIn
        {
            get { return m_UpdateIn; }
            set
            {
                if (m_UpdateIn != value)
                    m_UpdateIn = value;
            }
        }

        #endregion

        #region --- Advanced Settings ---

        /// <summary>
        /// Gets or sets Spline color
        /// </summary>
        public Color GizmoColor
        {
            get { return m_GizmoColor; }
            set
            {
                if (m_GizmoColor != value)
                    m_GizmoColor = value;
            }
        }

        /// <summary>
        /// Gets or sets selected segment color
        /// </summary>
        public Color GizmoSelectionColor
        {
            get { return m_GizmoSelectionColor; }
            set
            {
                if (m_GizmoSelectionColor != value)
                    m_GizmoSelectionColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the cache density
        /// Defines how densely the cached points are. When the value is 100, the number of cached points per world distance unit is equal to the spline's MaxPointsPerUnit
        /// </summary>
        public int CacheDensity
        {
            get { return m_CacheDensity; }
            set
            {
                int clampedDensity = Mathf.Clamp(value, 1, 100);

                if (m_CacheDensity != clampedDensity)
                {
                    m_CacheDensity = clampedDensity;
                    SetDirtyAll(SplineDirtyingType.Everything, false);
                }
            }
        }

        /// <summary>
        /// The maximum number of sampling points per world distance unit. Sampling is used in caching or shape extrusion for example" </summary>
        public float MaxPointsPerUnit
        {
            get
            {
                return m_MaxPointsPerUnit;
            }
            set
            {
                float clampedValue = Mathf.Clamp(value, 0.0001f, 1000);
                if (m_MaxPointsPerUnit != clampedValue)
                {
                    m_MaxPointsPerUnit = clampedValue;
                    SetDirtyAll(SplineDirtyingType.Everything, false);
                }
            }
        }

        /// <summary>
        /// Whether to use GameObject pooling for Control Points at runtime
        /// </summary>
        public bool UsePooling
        {
            get { return m_UsePooling; }
            set
            {
                if (m_UsePooling != value)
                    m_UsePooling = value;
            }
        }
        /// <summary>
        /// Whether to use threading where applicable or not.
        /// Threading is currently not supported when targetting WebGL and Universal Windows Platform
        /// </summary>
        public bool UseThreading
        {
            get
            {
#if !UNITY_WSA && !UNITY_WEBGL
                return m_UseThreading;
#else

                return false;
#endif
            }
            set
            {
                if (m_UseThreading != value)
                    m_UseThreading = value;
            }
        }

        /// <summary>
        /// Whether the spline should automatically refresh when a Control Point's position change
        /// </summary>
        /// <remarks>Enable this if you animate a Control Point's transform!</remarks>
        public bool CheckTransform
        {
            get { return m_CheckTransform; }
            set
            {
                if (m_CheckTransform != value)
                {
                    m_CheckTransform = value;
                    SetDirtyAll(SplineDirtyingType.Everything, false);
                }
            }
        }


        #endregion

        #region --- TCB Options ---

        /// <summary>
        /// Global Tension
        /// </summary>
        /// <remarks>This only applies to TCB interpolation</remarks>
        public float Tension
        {
            get { return m_Tension; }
            set
            {
                if (m_Tension != value)
                {
                    m_Tension = value;
                    SetDirtyAll(SplineDirtyingType.Everything, false);
                }
            }
        }

        /// <summary>
        /// Global Continuity
        /// </summary>
        /// <remarks>This only applies to TCB interpolation</remarks>
        public float Continuity
        {
            get { return m_Continuity; }
            set
            {
                if (m_Continuity != value)
                {
                    m_Continuity = value;
                    SetDirtyAll(SplineDirtyingType.Everything, false);
                }
            }
        }

        /// <summary>
        /// Global Bias
        /// </summary>
        /// <remarks>This only applies to TCB interpolation</remarks>
        public float Bias
        {
            get { return m_Bias; }
            set
            {
                if (m_Bias != value)
                {
                    m_Bias = value;
                    SetDirtyAll(SplineDirtyingType.Everything, false);
                }
            }
        }
        #endregion

        #region --- Others ---

        /// <summary>
        /// Whether the spline is fully initialized and all segments loaded
        /// </summary>
        public bool IsInitialized { get { return mIsInitialized; } }

        /// <summary>
        /// The bounding box of the spline
        /// </summary>
        public Bounds Bounds
        {
            get
            {
                if (!mBounds.HasValue)
                {
#if CURVY_SANITY_CHECKS
                    DoSanityChecks();
#endif
                    Bounds bounds;
                    if (Count > 0)
                    {
                        Bounds b = this[0].Bounds;

                        for (int i = 1; i < Count; i++)
                            b.Encapsulate(this[i].Bounds);
                        bounds = b;
                    }
                    else
                        bounds = new Bounds(transform.position, Vector3.zero);

                    if (Dirty == false)
                        mBounds = bounds;
                    return bounds;
                }
                return mBounds.Value;
            }
        }

        /// <summary>
        /// Gets the number of Segments
        /// </summary>
#pragma warning disable 618
        public int Count { get { return Segments.Count; } }
#pragma warning restore 618
        /// <summary>
        /// Gets the number of Control Points
        /// </summary>
        public int ControlPointCount { get { return ControlPoints.Count; } }

        /// <summary>
        /// Gets total Cache Size
        /// </summary>
        public int CacheSize
        {
            get
            {
                if (mCacheSize < 0)
                {
#if CURVY_SANITY_CHECKS
                    DoSanityChecks();
#endif
                    int cacheSize = 0;
#pragma warning disable 618
                    List<CurvySplineSegment> segments = Segments;
#pragma warning restore 618
                    for (int i = 0; i < segments.Count; i++)
                        cacheSize += segments[i].CacheSize;

                    if (Dirty == false)
                        mCacheSize = cacheSize;
                    return cacheSize;
                }
                return mCacheSize;
            }
        }

        /// <summary>
        /// Gets the total length of the Spline or SplineGroup
        /// </summary>
        /// <remarks>The accuracy depends on the current Granularity (higher Granularity means more exact values)</remarks>
        public float Length
        {
            get
            {
                if (length < 0)
                {
#if CURVY_SANITY_CHECKS
                    DoSanityChecks();
#endif

                    float tempLength;
#pragma warning disable 618
                    if (Segments.Count == 0)
#pragma warning restore 618
                        tempLength = 0;
                    else
                        tempLength = (Closed) ? this[Count - 1].Distance + this[Count - 1].Length : LastVisibleControlPoint.Distance;
                    if (Dirty == false)
                        length = tempLength;
                    return tempLength;
                }
                return length;
            }
        }

        /// <summary>
        /// When a spline is dirty, this means that it's cached data is no more up to date, and should be updated. The update is done automatically each frame when needed, or manually by calling <see cref="Refresh"/>
        /// </summary>
        public bool Dirty
        {
            get
            {
                return (allControlPointsAreDirty || dirtyControlPointsMinimalSet.Count > 0);
            }
        }

        /// <summary>
        /// Gets the Segment at a certain index
        /// </summary>
        /// <param name="idx">an index in the range 0..Count</param>
        /// <returns>the corresponding spline segment</returns>
        public CurvySplineSegment this[int idx]
        {
            get
            {
#if CONTRACTS_FULL
                Contract.Requires(idx > -1 && idx < Segments.Count);
#endif
#pragma warning disable 618
                return Segments[idx];
#pragma warning restore 618
            }
        }

        /// <summary>
        /// The list of control points
        /// </summary>
        public ReadOnlyCollection<CurvySplineSegment> ControlPointsList
        {
            //TODO use IReadOnlyList when .NET 4.6 will be default
            get
            {
                //OPTIM find the proper place to initialize readOnlyControlPoints, to avoid the if bellow to be tested at each call.
                //Note: even when initializing readOnlyControlPoints in Awake, OnEnable, OnAfterDeserialize  and OnValidate, scene 25 had a null reference exception on readOnlyControlPoints. Here is the stack trace:
                /*
                NullReferenceException: Object reference not set to an instance of an object
                FluffyUnderware.Curvy.CurvySpline.GetControlPointIndex (FluffyUnderware.Curvy.CurvySplineSegment controlPoint) (at Assets/Packages/Curvy/Base/CurvySpline.cs:254)
                FluffyUnderware.Curvy.CurvySplineSegment.GetPreviousControlPoint (System.Boolean segmentsOnly, System.Boolean useFollowUp) (at Assets/Packages/Curvy/Base/CurvySplineSegment.cs:1552)
                FluffyUnderware.Curvy.CurvySplineSegment.SetDirty (System.Boolean dirtyCurve, System.Boolean dirtyOrientation) (at Assets/Packages/Curvy/Base/CurvySplineSegment.cs:2148)
                FluffyUnderware.Curvy.CurvyMetadataBase.SetDirty () (at Assets/Packages/Curvy/Base/CurvyMetadataBase.cs:84)
                FluffyUnderware.Curvy.MetaCGOptions.OnValidate () (at Assets/Packages/Curvy/Base/CG/MetaCGOptions.cs:239)
                */
                if (readOnlyControlPoints == null)
                    readOnlyControlPoints = ControlPoints.AsReadOnly();
                return readOnlyControlPoints;
            }
        }



        /// <summary>
        /// Gets the first visible Control Point (equals the first segment or this[0])
        /// </summary>
        /// <remarks>Can be null, for example for a Catmull-Rom spline whith only two splines and AutoEndTangent set to false</remarks>
        [CanBeNull]
        public CurvySplineSegment FirstVisibleControlPoint
        {
            get
            {
                if (isCpsRelationshipCacheValid == false)
                    RebuildControlPointsRelationshipCache(true);
                return firstVisibleControlPoint;
            }
        }

        /// <summary>
        /// Gets the last visible Control Point (i.e. the end CP of the last segment)
        /// </summary>
        /// <remarks>Is null if spline has no segments</remarks>
        [CanBeNull]
        public CurvySplineSegment LastVisibleControlPoint
        {
            get
            {
                if (isCpsRelationshipCacheValid == false)
                    RebuildControlPointsRelationshipCache(true);
                return lastVisibleControlPoint;
            }
        }

        /// <summary>
        /// Gets the first segment of the spline
        /// </summary>
        public CurvySplineSegment FirstSegment
        {
            get
            {
                if (isCpsRelationshipCacheValid == false)
                    RebuildControlPointsRelationshipCache(true);
                return firstSegment;
            }
        }

        /// <summary>
        /// Gets the last segment of the spline
        /// </summary>
        public CurvySplineSegment LastSegment
        {
            get
            {
                if (isCpsRelationshipCacheValid == false)
                    RebuildControlPointsRelationshipCache(true);
                return lastSegment;
            }
        }

        /// <summary>
        /// Gets the FollowUp spline of the last Control Point, i.e. the next fully connected spline 
        /// </summary>
        public CurvySpline NextSpline
        {
            get
            {
                CurvySplineSegment cp = LastVisibleControlPoint;
                return (cp && cp.FollowUp) ? cp.FollowUp.Spline : null;
            }
        }

        /// <summary>
        /// Gets the FollowUp spline of the first Control Point, i.e. the previous fully connected spline 
        /// </summary>
        public CurvySpline PreviousSpline
        {
            get
            {
                CurvySplineSegment cp = FirstVisibleControlPoint;
                return (cp && cp.FollowUp) ? cp.FollowUp.Spline : null;
            }
        }

        /// <summary>
        /// Returns true if the global position, rotation or scale of the spline has changed this frame
        /// </summary>
        /// <seealso cref="OnGlobalCoordinatesChanged"/>
        public bool GlobalCoordinatesChangedThisFrame
        {
            get { return globalCoordinatesChangedThisFrame; }
        }

        /// <summary>
        /// Is triggered when the global position, rotation or scale of the spline changes.
        /// The triggering instance of CurvySpline is passed as a parameter of the delegate
        /// </summary>
        /// <remarks>This is triggered at the very end of the spline updating method. <see cref="UpdateIn"/></remarks>
        /// <seealso cref="GlobalCoordinatesChangedThisFrame"/>
        public Action<CurvySpline> OnGlobalCoordinatesChanged { get; set; }

        public CurvySplineEvent OnRefresh
        {
            get { return m_OnRefresh; }
            set
            {
                if (m_OnRefresh != value)
                    m_OnRefresh = value;

            }

        }
        /// <summary>
        /// Callback after one or more Control Points have been added or deleted
        /// </summary>
        /// <remarks>This executes last, after individual add/delete events and OnRefresh </remarks>
        public CurvySplineEvent OnAfterControlPointChanges
        {
            get { return m_OnAfterControlPointChanges; }
            set
            {
                if (m_OnAfterControlPointChanges != value)
                    m_OnAfterControlPointChanges = value;
            }
        }
        /// <summary>
        /// Callback before a Control Point is about to be added. Return false to cancel the execution
        /// </summary>
        public CurvyControlPointEvent OnBeforeControlPointAdd
        {
            get { return m_OnBeforeControlPointAdd; }
            set
            {
                if (m_OnBeforeControlPointAdd != value)
                    m_OnBeforeControlPointAdd = value;
            }
        }
        /// <summary>
        /// Callback after a Control Point has been added and the spline was refreshed
        /// </summary>
        public CurvyControlPointEvent OnAfterControlPointAdd
        {
            get { return m_OnAfterControlPointAdd; }
            set
            {
                if (m_OnAfterControlPointAdd != value)
                    m_OnAfterControlPointAdd = value;
            }

        }
        /// <summary>
        /// Callback before a Control Point is about to be deleted. Return false to cancel the execution.
        /// </summary>
        public CurvyControlPointEvent OnBeforeControlPointDelete
        {
            get { return m_OnBeforeControlPointDelete; }
            set
            {
                if (m_OnBeforeControlPointDelete != value)
                    m_OnBeforeControlPointDelete = value;
            }
        }

        #endregion

        #endregion

        #region ### Public Static Methods ###

        /// <summary>
        /// Creates an empty spline
        /// </summary>
        public static CurvySpline Create()
        {
            CurvySpline spl = new GameObject("Curvy Spline", typeof(CurvySpline)).GetComponent<CurvySpline>();
            spl.gameObject.layer = CurvyGlobalManager.SplineLayer;
            spl.Start();
            return spl;
        }

        /// <summary>
        /// Creates an empty spline with the same settings as another spline
        /// </summary>
        /// <param name="takeOptionsFrom">another spline</param>
        public static CurvySpline Create(CurvySpline takeOptionsFrom)
        {
            CurvySpline spl = Create();
            if (takeOptionsFrom)
            {
                spl.RestrictTo2D = takeOptionsFrom.RestrictTo2D;
                spl.GizmoColor = takeOptionsFrom.GizmoColor;
                spl.GizmoSelectionColor = takeOptionsFrom.GizmoSelectionColor;
                spl.Interpolation = takeOptionsFrom.Interpolation;
                spl.Closed = takeOptionsFrom.Closed;
                spl.AutoEndTangents = takeOptionsFrom.AutoEndTangents;
                spl.CacheDensity = takeOptionsFrom.CacheDensity;
                spl.MaxPointsPerUnit = takeOptionsFrom.MaxPointsPerUnit;
                spl.Orientation = takeOptionsFrom.Orientation;
                spl.CheckTransform = takeOptionsFrom.CheckTransform;
            }
            return spl;
        }

        /// <summary>
        /// Gets the number of Cache Points needed for a certain part of a spline
        /// </summary>
        /// <param name="density">A value between 1 and 100 included. When equal to 100, the number of cache points per world distance unit is equal to maxPointsPerUnit</param>
        /// <param name="splineLength">the length of the spline segment</param>
        /// <param name="maxPointsPerUnit">Maximum number of Cache Points per world distance unit</param>
        public static int CalculateCacheSize(int density, float splineLength, float maxPointsPerUnit)
        {
#if CONTRACTS_FULL
            Contract.Requires(CodeContractsUtility.IsPositiveNumber(splineLength));
#endif
            return Mathf.FloorToInt(CalculateSamplingPointsPerUnit(density, maxPointsPerUnit) * splineLength) + 1;
        }

        /// <summary>
        /// Returns the (floating) number of sampling points per world distance unit.
        /// </summary>
        /// <param name="density">A value between 1 and 100 included. When equal to 100, the number of sampling points per world distance unit is equal to maxPointsPerUnit</param>
        /// <param name="maxPointsPerUnit">Maximum number of sampling points per world distance unit</param>
        /// <returns></returns>
        static public float CalculateSamplingPointsPerUnit(int density, float maxPointsPerUnit)
        {
#if CONTRACTS_FULL
            Contract.Requires(density > 0);
            Contract.Requires(density <= 100);
            Contract.Requires(maxPointsPerUnit.IsPositiveNumber());
#endif
            int clampedDensity = Mathf.Clamp(density, 1, 100);
            if (clampedDensity != density)
            {
                DTLog.LogWarning("[Curvy] CalculateSamplingPointsPerUnit got an invalid density parameter. It should be between 1 and 100. The parameter value was " + density);
                density = clampedDensity;
            }

            //This basically equals to Mathf.FloorToInt(length * (maxPointsPerUnit * ((density - 1) / 99)²)) + 1
            //Here is a plot of (density - 1) / 99)²
            //https://www.wolframalpha.com/input/?i=plot+((x+-+1)+%2F+99+)%5E2+for+x+from+1+to+100
            return DTTween.QuadIn(density - 1, 0, maxPointsPerUnit, 99);
        }

        /// <summary>
        /// Cubic-Beziere Interpolation
        /// </summary>
        /// <param name="T0">HandleIn</param>
        /// <param name="P0">Pn</param>
        /// <param name="P1">Pn+1</param>
        /// <param name="T1">HandleOut</param>
        /// <param name="f">f in the range 0..1</param>
        /// <returns></returns>
        public static Vector3 Bezier(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
        {
            //If you modify this, modify also the inlined version of this method in refreshCurveINTERNAL()

            const double Ft2 = 3; const double Ft3 = -3;
            const double Fu1 = 3; const double Fu2 = -6; const double Fu3 = 3;
            const double Fv1 = -3; const double Fv2 = 3;

            double FAX = -P0.x + Ft2 * T0.x + Ft3 * T1.x + P1.x;
            double FBX = Fu1 * P0.x + Fu2 * T0.x + Fu3 * T1.x;
            double FCX = Fv1 * P0.x + Fv2 * T0.x;
            double FDX = P0.x;

            double FAY = -P0.y + Ft2 * T0.y + Ft3 * T1.y + P1.y;
            double FBY = Fu1 * P0.y + Fu2 * T0.y + Fu3 * T1.y;
            double FCY = Fv1 * P0.y + Fv2 * T0.y;
            double FDY = P0.y;

            double FAZ = -P0.z + Ft2 * T0.z + Ft3 * T1.z + P1.z;
            double FBZ = Fu1 * P0.z + Fu2 * T0.z + Fu3 * T1.z;
            double FCZ = Fv1 * P0.z + Fv2 * T0.z;
            double FDZ = P0.z;

            float FX = (float)(((FAX * f + FBX) * f + FCX) * f + FDX);
            float FY = (float)(((FAY * f + FBY) * f + FCY) * f + FDY);
            float FZ = (float)(((FAZ * f + FBZ) * f + FCZ) * f + FDZ);

            return new Vector3(FX, FY, FZ);
        }

        [Obsolete("This methods is not used and will be removed in an upcoming version")]
        public static float BezierTangent(float T0, float P0, float P1, float T1, float t)
        {
            float C1 = (P1 - (3.0f * T1) + (3.0f * T0) - P0);
            float C2 = ((3.0f * T1) - (6.0f * T0) + (3.0f * P0));
            float C3 = ((3.0f * T0) - (3.0f * P0));
            return ((3.0f * C1 * t * t) + (2.0f * C2 * t) + C3);
        }

        //OPTIM Is using this better than using positions delta?
        public static Vector3 BezierTangent(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
        {
            Vector3 C1 = (P1 - (3.0f * T1) + (3.0f * T0) - P0);
            Vector3 C2 = ((3.0f * T1) - (6.0f * T0) + (3.0f * P0));
            Vector3 C3 = ((3.0f * T0) - (3.0f * P0));
            return ((3.0f * C1 * f * f) + (2.0f * C2 * f) + C3);
        }

        /// <summary>
        /// Catmull-Rom Interpolation
        /// </summary>
        /// <param name="T0">Pn-1 (In Tangent)</param>
        /// <param name="P0">Pn</param>
        /// <param name="P1">Pn+1</param>
        /// <param name="T1">Pn+2 (Out Tangent)</param>
        /// <param name="f">f in the range 0..1</param>
        /// <returns>the interpolated position</returns>
        public static Vector3 CatmullRom(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
        {
            //If you modify this, modify also the inlined version of this method in refreshCurveINTERNAL()

            const double Ft1 = -0.5; const double Ft2 = 1.5; const double Ft3 = -1.5; const double Ft4 = 0.5;
            const double Fu2 = -2.5; const double Fu3 = 2; const double Fu4 = -0.5;
            const double Fv1 = -0.5; const double Fv3 = 0.5;

            double FAX = Ft1 * T0.x + Ft2 * P0.x + Ft3 * P1.x + Ft4 * T1.x;
            double FBX = T0.x + Fu2 * P0.x + Fu3 * P1.x + Fu4 * T1.x;
            double FCX = Fv1 * T0.x + Fv3 * P1.x;
            double FDX = P0.x;

            double FAY = Ft1 * T0.y + Ft2 * P0.y + Ft3 * P1.y + Ft4 * T1.y;
            double FBY = T0.y + Fu2 * P0.y + Fu3 * P1.y + Fu4 * T1.y;
            double FCY = Fv1 * T0.y + Fv3 * P1.y;
            double FDY = P0.y;

            double FAZ = Ft1 * T0.z + Ft2 * P0.z + Ft3 * P1.z + Ft4 * T1.z;
            double FBZ = T0.z + Fu2 * P0.z + Fu3 * P1.z + Fu4 * T1.z;
            double FCZ = Fv1 * T0.z + Fv3 * P1.z;
            double FDZ = P0.z;

            float FX = (float)(((FAX * f + FBX) * f + FCX) * f + FDX);
            float FY = (float)(((FAY * f + FBY) * f + FCY) * f + FDY);
            float FZ = (float)(((FAZ * f + FBZ) * f + FCZ) * f + FDZ);

            return new Vector3(FX, FY, FZ);
        }

        /// <summary>
        /// Kochanek-Bartels/TCB-Interpolation
        /// </summary>
        /// <param name="T0">Pn-1 (In Tangent)</param>
        /// <param name="P0">Pn</param>
        /// <param name="P1">Pn+1</param>
        /// <param name="T1">Pn+2 (Out Tangent)</param>
        /// <param name="f">f in the range 0..1</param>
        /// <param name="FT0">Start Tension</param>
        /// <param name="FC0">Start Continuity</param>
        /// <param name="FB0">Start Bias</param>
        /// <param name="FT1">End Tension</param>
        /// <param name="FC1">End Continuity</param>
        /// <param name="FB1">End Bias</param>
        /// <returns>the interpolated position</returns>
        public static Vector3 TCB(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f, float FT0, float FC0, float FB0, float FT1, float FC1, float FB1)
        {
            //If you modify this, modify also the inlined version of this method in refreshCurveINTERNAL()

            double FFA = (1 - FT0) * (1 + FC0) * (1 + FB0);
            double FFB = (1 - FT0) * (1 - FC0) * (1 - FB0);
            double FFC = (1 - FT1) * (1 - FC1) * (1 + FB1);
            double FFD = (1 - FT1) * (1 + FC1) * (1 - FB1);

            double DD = 2;
            double Ft1 = -FFA / DD; double Ft2 = (+4 + FFA - FFB - FFC) / DD; double Ft3 = (-4 + FFB + FFC - FFD) / DD; double Ft4 = FFD / DD;
            double Fu1 = +2 * FFA / DD; double Fu2 = (-6 - 2 * FFA + 2 * FFB + FFC) / DD; double Fu3 = (+6 - 2 * FFB - FFC + FFD) / DD; double Fu4 = -FFD / DD;
            double Fv1 = -FFA / DD; double Fv2 = (FFA - FFB) / DD; double Fv3 = FFB / DD;
            double Fw2 = +2 / DD;

            double FAX = Ft1 * T0.x + Ft2 * P0.x + Ft3 * P1.x + Ft4 * T1.x;
            double FBX = Fu1 * T0.x + Fu2 * P0.x + Fu3 * P1.x + Fu4 * T1.x;
            double FCX = Fv1 * T0.x + Fv2 * P0.x + Fv3 * P1.x;
            double FDX = Fw2 * P0.x;

            double FAY = Ft1 * T0.y + Ft2 * P0.y + Ft3 * P1.y + Ft4 * T1.y;
            double FBY = Fu1 * T0.y + Fu2 * P0.y + Fu3 * P1.y + Fu4 * T1.y;
            double FCY = Fv1 * T0.y + Fv2 * P0.y + Fv3 * P1.y;
            double FDY = Fw2 * P0.y;

            double FAZ = Ft1 * T0.z + Ft2 * P0.z + Ft3 * P1.z + Ft4 * T1.z;
            double FBZ = Fu1 * T0.z + Fu2 * P0.z + Fu3 * P1.z + Fu4 * T1.z;
            double FCZ = Fv1 * T0.z + Fv2 * P0.z + Fv3 * P1.z;
            double FDZ = Fw2 * P0.z;

            float FX = (float)(((FAX * f + FBX) * f + FCX) * f + FDX);
            float FY = (float)(((FAY * f + FBY) * f + FCY) * f + FDY);
            float FZ = (float)(((FAZ * f + FBZ) * f + FCZ) * f + FDZ);

            return new Vector3(FX, FY, FZ);
        }

        #endregion

        #region ### Public Methods ###

        #region --- Methods based on TF (total fragment) ---

        /// <summary>
        /// Gets the interpolated position for a certain TF
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 Interpolate(float tf)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            return Interpolate(tf, Interpolation);
        }

        /// <summary>
        /// Gets the interpolated position for a certain TF
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <param name="interpolation">the interpolation to use</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 Interpolate(float tf, CurvyInterpolation interpolation)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.Interpolate(localF, interpolation) : Vector3.zero;
        }

        /// <summary>
        /// Gets the interpolated position for a certain TF using a linear approximation
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 InterpolateFast(float tf)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);

            return (seg) ? seg.InterpolateFast(localF) : Vector3.zero;
        }

        /// <summary>
        /// Gets metadata for a certain TF
        /// </summary>
        /// <typeparam name="T">Metadata type interfacing ICurvyMetadata</typeparam>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the metadata</returns>
#pragma warning disable 618
        public T GetMetadata<T>(float tf) where T : Component, ICurvyMetadata
#pragma warning restore 618
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.GetMetadata<T>() : null;
        }


        /// <summary>
        /// Gets metadata for a certain TF
        /// </summary>
        /// <param name="type">Metadata type interfacing ICurvyMetadata</typeparam>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the metadata</returns>
        [Obsolete("Use GetMetaData<T> instead")]
        public Component GetMetadata(System.Type type, float tf)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.GetMetaData(type) : null;
        }

        /// <summary>
        /// Gets an interpolated Metadata value for a certain TF
        /// </summary>
        /// <typeparam name="T">Metadata type inheriting from CurvyInterpolatableMetadataBase</typeparam>
        /// <typeparam name="U">Metadata's Value type</typeparam>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>The interpolated value. If no Metadata of specified type is present at the given tf, the default value of type U is returned</returns>
        public U GetInterpolatedMetadata<T, U>(float tf) where T : CurvyInterpolatableMetadataBase<U>
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.GetInterpolatedMetadata<T, U>(localF) : default(U);
        }

        /// <summary>
        /// Gets an interpolated Metadata value for a certain TF
        /// </summary>
        /// <typeparam name="T">Metadata type interfacing ICurvyInterpolatableMetadata</typeparam>
        /// <typeparam name="U">Return Value type of T</typeparam>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the interpolated value</returns>
        [Obsolete("Use GetInterpolatedMetadata<T, U> instead")]
        public U InterpolateMetadata<T, U>(float tf) where T : Component, ICurvyInterpolatableMetadata<U>
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.InterpolateMetadata<T, U>(localF) : default(U);
        }

        /// <summary>
        /// Gets an interpolated Metadata value for a certain TF
        /// </summary>
        /// <param name="type">Metadata type interfacing ICurvyInterpolatableMetadata</param>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the interpolated value</returns>
        [Obsolete("Use GetInterpolatedMetadata<T, U> instead")]
        public object InterpolateMetadata(System.Type type, float tf)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.InterpolateMetadata(type, localF) : null;
        }

        /// <summary>
        /// Gets an interpolated Scale for a certain TF
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
        /// <param name="tf">TF value reflecting position on spline(0..1)</param>
        /// <returns>the interpolated value</returns>
        public Vector3 InterpolateScale(float tf)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.InterpolateScale(localF) : Vector3.zero;
        }

        /// <summary>
        /// Gets the Up-Vector for a certain TF based on the splines' Orientation mode
        /// </summary>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the Up-Vector</returns
        public Vector3 GetOrientationUpFast(float tf)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);

            return (seg) ? seg.GetOrientationUpFast(localF) : Vector3.zero;
        }

        /// <summary>
        /// Gets a rotation looking to Tangent with the head upwards along the Up-Vector
        /// </summary>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <param name="inverse">whether the orientation should be inversed or not</param>
        /// <returns>a rotation</returns>
        public Quaternion GetOrientationFast(float tf, bool inverse = false)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.GetOrientationFast(localF, inverse) : Quaternion.identity;
        }

        /// <summary>
        /// Gets the normalized tangent for a certain TF
        /// </summary>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <returns>a tangent vector</returns>
        public Vector3 GetTangent(float tf)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.GetTangent(localF) : Vector3.zero;
        }

        /// <summary>
        /// Gets the normalized tangent for a certain TF with a known position for TF
        /// </summary>
        /// <remarks>This saves one interpolation</remarks>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <param name="localPosition">the interpolated position, relative to the spline's local space for TF</param>
        /// <returns>a tangent vector</returns>
        public Vector3 GetTangent(float tf, Vector3 localPosition)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.GetTangent(localF, localPosition) : Vector3.zero;
        }

        /// <summary>
        /// Gets the normalized tangent for a certain TF using a linear approximation
        /// </summary>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <returns>a tangent vector</returns>
        public Vector3 GetTangentFast(float tf)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.GetTangentFast(localF) : Vector3.zero;
        }

        /// <summary>
        /// Combines a call to <see cref="Interpolate(float)"/> with a call to <see cref="GetTangent(float)"/> in an optimized manner
        /// </summary>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <param name="localPosition">the corresponding position, relative to the spline's local space</param>
        /// <param name="localTangent">the corresponding tangent, relative to the spline's local space</param>
        public void InterpolateAndGetTangent(float tf, out Vector3 localPosition, out Vector3 localTangent)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            if (seg)
                seg.InterpolateAndGetTangent(localF, out localPosition, out localTangent);
            else
            {
                localPosition = Vector3.zero;
                localTangent = Vector3.zero;
            }
        }

        /// <summary>
        /// Combines a call to <see cref="InterpolateFast(float)"/> with a call to <see cref="GetTangentFast(float)"/> in an optimized manner
        /// </summary>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <param name="localPosition">the corresponding position, relative to the spline's local space</param>
        /// <param name="localTangent">the corresponding tangent, relative to the spline's local space</param>
        public void InterpolateAndGetTangentFast(float tf, out Vector3 localPosition, out Vector3 localTangent)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            if (seg)
                seg.InterpolateAndGetTangentFast(localF, out localPosition, out localTangent);
            else
            {
                localPosition = Vector3.zero;
                localTangent = Vector3.zero;
            }
        }

        /// <summary>
        /// Gets the normalized tangent by distance from the spline/group's start.
        /// </summary>
        /// <param name="distance">distance in the range of 0..Length</param>
        /// <returns>the tangent/direction</returns>
        public Vector3 GetTangentByDistance(float distance)
        {
            return GetTangent(DistanceToTF(distance));
        }

        /// <summary>
        /// Gets the normalized tangent by distance from the spline/group's start. Unlike TangentByDistance() this uses a linear approximation
        /// </summary>
        /// <param name="distance">distance in the range of 0..Length</param>
        /// <returns>the tangent/direction</returns>
        public Vector3 GetTangentByDistanceFast(float distance)
        {
            return GetTangentFast(DistanceToTF(distance));
        }
        /// <summary>
        /// Gets the interpolated position by distance from the spline/group's start
        /// </summary>
        /// <param name="distance">distance in the range of 0..Length</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 InterpolateByDistance(float distance)
        {
            return Interpolate(DistanceToTF(distance));
        }

        /// <summary>
        /// Gets the interpolated position by distance from the spline/group's start. Unlike InterpolateByDistance() this uses a linear approximation
        /// </summary>
        /// <param name="distance">distance in the range of 0..Length</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 InterpolateByDistanceFast(float distance)
        {
            return InterpolateFast(DistanceToTF(distance));
        }

        /// <summary>
        /// Converts a unit distance into TF-distance by extrapolating the curvation at a given point on the curve
        /// </summary>
        /// <param name="tf">the current TF value</param>
        /// <param name="distance">the distance in world units</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the distance in TF</returns>
        public float ExtrapolateDistanceToTF(float tf, float distance, float stepSize)
        {
            stepSize = Mathf.Max(0.0001f, stepSize);
            Vector3 p = Interpolate(tf);
            float tf1;
            if (tf == 1)
                tf1 = Mathf.Min(1f, tf - stepSize);
            else
                tf1 = Mathf.Min(1f, tf + stepSize);

            stepSize = Mathf.Abs(tf1 - tf);
            Vector3 p1 = Interpolate(tf1);
            float mag = (p1 - p).magnitude;

            if (mag != 0)
            {
                return (1 / mag) * stepSize * distance;
            }
            else return 0;
        }

        /// <summary>
        /// Converts a unit distance into TF-distance by extrapolating the curvation at a given point on the curve using a linear approximation
        /// </summary>
        /// <param name="tf">the current TF value</param>
        /// <param name="distance">the distance in world units</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the distance in TF</returns>
        public float ExtrapolateDistanceToTFFast(float tf, float distance, float stepSize)
        {
            stepSize = Mathf.Max(0.0001f, stepSize);
            Vector3 p = InterpolateFast(tf);
            float tf1;
            if (tf == 1)
                tf1 = Mathf.Min(1f, tf - stepSize);
            else
                tf1 = Mathf.Min(1f, tf + stepSize);

            stepSize = Mathf.Abs(tf1 - tf);
            Vector3 p1 = InterpolateFast(tf1);
            float mag = (p1 - p).magnitude;

            if (mag != 0)
            {
                return (1 / mag) * stepSize * distance;
            }
            else return 0;
        }

        #endregion

        #region --- Conversion Methods ---

        /// <summary>
        /// Converts a TF value to a distance
        /// </summary>
        /// <param name="tf">a TF value</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>distance from the first segment's Control Point</returns>
        public float TFToDistance(float tf, CurvyClamping clamping = CurvyClamping.Clamp)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif
            float splineLength = Length;

            float result;
            if (splineLength == 0)
                result = 0;
            else if (tf == 0)
                result = 0;
            else if (tf == 1)
                result = splineLength;
            else
            {
                float localF;
                CurvySplineSegment seg = TFToSegment(tf, out localF, clamping);
                result = (seg) ? seg.Distance + seg.LocalFToDistance(localF) : 0;
            }
            return result;
        }

        /// <summary>
        /// Gets the segment and the local F for a certain TF
        /// </summary>
        /// <param name="tf">the TF value</param>
        /// <param name="localF">gets the remaining localF in the range 0..1</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>the segment the given TF is inside</returns>
        public CurvySplineSegment TFToSegment(float tf, out float localF, CurvyClamping clamping)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            tf = CurvyUtility.ClampTF(tf, clamping);
            localF = 0;
            int segmentsCount = Count;
            if (segmentsCount == 0) return null;
            float f = tf * segmentsCount;


            double roundedF = Math.Round(f);
            int idx;
            if (f.Approximately((float)roundedF))
            {
                //This is to handle cases where (example) f is shown as 28 in debugger, but (int)f gives 27, which means that f is in fact a 27.9999999999......
                idx = (int)roundedF;
                localF = 0;
            }
            else
            {
                idx = (int)f;
                localF = f - idx;
            }

            if (idx == segmentsCount)
            {
                idx--; localF = 1;
            }

            return this[idx];
        }

        /// <summary>
        /// Gets the segment for a certain TF
        /// </summary>
        /// <param name="tf">the TF value</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>the segment the given TF is inside</returns>
        public CurvySplineSegment TFToSegment(float tf, CurvyClamping clamping)
        {
            float f;
            return TFToSegment(tf, out f, clamping);
        }

        /// <summary>
        /// Gets the segment for a certain TF clamped to 0..1
        /// </summary>
        /// <param name="tf">the TF value</param>
        /// <returns>the segment the given TF is inside</returns>
        public CurvySplineSegment TFToSegment(float tf)
        {
            float f;
            return TFToSegment(tf, out f, CurvyClamping.Clamp);
        }
        /// <summary>
        /// Gets the segment and the local F for a certain TF clamped to 0..1
        /// </summary>
        /// <param name="tf">the TF value</param>
        /// <param name="localF">gets the remaining localF in the range 0..1</param>
        /// <returns>the segment the given TF is inside</returns>
        public CurvySplineSegment TFToSegment(float tf, out float localF)
        {
            return TFToSegment(tf, out localF, CurvyClamping.Clamp);
        }

        /// <summary>
        /// Gets a TF value from a segment
        /// </summary>
        /// <param name="segment">a segment</param>
        /// <returns>a TF value in the range 0..1</returns>
        public float SegmentToTF(CurvySplineSegment segment)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif
            return SegmentToTF(segment, 0);
        }

        /// <summary>
        /// Gets a TF value from a segment and a local F
        /// </summary>
        /// <param name="segment">a segment</param>
        /// <param name="localF">F of this segment in the range 0..1</param>
        /// <returns>a TF value in the range 0..1</returns>
        public float SegmentToTF(CurvySplineSegment segment, float localF)
        {
#if CONTRACTS_FULL
            Contract.Requires(segment.Spline == this);
#endif
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float result;
            int segmentIndex = GetSegmentIndex(segment);
            if (segmentIndex != -1)
                result = ((float)segmentIndex / Count) + (1f / Count) * localF;
            else
            {
                if (Closed == false && segment == LastVisibleControlPoint)
                    result = 1;
                else if (Count == 0)
                    result = 0;
                else
                {
                    short controlPointIndex = GetControlPointIndex(segment);

                    if (AutoEndTangents == false && controlPointIndex == 0)
                        result = 0;
                    else if (AutoEndTangents == false && controlPointIndex == ControlPointCount - 1)
                        result = 1;
                    else
                    {
                        DTLog.LogError("[Curvy] SegmentToTF reached an unexpected case. Please raise a bug report.");
                        result = -1;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Converts a distance to a TF value
        /// </summary>
        /// <param name="distance">distance</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>a TF value in the range 0..1</returns>
        public float DistanceToTF(float distance, CurvyClamping clamping = CurvyClamping.Clamp)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif
            float result;
            if (Length == 0)
                result = 0;
            else if (distance == 0)
                result = 0;
            else if (distance == Length)
                result = 1;
            else
            {
                float localDistance;
                // Get the segment the distance lies within
                CurvySplineSegment seg = DistanceToSegment(distance, out localDistance, clamping);
                result = (seg) ? SegmentToTF(seg, seg.DistanceToLocalF(localDistance)) : 0;
            }
            return result;
        }

        /// <summary>
        /// Gets the segment a certain distance lies within
        /// </summary>
        /// <param name="distance">a distance in the range 0..Length</param>
        /// <param name="clamping">clamping to use</param>
        /// <returns>a spline segment or null</returns>
        public CurvySplineSegment DistanceToSegment(float distance, CurvyClamping clamping = CurvyClamping.Clamp)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            float d;
            return DistanceToSegment(distance, out d, clamping);
        }



        /// <summary>
        /// Gets the segment a certain distance lies within
        /// </summary>
        /// <param name="distance">a distance in the range 0..Length</param>
        /// <param name="localDistance">gets the remaining distance inside the segment</param>
        /// <param name="clamping">clamping to use</param>
        /// <returns>a spline segment</returns>
        public CurvySplineSegment DistanceToSegment(float distance, out float localDistance, CurvyClamping clamping = CurvyClamping.Clamp)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            distance = CurvyUtility.ClampDistance(distance, clamping, Length);
            CurvySplineSegment resultSegment;
            if (Count > 0)
            {
                if (distance >= LastSegment.Distance)
                {
                    resultSegment = LastSegment;
                    localDistance = Mathf.Approximately(distance, Length)
                        ? resultSegment.Length //This case was added because when distance is equal to Length, and for long splines (like in scen 98_Curvy, last control point), distance - resultSegment.Distance might a value slightly different than resultSegment.Length, which leads to dead loops in SplineController.
                        : (distance - resultSegment.Distance);
                }
                else
                {
                    CurvySplineSegment currentSegment;
                    //OPTIM get rid of _lastDistToSeg, and use a sorted array or list of cps' distances, on which you will find the right cp by using dichotomy
                    if (_lastDistToSeg != null && distance >= _lastDistToSeg.Distance)
                        currentSegment = _lastDistToSeg;
                    else
#pragma warning disable 618
                        currentSegment = Segments[0];
#pragma warning restore 618


                    int dead = Count;

                    CurvySplineSegment nextSegment = GetNextSegment(currentSegment);
                    while (nextSegment && distance >= nextSegment.Distance && dead-- > 0)
                    {
                        currentSegment = nextSegment;
                        nextSegment = GetNextSegment(currentSegment);
                    }

                    if (dead <= 0)
                    {
                        DTLog.LogError("[Curvy] CurvySpline.DistanceToSegment() caused a deadloop! This shouldn't happen at all. Please raise a bug report.");
                    }

                    resultSegment = currentSegment;
                    localDistance = distance.Approximately(resultSegment.Distance) ? 0 : distance - resultSegment.Distance;

                    if (Dirty == false) //TODO remove this test once Curvy has the sanity checks enabled in prod, because when they are, executing this code with Dirty == true would lead to console errors/warnings
                        _lastDistToSeg = resultSegment;
                }
            }
            else
            {
                resultSegment = null;
                localDistance = -1;
            }

            return resultSegment;
        }


        #endregion

        #region Move along spline methods

        /// <summary>
        /// Alter TF to reflect a movement over a certain portion of the spline/group
        /// </summary>
        /// <remarks>fDistance relates to the total spline, so longer splines will result in faster movement for constant fDistance</remarks>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="fDistance">the percentage of the spline/group to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 Move(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
        {
            tf = CurvyUtility.ClampTF(tf + fDistance * direction, ref direction, clamping);
            return Interpolate(tf);
        }

        /// <summary>
        /// Alter TF to reflect a movement over a certain portion of the spline/group, respecting Clamping. Unlike Move() a linear approximation will be used
        /// </summary>
        /// <remarks>fDistance relates to the total spline, so longer splines will result in faster movement for constant fDistance</remarks>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="fDistance">the percentage of the spline/group to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 MoveFast(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
        {
            tf = CurvyUtility.ClampTF(tf + fDistance * direction, ref direction, clamping);
            return InterpolateFast(tf);
        }

        /// <summary>
        /// Alter TF to reflect a movement over a certain distance
        /// </summary>
        /// <remarks>MoveBy works by extrapolating current curvation, so results may be inaccurate for large distances</remarks>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="distance">the distance in world units to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 MoveBy(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize = 0.002f)
        {
            return Move(ref tf, ref direction, ExtrapolateDistanceToTF(tf, distance, stepSize), clamping);
        }


        /// <summary>
        /// Alter TF to reflect a movement over a certain distance. Unlike MoveBy, a linear approximation will be used
        /// </summary>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="distance">the distance in world units to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 MoveByFast(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize = 0.002f)
        {
            return MoveFast(ref tf, ref direction, ExtrapolateDistanceToTFFast(tf, distance, stepSize), clamping);
        }

        /// <summary>
        /// Alter TF to reflect a movement over a certain distance.
        /// </summary>
        /// <remarks>MoveByLengthFast works by using actual lengths</remarks>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="distance">the distance in world units to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 MoveByLengthFast(ref float tf, ref int direction, float distance, CurvyClamping clamping)
        {
            float dist = ClampDistance(TFToDistance(tf) + distance * direction, ref direction, clamping);
            tf = DistanceToTF(dist);
            return InterpolateFast(tf);
        }

        /// <summary>
        /// Alter TF to move until the curvation reached angle.
        /// </summary>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="angle">the angle in degrees</param>
        /// <param name="clamping">the clamping mode. CurvyClamping.PingPong isn't supported!</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 MoveByAngle(ref float tf, ref int direction, float angle, CurvyClamping clamping, float stepSize = 0.002f)
        {
            if (clamping == CurvyClamping.PingPong)
            {
                DTLog.LogError("[Curvy] MoveByAngle does not support PingPong clamping");
                return Vector3.zero;
            }
            stepSize = Mathf.Max(0.0001f, stepSize);
            float initialTF = tf;
            Vector3 initialP;
            Vector3 initialT;
            InterpolateAndGetTangent(tf, out initialP, out initialT);
            Vector3 P2 = Vector3.zero;
            Vector3 T2;
            int deadlock = 10000;
            while (deadlock-- > 0)
            {
                tf += stepSize * direction;
                if (tf > 1)
                {
                    if (clamping == CurvyClamping.Loop)
                        tf -= 1;
                    else
                    {
                        tf = 1;
                        return Interpolate(1);
                    }
                }
                else if (tf < 0)
                {
                    if (clamping == CurvyClamping.Loop)
                        tf += 1;
                    else
                    {
                        tf = 0;
                        return Interpolate(0);
                    }
                }
                P2 = Interpolate(tf);
                T2 = P2 - initialP;
                float accAngle = Vector3.Angle(initialT, T2);
                if (accAngle >= angle)
                {
                    tf = initialTF + (tf - initialTF) * angle / accAngle;
                    return Interpolate(tf);
                }
            }
            return P2;

        }

        /// <summary>
        /// Alter TF to move until the curvation reached angle. Unlike MoveByAngle, a linear approximation will be used
        /// </summary>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="angle">the angle in degrees</param>
        /// <param name="clamping">the clamping mode. CurvyClamping.PingPong isn't supported!</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the interpolated position, relative to the spline's local space</returns>
        public Vector3 MoveByAngleFast(ref float tf, ref int direction, float angle, CurvyClamping clamping, float stepSize)
        {
            if (clamping == CurvyClamping.PingPong)
            {
                DTLog.LogError("[Curvy] MoveByAngleFast does not support PingPong clamping");
                return Vector3.zero;
            }

            stepSize = Mathf.Max(0.0001f, stepSize);
            float initialTF = tf;
            Vector3 initialP;
            Vector3 initialT;
            InterpolateAndGetTangentFast(tf, out initialP, out initialT);
            Vector3 P2 = Vector3.zero;
            Vector3 T2;
            int deadlock = 10000;
            while (deadlock-- > 0)
            {
                tf += stepSize * direction;
                if (tf > 1)
                {
                    if (clamping == CurvyClamping.Loop)
                        tf -= 1;
                    else
                    {
                        tf = 1;
                        return InterpolateFast(1);
                    }
                }
                else if (tf < 0)
                {
                    if (clamping == CurvyClamping.Loop)
                        tf += 1;
                    else
                    {
                        tf = 0;
                        return InterpolateFast(0);
                    }
                }
                P2 = InterpolateFast(tf);
                T2 = P2 - initialP;
                float accAngle = Vector3.Angle(initialT, T2);
                if (accAngle >= angle)
                {
                    tf = initialTF + (tf - initialTF) * angle / accAngle;
                    return InterpolateFast(tf);
                }
            }
            return P2;
        }

        #endregion


        #region Clamping

        /// <summary>
        /// Clamps absolute position
        /// </summary>
        public float ClampDistance(float distance, CurvyClamping clamping)
        {
            return CurvyUtility.ClampDistance(distance, clamping, Length);
        }

        /// <summary>
        /// Clamps absolute position
        /// </summary>
        public float ClampDistance(float distance, CurvyClamping clamping, float min, float max)
        {
            return CurvyUtility.ClampDistance(distance, clamping, Length, min, max);
        }

        /// <summary>
        /// Clamps absolute position and sets new direction
        /// </summary>
        public float ClampDistance(float distance, ref int dir, CurvyClamping clamping)
        {
            return CurvyUtility.ClampDistance(distance, ref dir, clamping, Length);
        }

        /// <summary>
        /// Clamps absolute position and sets new direction
        /// </summary>
        public float ClampDistance(float distance, ref int dir, CurvyClamping clamping, float min, float max)
        {
            return CurvyUtility.ClampDistance(distance, ref dir, clamping, Length, min, max);
        }

        #endregion

        #region --- General ---

        /// <summary>
        /// Adds a Control Point and refreshes the spline
        /// </summary>
        /// <returns>a Control Point</returns>
        public CurvySplineSegment Add() { return InsertAfter(null, false); }

        /// <summary>
        /// Adds several Control Points at once and refresh the spline
        /// </summary>
        /// <param name="controlPointsLocalPositions">one or more positions in the spline's local space</param>
        /// <returns>an array containing the new Control Points</returns>
        public CurvySplineSegment[] Add(params Vector3[] controlPointsLocalPositions)
        {
            OnBeforeControlPointAddEvent(defaultAddAfterEventArgs);

            CurvySplineSegment[] cps = new CurvySplineSegment[controlPointsLocalPositions.Length];
            Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
            for (int i = 0; i < controlPointsLocalPositions.Length; i++)
                cps[i] = InsertAfter(null, localToWorldMatrix.MultiplyPoint3x4(controlPointsLocalPositions[i]), true);

            Refresh();
            OnAfterControlPointAddEvent(defaultAddAfterEventArgs);
            OnAfterControlPointChangesEvent(defaultSplineEventArgs);

            return cps;
        }

        /// <summary>
        /// Inserts a Control Point before a given Control Point
        /// </summary>
        /// <remarks>If you add several Control Points in a row, using <see cref="Add(Vector3[] controlPoints)"/> will be more efficient</remarks>
        /// <param name="controlPoint">A control point of the spline, before which the new control point will be added. If null, the CP will be added at the start of the spline</param>
        /// <returns>a Control Point</returns>
        /// <param name="skipRefreshingAndEvents">If true, the spline's <see cref="Refresh"/> method will not be called, and the relevant events will not be triggered</param>
        public CurvySplineSegment InsertBefore(CurvySplineSegment controlPoint, bool skipRefreshingAndEvents = false)
        {
            Vector3 position;
            if (controlPoint)
            {
                CurvySplineSegment previousControlPoint = GetPreviousControlPoint(controlPoint);
                if (IsControlPointASegment(previousControlPoint))
                    position = transform.localToWorldMatrix.MultiplyPoint3x4(previousControlPoint.Interpolate(0.5f));
                else
                {
                    position = previousControlPoint
                        ? Vector3.LerpUnclamped(previousControlPoint.transform.position, controlPoint.transform.position, 0.5f)
                        : controlPoint.transform.position;
                }
            }
            else
                position = transform.position;

            return InsertBefore(controlPoint, position, skipRefreshingAndEvents);
        }

        /// <summary>
        /// Inserts a Control Point before a given Control Point
        /// </summary>
        /// <remarks>If you add several Control Points in a row, using <see cref="Add(Vector3[] controlPoints)"/> will be more efficient</remarks>
        /// <param name="controlPoint">A control point of the spline, before which the new control point will be added. If null, the CP will be added at the start of the spline</param>
        /// <param name="globalPosition">The position of the control point at its creation</param>
        /// <param name="skipRefreshingAndEvents">If true, the spline's <see cref="Refresh"/> method will not be called, and the relevant events will not be triggered</param>
        /// <returns>a Control Point</returns>
        public CurvySplineSegment InsertBefore(CurvySplineSegment controlPoint, Vector3 globalPosition, bool skipRefreshingAndEvents = false)
        {
            return InsertAt(
                controlPoint,
                globalPosition,
                controlPoint
                    ? Mathf.Max(0, GetControlPointIndex(controlPoint))
                    : 0,
                CurvyControlPointEventArgs.ModeEnum.AddBefore,
                skipRefreshingAndEvents);
        }

        /// <summary>
        /// Inserts a Control Point after a given Control Point
        /// </summary>
        /// <remarks>If you add several Control Points in a row, using <see cref="Add(Vector3[] controlPoints)"/> will be more efficient</remarks>
        /// <param name="controlPoint">A control point of the spline, behind which the new control point will be added. If null, the CP will be added at the end of the spline</param>
        /// <param name="skipRefreshingAndEvents">If true, the spline's <see cref="Refresh"/> method will not be called, and the relevant events will not be triggered</param>
        /// <returns>the new Control Point</returns>
        public CurvySplineSegment InsertAfter(CurvySplineSegment controlPoint, bool skipRefreshingAndEvents = false)
        {
            Vector3 position;
            if (controlPoint)
            {
                if (IsControlPointASegment(controlPoint))
                    position = transform.localToWorldMatrix.MultiplyPoint3x4(controlPoint.Interpolate(0.5f));
                else
                {
                    CurvySplineSegment nextControlPoint = GetNextControlPoint(controlPoint);
                    position = nextControlPoint
                        ? Vector3.LerpUnclamped(nextControlPoint.transform.position, controlPoint.transform.position, 0.5f)
                        : controlPoint.transform.position;
                }
            }
            else
                position = transform.position;

            return InsertAfter(controlPoint, position, skipRefreshingAndEvents);
        }

        /// <summary>
        /// Inserts a Control Point after a given Control Point
        /// </summary>
        /// <remarks>If you add several Control Points in a row, using <see cref="Add(Vector3[] controlPoints)"/> will be more efficient</remarks>
        /// <param name="controlPoint">A control point of the spline, behind which the new control point will be added. If null, the CP will be added at the end of the spline</param>
        /// <param name="globalPosition">The position of the control point at its creation</param>
        /// <param name="skipRefreshingAndEvents">If true, the spline's <see cref="Refresh"/> method will not be called, and the relevant events will not be triggered</param>
        /// <returns>the new Control Point</returns>
        public CurvySplineSegment InsertAfter(CurvySplineSegment controlPoint, Vector3 globalPosition, bool skipRefreshingAndEvents = false)
        {
            return InsertAt(
                controlPoint,
                globalPosition,
                 controlPoint
                    ? GetControlPointIndex(controlPoint) + 1
                    : ControlPoints.Count,
                CurvyControlPointEventArgs.ModeEnum.AddAfter,
                skipRefreshingAndEvents);
        }

        /// <summary>
        /// Removes all control points
        /// </summary>
        public void Clear()
        {
            OnBeforeControlPointDeleteEvent(defaultDeleteEventArgs);

            for (int i = ControlPointCount - 1; i >= 0; i--)
                if (UsePooling && Application.isPlaying)
                {
                    CurvyGlobalManager curvyGlobalManager = CurvyGlobalManager.Instance;
                    if (curvyGlobalManager == null)
                        DTLog.LogError("[Curvy] Couldn't find Curvy Global Manager. Please raise a bug report.");
                    else
                        curvyGlobalManager.ControlPointPool.Push(ControlPoints[i]);
                }
                else
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        //BUG I am sure this is source of bug if the scene has an id 42. Check this potential issue
                        _newSelectionInstanceIDINTERNAL = 42; // Prevent selection of another CP/Spline
                        Undo.DestroyObjectImmediate(ControlPoints[i].gameObject);
                    }
                    else
                        DestroyImmediate(ControlPoints[i].gameObject);
#else
                        Destroy(controlPoints[i].gameObject);
#endif
                }

            ClearControlPoints();

            Refresh();
            OnAfterControlPointChangesEvent(defaultSplineEventArgs);
#if UNITY_EDITOR
            _newSelectionInstanceIDINTERNAL = 0;
#endif
        }

        /// <summary>
        /// Deletes a Control Point
        /// </summary>
        /// <param name="controlPoint">a Control Point</param>
        /// <param name="skipRefreshingAndEvents">If true, the spline's <see cref="Refresh"/> method will not be called, and the relevant events will not be triggered</param>
        public void Delete(CurvySplineSegment controlPoint, bool skipRefreshingAndEvents = false)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPoints.Contains(controlPoint));
#endif
            if (!controlPoint)
                return;

            if (skipRefreshingAndEvents == false)
                OnBeforeControlPointDeleteEvent(new CurvyControlPointEventArgs(this, this, controlPoint, CurvyControlPointEventArgs.ModeEnum.Delete));

            RemoveControlPoint(controlPoint);
            controlPoint.transform.SetAsLastSibling();// IMPORTANT! Runtime Delete is delayed, so we need to make sure it got sorted to the end 
            if (UsePooling && Application.isPlaying)
            {
                CurvyGlobalManager curvyGlobalManager = CurvyGlobalManager.Instance;
                if (curvyGlobalManager == null)
                    DTLog.LogError("[Curvy] Couldn't find Curvy Global Manager. Please raise a bug report.");
                else
                    curvyGlobalManager.ControlPointPool.Push(controlPoint);
            }
            else
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    _newSelectionInstanceIDINTERNAL = 42; // Prevent selection of another CP/Spline
                    Undo.DestroyObjectImmediate(controlPoint.gameObject);
                }
                else
                    DestroyImmediate(controlPoint.gameObject);
#else
                        Destroy(controlPoint.gameObject);
#endif
            }

            if (skipRefreshingAndEvents == false)
            {
                Refresh();
                OnAfterControlPointChangesEvent(defaultSplineEventArgs);
            }
        }

        /// <summary>
        /// Gets an array containing all approximation points
        /// </summary>
        /// <param name="space">Whether to use local or global space</param>
        /// <remarks>This can be used to feed meshbuilders etc...</remarks>
        /// <returns>an array of world/local positions</returns>
        public Vector3[] GetApproximation(Space space = Space.Self)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            Vector3[] apps = new Vector3[CacheSize + 1];
            int idx = 0;
            for (int si = 0; si < Count; si++)
            {
                this[si].Approximation.CopyTo(apps, idx);
                idx += Mathf.Max(0, this[si].Approximation.Length - 1);
            }

            if (space == Space.World)
            {
                Matrix4x4 m = transform.localToWorldMatrix;
                for (int i = 0; i < apps.Length; i++)
                    apps[i] = m.MultiplyPoint3x4(apps[i]);
            }

            return apps;
        }

        /// <summary>
        /// Gets an array containing all approximation tangents
        /// </summary>
        /// <remarks>This can be used to feed meshbuilders etc...</remarks>
        /// <returns>an array of tangents</returns>
        public Vector3[] GetApproximationT()
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            Vector3[] apps = new Vector3[CacheSize + 1];
            int idx = 0;
            for (int si = 0; si < Count; si++)
            {
                this[si].ApproximationT.CopyTo(apps, idx);
                idx += Mathf.Max(0, this[si].ApproximationT.Length - 1);
            }
            return apps;
        }

        /// <summary>
        /// Gets an array containing all approximation Up-Vectors
        /// </summary>
        /// <remarks>This can be used to feed meshbuilders etc...</remarks>
        /// <returns>an array of Up-Vectors</returns>
        public Vector3[] GetApproximationUpVectors()
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            Vector3[] apps = new Vector3[CacheSize + 1];
            int idx = 0;
            for (int si = 0; si < Count; si++)
            {
                this[si].ApproximationUp.CopyTo(apps, idx);
                idx += Mathf.Max(0, this[si].ApproximationUp.Length - 1);
            }
            return apps;
        }

        /// <summary>
        /// Gets the TF value that is nearest to localPosition
        /// </summary>
        /// <param name="localPosition">a point in spline's local space</param>
        /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
        /// <returns>a TF value in the range 0..1 or -1 on error</returns>
        public float GetNearestPointTF(Vector3 localPosition)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            Vector3 v;
            return GetNearestPointTF(localPosition, out v, 0, -1);
        }

        /// <summary>
        /// Gets the TF value that is nearest to localPosition
        /// </summary>
        /// <param name="localPosition">a point in spline's local space</param>
        /// <param name="nearest">returns the nearest position</param>
        /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
        /// <returns>a TF value in the range 0..1 or -1 on error</returns>
        public float GetNearestPointTF(Vector3 localPosition, out Vector3 nearest)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            return GetNearestPointTF(localPosition, out nearest, 0, -1);
        }

        /// <summary>
        /// Gets the TF value that is nearest to localPosition
        /// </summary>
        /// <param name="localPosition">a point in spline's local space</param>
        /// <param name="startSegmentIndex">the segment index to start searching</param>
        /// <param name="stopSegmentIndex">the segment index to stop searching or -1 to search until end</param>
        /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
        /// <returns>a TF value in the range 0..1 or -1 on error</returns>
        public float GetNearestPointTF(Vector3 localPosition, int startSegmentIndex = 0, int stopSegmentIndex = -1)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            Vector3 v;
            return GetNearestPointTF(localPosition, out v, startSegmentIndex, stopSegmentIndex);
        }

        /// <summary>
        /// Gets the TF value that is nearest to localPosition
        /// </summary>
        /// <param name="localPosition">a point in spline's local space</param>
        /// <param name="nearest">returns the nearest position</param>
        /// <param name="startSegmentIndex">the segment index to start searching</param>
        /// <param name="stopSegmentIndex">the segment index to stop searching or -1 to search until end</param>
        /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
        /// <returns>a TF value in the range 0..1 or -1 on error</returns>
        public float GetNearestPointTF(Vector3 localPosition, out Vector3 nearest, int startSegmentIndex = 0, int stopSegmentIndex = -1)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            CurvySplineSegment segement;
            float f;
            return GetNearestPointTF(localPosition, out nearest, out segement, out f, startSegmentIndex, stopSegmentIndex);
        }

        /// <summary>
        /// Gets the TF value that is nearest to localPosition
        /// </summary>
        /// <param name="localPosition">a point in spline's local space</param>
        /// <param name="nearestPoint">returns the nearest position</param>
        /// <param name="nearestSegment">returns the nearest segment</param>
        /// <param name="nearestSegmentF">LocalF of the nearest position on the nearest segment</param>
        /// <param name="startSegmentIndex">the segment index to start searching</param>
        /// <param name="stopSegmentIndex">the segment index to stop searching or -1 to search until end</param>
        /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
        /// <returns>a TF value in the range 0..1 or -1 on error</returns>
        public float GetNearestPointTF(Vector3 localPosition, out Vector3 nearestPoint, [CanBeNull] out CurvySplineSegment nearestSegment, out float nearestSegmentF, int startSegmentIndex = 0, int stopSegmentIndex = -1)
        {
#if CURVY_SANITY_CHECKS
            DoSanityChecks();
#endif

            nearestPoint = Vector3.zero;
            if (Count == 0)
            {
                nearestSegment = null;
                nearestSegmentF = -1;
                return -1;
            }
            // for each segment, get the distance to it's approximation points
            float distSqr = float.MaxValue;
            float resF = 0;

            CurvySplineSegment resSeg = null;
            if (stopSegmentIndex == -1)
                stopSegmentIndex = Count - 1;
            startSegmentIndex = Mathf.Clamp(startSegmentIndex, 0, Count - 1);
            stopSegmentIndex = Mathf.Clamp(stopSegmentIndex + 1, startSegmentIndex + 1, Count);
            for (int i = startSegmentIndex; i < stopSegmentIndex; i++)
            {
                float f = this[i].GetNearestPointF(localPosition);
                Vector3 v = this[i].Interpolate(f);
                float magSqr = (v - localPosition).sqrMagnitude;
                if (magSqr <= distSqr)
                {
                    resSeg = this[i];
                    resF = f;
                    nearestPoint = v;
                    distSqr = magSqr;
                }
            }

            nearestSegment = resSeg;
            nearestSegmentF = resF;
            // return the nearest
            return resSeg.LocalFToTF(resF);
        }

        /// <summary>
        /// Refreshs the spline
        /// </summary>
        /// <remarks>This is called automatically on the next Update() if any changes are pending</remarks>
        public void Refresh()
        {
            ProcessDirtyControlPoints();
            //DESIGN OPTIM shouldn't this be called only if their was actual dirty data processed?
            OnRefreshEvent(defaultSplineEventArgs);
        }

        /// <summary>
        /// Ensures the whole spline (curve & orientation) will be recalculated on next call to Refresh()
        /// </summary>
        public void SetDirtyAll()
        {
            SetDirtyAll(SplineDirtyingType.Everything, true);
        }

        /// <summary>
        /// Ensure the whole spline will be recalculated on next call to Refresh()
        /// </summary>
        /// <param name="dirtyingType">Defines what aspect should be dirtied</param>
        /// <param name="dirtyConnectedControlPoints">whether to set dirty other splines's control points connected to one of this spline's control points</param>
        public void SetDirtyAll(SplineDirtyingType dirtyingType, bool dirtyConnectedControlPoints)
        {
#if CURVY_LOG_DIRTYING
            Debug.Log("Set Dirty all " + dirtyingType);
#endif
            allControlPointsAreDirty = true;
            SetDirtyingFlags(dirtyingType);

            if (dirtyConnectedControlPoints)
                //OPTIM: maintain a boolean saying if any control point has a connection. And then, avoid the following code if that boolean is false. Same suggestion for other dirtying codes related to connections
                for (int index = 0; index < ControlPoints.Count; index++)
                {
                    CurvySplineSegment controlPoint = ControlPoints[index];
                    if (controlPoint && controlPoint.Connection)//controlPoint can be null if you create a spline via the pen tool, and then undo it
                    {
                        ReadOnlyCollection<CurvySplineSegment> connectedControlPoints = controlPoint.Connection.ControlPointsList;
                        for (int connectedIndex = 0; connectedIndex < connectedControlPoints.Count; connectedIndex++)
                        {
                            CurvySplineSegment connectedControlPoint = connectedControlPoints[connectedIndex];
                            CurvySpline connectedSpline = connectedControlPoint != null
                                ? connectedControlPoint.Spline
                                : null;
                            if (connectedSpline && connectedSpline != this)
                            {
                                connectedSpline.dirtyControlPointsMinimalSet.Add(connectedControlPoint);
                                connectedSpline.SetDirtyingFlags(dirtyingType);
                            }
                        }
                    }
                }
        }

        /// <summary>
        /// Marks a Control Point to get recalculated on next call to Refresh(). Will also mark connected control points and control points that depend on the current one through the Follow-Up feature.
        /// </summary>
        /// <param name="dirtyControlPoint">the Control Point to dirty</param>
        /// <param name="dirtyingType">Defines what aspect should be dirtied</param>
        public void SetDirty(CurvySplineSegment dirtyControlPoint, SplineDirtyingType dirtyingType)
        {
            SetDirty(dirtyControlPoint, dirtyingType, GetPreviousControlPoint(dirtyControlPoint), GetNextControlPoint(dirtyControlPoint), false);
        }

        /// <summary>
        /// Marks a Control Point to get recalculated on next call to Refresh(). Will also mark connected control points and control points that depend on the current one through the Follow-Up feature. Be aware, this method, and unlike SetDirty, will not mark as dirty the control points connected to the "controlPoint" parameter
        /// </summary>
        /// <param name="dirtyControlPoint">the Control Point to dirty</param>
        /// <param name="dirtyingType">Defines what aspect should be dirtied</param>
        public void SetDirtyPartial(CurvySplineSegment dirtyControlPoint, SplineDirtyingType dirtyingType)
        {
            //OPTIM skip this if dirtyControlPoint is already dirty?
            SetDirty(dirtyControlPoint, dirtyingType, GetPreviousControlPoint(dirtyControlPoint), GetNextControlPoint(dirtyControlPoint), true);
        }

        /// <summary>
        /// Shortcut to transform.TransformPoint(localPosition)
        /// </summary>
        public Vector3 ToWorldPosition(Vector3 localPosition)
        {
            return transform.TransformPoint(localPosition);
        }

#if UNITY_EDITOR

        /// <summary>
        /// Apply proper names to all Control Points
        /// </summary>
        public void ApplyControlPointsNames()
        {
            // rename them and set their order based on ControlPoint list
            for (short i = 0; i < ControlPoints.Count; i++)
                ControlPoints[i].name = GetControlPointName(i);
        }

#endif

        /// <summary>
        /// Rebuilds the ControlPoints list from the hierarchy. It sets the spline as Dirty
        /// </summary>
        public void SyncSplineFromHierarchy()
        {
            ClearControlPoints();
            for (int i = 0; i < transform.childCount; i++)
            {
                CurvySplineSegment cp = transform.GetChild(i).GetComponent<CurvySplineSegment>();
                if (cp)
                    AddControlPoint(cp);
            }
        }

        /// <summary>
        /// Gets all Approximation points for a given spline part
        /// </summary>
        /// <param name="fromTF">start TF</param>
        /// <param name="toTF">end TF</param>
        /// <param name="includeEndPoint">Whether the end position should be included</param>
        /// <returns>an array of Approximation points</returns>
        public Vector3[] GetApproximationPoints(float fromTF, float toTF, bool includeEndPoint = true)
        {
            float startLF;
            float startFrag;
            float endLF;
            float endFrag;
            CurvySplineSegment startSeg = TFToSegment(fromTF, out startLF);
            int startIdx = startSeg.getApproximationIndexINTERNAL(startLF, out startFrag);
            CurvySplineSegment endSeg = TFToSegment(toTF, out endLF);
            int endIdx = endSeg.getApproximationIndexINTERNAL(endLF, out endFrag);

            CurvySplineSegment seg = startSeg;
            Vector3[] res = new Vector3[1] { Vector3.Lerp(seg.Approximation[startIdx], seg.Approximation[startIdx + 1], startFrag) };
            //if (startFrag == 1)
            //    seg = seg.NextSegment;
            while (seg && seg != endSeg)
            {
                res = res.AddRange<Vector3>(seg.Approximation.SubArray<Vector3>(startIdx + 1, seg.Approximation.Length - 1));
                startIdx = 1;
                seg = seg.Spline.GetNextSegment(seg);
            }
            if (seg)
            {
                int i = (startSeg == seg) ? startIdx + 1 : 1;
                res = res.AddRange<Vector3>(seg.Approximation.SubArray<Vector3>(i, endIdx - i));
                if (includeEndPoint && (endFrag > 0 || endFrag < 1))
                    return res.Add<Vector3>(Vector3.Lerp(seg.Approximation[endIdx], seg.Approximation[endIdx + 1], endFrag));
            }
            return res;
        }

        #endregion

        #region --- Utilities ---

        /// <summary>
        /// Checks if the curve is planar
        /// </summary>
        /// <param name="ignoreAxis">returns the axis that can be ignored (0=x,1=y,2=z)</param>
        /// <returns>true if a planar axis was found</returns>
        public bool IsPlanar(out int ignoreAxis)
        {
            bool xp, yp, zp;
            bool res = IsPlanar(out xp, out yp, out zp);
            if (xp)
                ignoreAxis = 0;
            else if (yp)
                ignoreAxis = 1;
            else
                ignoreAxis = 2;
            return res;
        }

        /// <summary>
        /// Checks if the curve is planar
        /// </summary>
        /// <param name="xplanar">whether the x-axis is planar</param>
        /// <param name="yplanar">whether the y-axis is planar</param>
        /// <param name="zplanar">whether the z-axis is planar</param>
        /// <returns>true if at least on axis is planar</returns>
        public bool IsPlanar(out bool xplanar, out bool yplanar, out bool zplanar)
        {
            xplanar = true;
            yplanar = true;
            zplanar = true;
            if (ControlPointCount == 0) return true;
            Vector3 p = ControlPoints[0].transform.localPosition;
            for (int i = 1; i < ControlPointCount; i++)
            {
                if (!Mathf.Approximately(ControlPoints[i].transform.localPosition.x, p.x))
                    xplanar = false;
                if (!Mathf.Approximately(ControlPoints[i].transform.localPosition.y, p.y))
                    yplanar = false;
                if (!Mathf.Approximately(ControlPoints[i].transform.localPosition.z, p.z))
                    zplanar = false;

                if (xplanar == false && yplanar == false && zplanar == false)
                    return false;

            }
            return true;
        }

        /// <summary>
        /// Determines if the spline is at zero position on a certain plane
        /// </summary>
        /// <param name="plane">the plane the spline should be tested against</param>
        /// <returns>true if the spline is on the plane</returns>
        public bool IsPlanar(CurvyPlane plane)
        {
            switch (plane)
            {
                case CurvyPlane.XY:
                    for (int i = 0; i < ControlPointCount; i++)
                        if (ControlPoints[i].transform.localPosition.z != 0)
                            return false;
                    break;
                case CurvyPlane.XZ:
                    for (int i = 0; i < ControlPointCount; i++)
                        if (ControlPoints[i].transform.localPosition.y != 0)
                            return false;
                    break;
                case CurvyPlane.YZ:
                    for (int i = 0; i < ControlPointCount; i++)
                        if (ControlPoints[i].transform.localPosition.x != 0)
                            return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// Forces the spline to be at zero position on a certain plane
        /// </summary>
        /// <param name="plane">the plane the should be on</param>
        public void MakePlanar(CurvyPlane plane)
        {
            switch (plane)
            {
                case CurvyPlane.XY:
                    for (int i = 0; i < ControlPointCount; i++)
                        if (ControlPoints[i].transform.localPosition.z != 0)
                            ControlPoints[i].SetLocalPosition(new Vector3(ControlPoints[i].transform.localPosition.x, ControlPoints[i].transform.localPosition.y, 0));
                    break;
                case CurvyPlane.XZ:
                    for (int i = 0; i < ControlPointCount; i++)
                        if (ControlPoints[i].transform.localPosition.y != 0)
                            ControlPoints[i].SetLocalPosition(new Vector3(ControlPoints[i].transform.localPosition.x, 0, ControlPoints[i].transform.localPosition.z));
                    break;
                case CurvyPlane.YZ:
                    for (int i = 0; i < ControlPointCount; i++)
                        if (ControlPoints[i].transform.localPosition.x != 0)
                            ControlPoints[i].SetLocalPosition(new Vector3(0, ControlPoints[i].transform.localPosition.y, ControlPoints[i].transform.localPosition.z));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Subdivides the spline, i.e. adds additional segments to a certain range
        /// </summary>
        /// <param name="fromCP">starting ControlPoint</param>
        /// <param name="toCP">ending ControlPoint</param>
        public void Subdivide(CurvySplineSegment fromCP = null, CurvySplineSegment toCP = null)
        {
            if (!fromCP)
                fromCP = FirstVisibleControlPoint;
            if (!toCP)
                toCP = LastVisibleControlPoint;

            if (fromCP == null || toCP == null || fromCP.Spline != this || toCP.Spline != this)
            {
                Debug.Log("CurvySpline.Subdivide: Not a valid range selection!");
                return;
            }

            int startCPIndex = Mathf.Clamp(fromCP.Spline.GetControlPointIndex(fromCP), 0, ControlPointCount - 2);
            int endCPIndex = Mathf.Clamp(toCP.Spline.GetControlPointIndex(toCP), startCPIndex + 1, ControlPointCount - 1);

            if (endCPIndex - startCPIndex < 1)
            {
                Debug.Log("CurvySpline.Subdivide: Not a valid range selection!");
            }
            else
            {
                OnBeforeControlPointAddEvent(defaultAddAfterEventArgs);

                Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;

                for (int i = endCPIndex - 1; i >= startCPIndex; i--)
                {
                    CurvySplineSegment previousControlPoint = ControlPoints[i];
                    CurvySplineSegment nextControlPoint = ControlPoints[i + 1];
                    CurvySplineSegment newControlPoint =
                        InsertAfter(
                        ControlPoints[i],
                        localToWorldMatrix.MultiplyPoint3x4(ControlPoints[i].Interpolate(0.5f)),
                        true);

                    if (Interpolation == CurvyInterpolation.Bezier)
                    {
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                        {
                            Undo.RecordObject(nextControlPoint, "Subdivide");
                            Undo.RecordObject(previousControlPoint, "Subdivide");
                        }
#endif

                        //Update Bézier handles to maintain the spline's shape
                        //Based on De Casteljau's algorithm. The following is it's special case implementation for a subdivision at the middle of a spline segment.
                        //Here is a picture explaining things: https://jeremykun.files.wordpress.com/2013/05/subdivision.png from https://jeremykun.com/2013/05/11/bezier-curves-and-picasso/

                        Vector3 P0 = previousControlPoint.transform.position;
                        Vector3 P1 = previousControlPoint.HandleOutPosition;
                        Vector3 P2 = nextControlPoint.HandleInPosition;
                        Vector3 P3 = nextControlPoint.transform.position;

                        Vector3 m0 = (P0 + P1) / 2;
                        Vector3 m1 = (P1 + P2) / 2;
                        Vector3 m2 = (P2 + P3) / 2;

                        Vector3 q0 = (m0 + m1) / 2;
                        Vector3 q1 = (m1 + m2) / 2;

                        previousControlPoint.AutoHandles = false;
                        previousControlPoint.HandleOutPosition = m0;

                        nextControlPoint.AutoHandles = false;
                        nextControlPoint.HandleInPosition = m2;

                        newControlPoint.AutoHandles = false;
                        newControlPoint.HandleInPosition = q0;
                        newControlPoint.HandleOutPosition = q1;
                    }
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        Undo.RegisterCreatedObjectUndo(newControlPoint.gameObject, "Subdivide");
#endif
                }

                Refresh();
                OnAfterControlPointAddEvent(defaultAddAfterEventArgs);
                OnAfterControlPointChangesEvent(defaultSplineEventArgs);
            }
        }

        /// <summary>
        /// Simplifies the spline, i.e. remove segments from a certain range
        /// </summary>
        /// <param name="fromCP">starting ControlPoint</param>
        /// <param name="toCP">ending ControlPoint</param>
        public void Simplify(CurvySplineSegment fromCP = null, CurvySplineSegment toCP = null)
        {
            if (!fromCP)
                fromCP = FirstVisibleControlPoint;
            if (!toCP)
                toCP = LastVisibleControlPoint;

            if (fromCP == null || toCP == null || fromCP.Spline != this || toCP.Spline != this)
            {
                Debug.Log("CurvySpline.Simplify: Not a valid range selection!");
                return;
            }
            int startCPIndex = Mathf.Clamp(fromCP.Spline.GetControlPointIndex(fromCP), 0, ControlPointCount - 2);
            int endCPIndex = Mathf.Clamp(toCP.Spline.GetControlPointIndex(toCP), startCPIndex + 2, ControlPointCount - 1);
            if (endCPIndex - startCPIndex < 2)
            {
                Debug.Log("CurvySpline.Simplify: Not a valid range selection!");
            }
            else
            {
                OnBeforeControlPointDeleteEvent(defaultDeleteEventArgs);

                for (int i = endCPIndex - 2; i >= startCPIndex; i -= 2)
                {
#if UNITY_EDITOR
                    _newSelectionInstanceIDINTERNAL = 42;
                    if (!Application.isPlaying)
                    {
                        Undo.SetCurrentGroupName("Simplify");
                        Undo.DestroyObjectImmediate(ControlPoints[i + 1].gameObject);
                    }
                    else
#endif
                        Delete(ControlPoints[i + 1], true);
#if UNITY_EDITOR
                    _newSelectionInstanceIDINTERNAL = 0;
#endif
                }
                Refresh();
                OnAfterControlPointChangesEvent(defaultSplineEventArgs);
            }
        }

        /// <summary>
        /// Equalizes the segment length of a certain range
        /// </summary>
        /// <param name="fromCP">starting ControlPoint</param>
        /// <param name="toCP">ending ControlPoint</param>
        public void Equalize(CurvySplineSegment fromCP = null, CurvySplineSegment toCP = null)
        {
            if (!fromCP)
                fromCP = FirstVisibleControlPoint;
            if (!toCP)
                toCP = LastVisibleControlPoint;

            if (fromCP == null || toCP == null || fromCP.Spline != this || toCP.Spline != this)
            {
                Debug.Log("CurvySpline.Equalize: Not a valid range selection!");
                return;
            }
            int startCPIndex = Mathf.Clamp(fromCP.Spline.GetControlPointIndex(fromCP), 0, ControlPointCount - 2);
            int endCPIndex = Mathf.Clamp(toCP.Spline.GetControlPointIndex(toCP), startCPIndex + 2, ControlPointCount - 1);
            if (endCPIndex - startCPIndex < 2)
            {
                Debug.Log("CurvySpline.Equalize: Not a valid range selection!");
                return;
            }
            float segmentLength = ControlPoints[endCPIndex].Distance - ControlPoints[startCPIndex].Distance;
            float equal = segmentLength / (endCPIndex - startCPIndex);
            float dist = ControlPoints[startCPIndex].Distance;
            for (int i = startCPIndex + 1; i < endCPIndex; i++)
            {
                dist += equal;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(ControlPoints[i].transform, "Equalize");
#endif
                ControlPoints[i].SetLocalPosition(InterpolateByDistance(dist));
            }
        }

        /// <summary>
        /// Applies a spline's scale to it's Control Points and resets scale
        /// </summary>
        public void Normalize()
        {
            Vector3 scl = transform.localScale;

            if (scl != Vector3.one)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(transform, "Normalize Spline");
#endif
                transform.localScale = Vector3.one;
                for (int i = 0; i < ControlPointCount; i++)
                {
                    CurvySplineSegment curvySplineSegment = ControlPoints[i];
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        Undo.RecordObject(curvySplineSegment.transform, "Normalize Spline");
                        Undo.RecordObject(curvySplineSegment, "Normalize Spline");
                    }
#endif
                    curvySplineSegment.SetLocalPosition(Vector3.Scale(curvySplineSegment.transform.localPosition, scl));
                    curvySplineSegment.HandleIn = Vector3.Scale(curvySplineSegment.HandleIn, scl);
                    curvySplineSegment.HandleOut = Vector3.Scale(curvySplineSegment.HandleOut, scl);

                }


            }
        }

        /// <summary>
        /// Equalize one axis of the spline to match the first control points's value
        /// </summary>
        /// <param name="axis">the axis to equalize (0=x,1=y,2=z)</param>
        public void MakePlanar(int axis)
        {
            Vector3 p = ControlPoints[0].transform.localPosition;
            for (int i = 1; i < ControlPointCount; i++)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(ControlPoints[i].transform, "MakePlanar");
#endif
                Vector3 pi = ControlPoints[i].transform.localPosition;
                switch (axis)
                {
                    case 0: pi.x = p.x; break;
                    case 1: pi.y = p.y; break;
                    case 2: pi.z = p.z; break;
                }
                ControlPoints[i].transform.localPosition = pi;
            }
            SetDirtyAll(SplineDirtyingType.Everything, true);
        }

        /// <summary>
        /// Sets the pivot of the spline
        /// </summary>
        /// <param name="xRel">-1 to 1</param>
        /// <param name="yRel">-1 to 1</param>
        /// <param name="zRel">-1 to 1</param>
        /// <param name="preview">if true, only return the new pivot position</param>
        /// <returns>the new pivot position</returns>
        public Vector3 SetPivot(float xRel = 0, float yRel = 0, float zRel = 0, bool preview = false)
        {
            Bounds b = Bounds;
            Vector3 v = new Vector3(b.min.x + b.size.x * ((xRel + 1) / 2),
                                    b.max.y - b.size.y * ((yRel + 1) / 2),
                                    b.min.z + b.size.z * ((zRel + 1) / 2));

            Vector3 off = transform.position - v;
            if (preview)
                return transform.position - off;

            for (int index = 0; index < ControlPoints.Count; index++)
            {
                CurvySplineSegment cp = ControlPoints[index];
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(cp.transform, "SetPivot");
#endif
                cp.transform.position += off;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RecordObject(transform, "SetPivot");
#endif
            transform.position -= off;
            SetDirtyAll(SplineDirtyingType.Everything, true);
            return transform.position;
        }

        /// <summary>
        /// Flips the direction of the spline, i.e. the first Control Point will become the last and vice versa.
        /// </summary>
        public void Flip()
        {
            if (ControlPointCount <= 1)
                return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RegisterFullObjectHierarchyUndo(this, "Flip Spline");
#endif
            switch (Interpolation)
            {
                case CurvyInterpolation.TCB:
                    Bias *= -1;
                    for (int i = ControlPointCount - 1; i >= 0; i--)
                    {
                        CurvySplineSegment cur = ControlPoints[i];

                        int j = i - 1;
                        if (j >= 0)
                        {
                            CurvySplineSegment prev = ControlPoints[j];

                            cur.EndBias = prev.StartBias * -1;
                            cur.EndContinuity = prev.StartContinuity;
                            cur.EndTension = prev.StartTension;

                            cur.StartBias = prev.EndBias * -1;
                            cur.StartContinuity = prev.EndContinuity;
                            cur.StartTension = prev.EndTension;

                            cur.OverrideGlobalBias = prev.OverrideGlobalBias;
                            cur.OverrideGlobalContinuity = prev.OverrideGlobalContinuity;
                            cur.OverrideGlobalTension = prev.OverrideGlobalTension;

                            cur.SynchronizeTCB = prev.SynchronizeTCB;
                        }
                    }
                    break;
                case CurvyInterpolation.Bezier:
                    for (int i = ControlPointCount - 1; i >= 0; i--)
                    {
                        CurvySplineSegment cur = ControlPoints[i];

                        Vector3 h = cur.HandleIn;
                        cur.HandleIn = cur.HandleOut;
                        cur.HandleOut = h;


                    }
                    break;
            }
            ReverseControlPoints();
            Refresh();
        }

        /// <summary>
        /// Moves ControlPoints from this spline, inserting them after a destination ControlPoint of another spline
        /// </summary>
        /// <param name="startIndex">ControlPointIndex of the first CP to move</param>
        /// <param name="count">number of ControlPoints to move</param>
        /// <param name="destCP">ControlPoint at the destination spline to insert after</param>
        public void MoveControlPoints(int startIndex, int count, CurvySplineSegment destCP)
        {
            if (!destCP || this == destCP.Spline || destCP.Spline.GetControlPointIndex(destCP) == -1)
                return;
            startIndex = Mathf.Clamp(startIndex, 0, ControlPointCount - 1);
            count = Mathf.Clamp(count, startIndex, ControlPointCount - startIndex);

            CurvySplineSegment cp;
            for (int i = 0; i < count; i++)
            {
                cp = ControlPoints[startIndex];
                RemoveControlPoint(cp);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.SetTransformParent(cp.transform, destCP.Spline.transform, "Move ControlPoints");
                else
#endif
                    cp.transform.SetParent(destCP.Spline.transform, true);

                destCP.Spline.InsertControlPoint(destCP.Spline.GetControlPointIndex(destCP) + i + 1, cp);
            }
            Refresh();
            destCP.Spline.Refresh();
        }

        /// <summary>
        /// Insert this spline after another spline's destination Control Point and delete this spline
        /// </summary>
        /// <param name="destCP">the Control Point of the destination spline</param>
        public void JoinWith(CurvySplineSegment destCP)
        {
            if (destCP.Spline == this)
                return;
            MoveControlPoints(0, ControlPointCount, destCP);
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.DestroyObjectImmediate(gameObject);
            else
#endif
                GameObject.Destroy(gameObject);
        }

        /// <summary>
        /// Splits this spline with the parameter controlPoint becoming the first Control Point of the new spline
        /// </summary>
        /// <returns>The new spline</returns>
        public CurvySpline Split(CurvySplineSegment controlPoint)
        {
            CurvySpline newSpline = CurvySpline.Create(this);
            newSpline.transform.SetParent(transform.parent, true);
            newSpline.name = name + "_parted";

#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RegisterCreatedObjectUndo(newSpline.gameObject, "Split Spline");
#endif

            // Move CPs
            List<CurvySplineSegment> affectedControlPoints;
            {
                int controlPointIndex = GetSegmentIndex(controlPoint);
                affectedControlPoints = new List<CurvySplineSegment>(ControlPointCount - controlPointIndex);
                for (int i = controlPointIndex; i < ControlPointCount; i++)
                    affectedControlPoints.Add(ControlPoints[i]);
            }

            for (int i = 0; i < affectedControlPoints.Count; i++)
            {
                CurvySplineSegment curvySplineSegment = affectedControlPoints[i];
                RemoveControlPoint(curvySplineSegment);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.SetTransformParent(curvySplineSegment.transform, newSpline.transform, "Split Spline");
#endif
                if (Application.isPlaying)
                    curvySplineSegment.transform.SetParent(newSpline.transform, true);
                newSpline.AddControlPoint(curvySplineSegment);
            }

            Refresh();
            newSpline.Refresh();
            return newSpline;
        }

        /// <summary>
        /// Defines the given Control Point to be the first Control Point of the spline
        /// </summary>
        /// <param name="controlPoint">One of this spline's control points</param>
        public void SetFirstControlPoint(CurvySplineSegment controlPoint)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPoints.Contains(controlPoint));
#endif
            short controlPointIndex = GetControlPointIndex(controlPoint);
            CurvySplineSegment[] controlPointsToMove = new CurvySplineSegment[controlPointIndex];
            for (int i = 0; i < controlPointIndex; i++)
                controlPointsToMove[i] = ControlPoints[i];

            for (int index = 0; index < controlPointsToMove.Length; index++)
            {
                CurvySplineSegment seg = controlPointsToMove[index];
                RemoveControlPoint(seg);
                AddControlPoint(seg);
            }
            Refresh();
        }

        #endregion

        #region Query data about control points

        /// <summary>
        /// Is the control point an orientation anchor? The answer is related to the control point's serialized OrientationAnchor value, plus it's position in the spline.
        /// </summary>
        /// <param name="controlPoint"></param>
        /// <returns></returns>
        public bool IsControlPointAnOrientationAnchor(CurvySplineSegment controlPoint)
        {
            return IsControlPointVisible(controlPoint) && (controlPoint.SerializedOrientationAnchor || (controlPoint == FirstVisibleControlPoint || controlPoint == LastVisibleControlPoint));
        }

        /// <summary>
        /// Can this control point have a Follow-Up? This is true if the control point is the beginning of the first segment or the end of the last segment of an open spline
        /// </summary>
        /// <param name="controlPoint"></param>
        /// <returns></returns>
        public bool CanControlPointHaveFollowUp(CurvySplineSegment controlPoint)
        {
            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);
            return controlPoint.GetExtrinsicPropertiesINTERNAL().CanHaveFollowUp;
        }

        /// <summary>
        /// Index of the control point
        /// </summary>
        /// <param name="controlPoint"></param>
        /// <returns></returns>
        public short GetControlPointIndex(CurvySplineSegment controlPoint)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPoints.Contains(controlPoint));
#endif
            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);
            short controlPointIndex = controlPoint.GetExtrinsicPropertiesINTERNAL().ControlPointIndex;
#if CURVY_SANITY_CHECKS_PRIVATE//Judged too expensive in cpu time to be part of the sanity checks available to users
            Assert.IsTrue(controlPoint == ControlPoints[controlPointIndex]);
#endif

            return controlPointIndex;
        }

        /// <summary>
        /// Index of the segment that this control point starts. -1 if control point does not start a segment.
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public short GetSegmentIndex(CurvySplineSegment segment)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPoints.Contains(segment));
#endif
            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);
            short segementIndex = segment.GetExtrinsicPropertiesINTERNAL().SegmentIndex;
#if CURVY_SANITY_CHECKS_PRIVATE//Judged too expensive in cpu time to be part of the sanity checks available to users
            Assert.IsTrue(segementIndex == -1 || segment == mSegments[segementIndex]);
#endif
            return segementIndex;
        }

        /// <summary>
        /// The next control point on the spline. Is null if none. Follow-Up not considered
        /// </summary>
        /// <param name="controlPoint"></param>
        /// <returns></returns>
        [CanBeNull]
        public CurvySplineSegment GetNextControlPoint(CurvySplineSegment controlPoint)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPoints.Contains(controlPoint));
#endif
            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);
            short nextControlPointIndex = controlPoint.GetExtrinsicPropertiesINTERNAL().NextControlPointIndex;
            return nextControlPointIndex == -1 ? null : ControlPoints[nextControlPointIndex];
        }

        /// <summary>
        /// The index of the next control point on the spline. Is -1 if none. Follow-Up not considered
        /// </summary>
        /// <param name="controlPoint"></param>
        /// <returns></returns>
        [CanBeNull]
        public short GetNextControlPointIndex(CurvySplineSegment controlPoint)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPoints.Contains(controlPoint));
#endif
            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);
            return controlPoint.GetExtrinsicPropertiesINTERNAL().NextControlPointIndex;
        }

        /// <summary>
        /// The next control point. Is null if none. Follow-Up is considered
        /// </summary>
        /// <param name="controlPoint"></param>
        /// <returns></returns>
        [CanBeNull]
        public CurvySplineSegment GetNextControlPointUsingFollowUp(CurvySplineSegment controlPoint)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPoints.Contains(controlPoint));
#endif
            return controlPoint.FollowUp != null && LastVisibleControlPoint == controlPoint
                ? GetFollowUpNextControlPoint(controlPoint.FollowUp, controlPoint.FollowUpHeading)
                : GetNextControlPoint(controlPoint);
        }

        /// <summary>
        /// The previous control point on the spline. Is null if none. Follow-Up not considered
        /// </summary>
        /// <param name="controlPoint"></param>
        /// <returns></returns>
        [CanBeNull]
        public CurvySplineSegment GetPreviousControlPoint(CurvySplineSegment controlPoint)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPoints.Contains(controlPoint));
#endif
            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);
            short previousControlPointIndex = controlPoint.GetExtrinsicPropertiesINTERNAL().PreviousControlPointIndex;
            return previousControlPointIndex == -1 ? null : ControlPoints[previousControlPointIndex];
        }

        /// <summary>
        /// The index of the previous control point on the spline. Is -1 if none. Follow-Up not considered
        /// </summary>
        /// <param name="controlPoint"></param>
        /// <returns></returns>
        [CanBeNull]
        public short GetPreviousControlPointIndex(CurvySplineSegment controlPoint)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPoints.Contains(controlPoint));
#endif
            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);
            return controlPoint.GetExtrinsicPropertiesINTERNAL().PreviousControlPointIndex;
        }

        /// <summary>
        /// The previous control point. Is null if none. Follow-Up is considered
        /// </summary>
        /// <param name="controlPoint"></param>
        /// <returns></returns>
        [CanBeNull]
        public CurvySplineSegment GetPreviousControlPointUsingFollowUp(CurvySplineSegment controlPoint)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPoints.Contains(controlPoint));
#endif
            return controlPoint.FollowUp != null && FirstVisibleControlPoint == controlPoint
                ? GetFollowUpNextControlPoint(controlPoint.FollowUp, controlPoint.FollowUpHeading)
                : GetPreviousControlPoint(controlPoint);
        }

        /// <summary>
        /// The next control point on the spline if it starts a segment. Is null if none. Follow-Up not considered
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [CanBeNull]
        public CurvySplineSegment GetNextSegment(CurvySplineSegment segment)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPoints.Contains(segment));
#endif
            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);

            CurvySplineSegment.ControlPointExtrinsicProperties cpExtrinsicProperties = segment.GetExtrinsicPropertiesINTERNAL();
            return cpExtrinsicProperties.NextControlPointIsSegment
                ? ControlPoints[cpExtrinsicProperties.NextControlPointIndex]
                : null;
        }

        /// <summary>
        /// The previous control point on the spline if it starts a segment. Is null if none. Follow-Up not considered. 
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [CanBeNull]
        public CurvySplineSegment GetPreviousSegment(CurvySplineSegment segment)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPoints.Contains(segment));
#endif
            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);
            CurvySplineSegment.ControlPointExtrinsicProperties cpExtrinsicProperties = segment.GetExtrinsicPropertiesINTERNAL();
            return cpExtrinsicProperties.PreviousControlPointIsSegment
                ? ControlPoints[cpExtrinsicProperties.PreviousControlPointIndex]
                : null;
        }

        /// <summary>
        /// Is the control point the start of a segment?
        /// </summary>
        /// <param name="controlPoint"></param>
        /// <returns></returns>
        public bool IsControlPointASegment(CurvySplineSegment controlPoint)
        {
            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);
            return controlPoint.GetExtrinsicPropertiesINTERNAL().IsSegment;
        }

        /// <summary>
        /// Is the control point part of a segment (whether starting it or ending it)
        /// </summary>
        /// <param name="controlPoint"></param>
        /// <returns></returns>
        public bool IsControlPointVisible(CurvySplineSegment controlPoint)
        {
            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);
            return controlPoint.GetExtrinsicPropertiesINTERNAL().IsVisible;
        }

        /// <summary>
        /// The index of the control point being the orientation anchor for the anchor group containing the controlPoint
        /// Is -1 for non visible control points
        /// </summary>
        public short GetControlPointOrientationAnchorIndex(CurvySplineSegment controlPoint)
        {
            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);
            return controlPoint.GetExtrinsicPropertiesINTERNAL().OrientationAnchorIndex;
        }
        #endregion

        /// <summary>
        /// Event-friendly helper that sets a field or property value
        /// </summary>
        /// <param name="fieldAndValue">e.g. "MyValue=42"</param>
        public void SetFromString(string fieldAndValue)
        {
            string[] f = fieldAndValue.Split('=');
            if (f.Length != 2)
                return;

            FieldInfo fi = this.GetType().FieldByName(f[0], true, false);
            if (fi != null)
            {
                try
                {
#if NETFX_CORE
                    if (fi.FieldType.GetTypeInfo().IsEnum)
#else
                    if (fi.FieldType.IsEnum)
#endif
                        fi.SetValue(this, System.Enum.Parse(fi.FieldType, f[1]));
                    else
                        fi.SetValue(this, System.Convert.ChangeType(f[1], fi.FieldType, System.Globalization.CultureInfo.InvariantCulture));
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(this.name + ".SetFromString(): " + e.ToString());
                }
            }
            else
            {
                PropertyInfo pi = this.GetType().PropertyByName(f[0], true, false);
                if (pi != null)
                {
                    try
                    {
#if NETFX_CORE
                        if (pi.PropertyType.GetTypeInfo().IsEnum)
#else
                        if (pi.PropertyType.IsEnum)
#endif

                            pi.SetValue(this, System.Enum.Parse(pi.PropertyType, f[1]), null);
                        else
                            pi.SetValue(this, System.Convert.ChangeType(f[1], pi.PropertyType, System.Globalization.CultureInfo.InvariantCulture), null);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning(this.name + ".SetFromString(): " + e.ToString());
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Used as a parameter for dirtying methods. Instructs if only the spline's orientation cache should be recomputed, or the whole approximations cache
    /// </summary>
    public enum SplineDirtyingType
    {
        /// <summary>
        /// Orientation approximations cache will be dirtied. Positions approximations cache will be ignored.
        /// </summary>
        OrientationOnly,
        /// <summary>
        /// Orientation approximations cache and positions approximations cache will be dirtied.
        /// </summary>
        Everything
    }
}


