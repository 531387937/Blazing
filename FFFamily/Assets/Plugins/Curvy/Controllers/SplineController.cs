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
using System.Collections.ObjectModel;
using System.Linq;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace FluffyUnderware.Curvy.Controllers
{

    /// <summary>
    /// Defines what spline a <see cref="SplineController"/> will use when reaching a <see cref="CurvyConnection"/>.
    /// </summary>
    public enum SplineControllerConnectionBehavior
    {
        /// <summary>
        /// Continue moving on the current spline, ignoring the connection.
        /// </summary>
        CurrentSpline,
        /// <summary>
        /// Move to the spline containing the Follow-Up if any. If none, continue moving on the current spline, ignoring the connection.
        /// </summary>
        FollowUpSpline,
        /// <summary>
        /// Move to the spline of a randomly selected control point from all the connected control points.
        /// </summary>
        RandomSpline,
        /// <summary>
        /// Move to the spline containing the Follow-Up if any. If none, move to the spline of a randomly selected control point from all the connected control points.
        /// </summary>
        FollowUpOtherwiseRandom,
        /// <summary>
        /// Use a custom defined selection logic
        /// </summary>
        Custom
    }

    /// <summary>
    /// Controller working with Splines
    /// </summary>
    [AddComponentMenu("Curvy/Controller/Spline Controller", 5)]
    [HelpURL(CurvySpline.DOCLINK + "splinecontroller")]
    public class SplineController : CurvyController
    {
        public SplineController()
        {
            preAllocatedEventArgs = new CurvySplineMoveEventArgs(this, Spline, null, Single.NaN, false, Single.NaN, MovementDirection.Forward);
        }

        #region ### Serialized Fields ###
        /// <summary>
        /// The spline to use. It is best to set/get the spline through the <see cref="Spline"/> property instead
        /// </summary>
        [Section("General", Sort = 0)]
        [FieldCondition("m_Spline", null, false, ActionAttribute.ActionEnum.ShowError, "Missing source Spline")]
        [SerializeField]
        protected CurvySpline m_Spline;

        [SerializeField]
        [Tooltip("Whether spline's cache data should be used. Set this to true to gain performance if precision is not required.")]
        bool m_UseCache;

        #region Connections handling

        [Section("Connections handling", Sort = 250, HelpURL = CurvySpline.DOCLINK + "curvycontroller_move")]

        [SerializeField, Label("At connection, use", "What spline should the controller use when reaching a Connection")]
        SplineControllerConnectionBehavior connectionBehavior = SplineControllerConnectionBehavior.CurrentSpline;

        #region Random Connection and Follow-Up options

        [SerializeField, Label("Allow direction change", "When true, the controller will modify its direction to best fit the connected spline")]
#if UNITY_EDITOR
        [FieldCondition("connectionBehavior", SplineControllerConnectionBehavior.FollowUpSpline, false, ConditionalAttribute.OperatorEnum.OR, "ShowRandomConnectionOptions", true, false)]
#endif

        private bool allowDirectionChange = true;

        #endregion

        #region Random Connection options

        [SerializeField, Label("Reject current spline", "Whether the current spline should be excluded from the randomly selected splines")]
        [FieldCondition("ShowRandomConnectionOptions", true)]
        private bool rejectCurrentSpline = true;

        [SerializeField, Label("Reject divergent splines", "Whether splines that diverge from the current spline with more than a specific angle should be excluded from the randomly selected splines")]
        [FieldCondition("ShowRandomConnectionOptions", true)]
        private bool rejectTooDivergentSplines = false;

        [SerializeField, Label("Max allowed angle", "Maximum allowed divergence angle in degrees")]
#if UNITY_EDITOR
        [FieldCondition("ShowRandomConnectionOptions", true, false, ConditionalAttribute.OperatorEnum.AND, "rejectTooDivergentSplines", true, false)]
#endif
        [Range(0, 180)]
        private float maxAllowedDivergenceAngle = 90;

        #endregion

        #region Custom options

        [SerializeField, Label("Custom Selector", "A custom logic to select which connected spline to follow. Select a Script inheriting from SplineControllerConnectionBehavior")]
        [FieldCondition("connectionBehavior", SplineControllerConnectionBehavior.Custom, false, ActionAttribute.ActionEnum.Show)]
        [FieldCondition("connectionCustomSelector", null, false, ActionAttribute.ActionEnum.ShowWarning, "Missing custom selector")]
        private ConnectedControlPointsSelector connectionCustomSelector;

        #endregion

        #endregion


        [Section("Events", false, false, 1000, HelpURL = CurvySpline.DOCLINK + "splinecontroller_events")]
        [SerializeField]
        protected CurvySplineMoveEvent m_OnControlPointReached;
        [SerializeField]
        protected CurvySplineMoveEvent m_OnEndReached;
        [SerializeField]
        protected CurvySplineMoveEvent m_OnSwitch;


        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// Gets or sets the spline to use
        /// </summary>
        public virtual CurvySpline Spline
        {
            get { return m_Spline; }
            set
            { m_Spline = value; }
        }

        /// <summary>
        /// Gets or sets whether spline's cache data should be used
        /// </summary>
        public bool UseCache
        {
            get
            {
                return m_UseCache;
            }
            set
            {
                if (m_UseCache != value)
                    m_UseCache = value;
            }
        }


        #region Connections handling

        /// <summary>
        /// Connections handling: What spline should the controller use when reaching a Connection
        /// </summary>
        public SplineControllerConnectionBehavior ConnectionBehavior
        {
            get { return connectionBehavior; }
            set { connectionBehavior = value; }
        }

        /// <summary>
        /// Connections handling: A custom logic to select which connected spline to follow. Select a Script inheriting from SplineControllerConnectionBehavior. Is used when <see cref="ConnectionBehavior"/> is equal to <see cref="SplineControllerConnectionBehavior.Custom"/>
        /// </summary>
        public ConnectedControlPointsSelector ConnectionCustomSelector
        {
            get { return connectionCustomSelector; }
            set { connectionCustomSelector = value; }
        }

        /// <summary>
        /// Connections handling: When true, the controller will modify its direction to best fit the connected spline. Is used when <see cref="ConnectionBehavior"/> is equal to <see cref="SplineControllerConnectionBehavior.FollowUpSpline"/>,  <see cref="SplineControllerConnectionBehavior.RandomSpline"/>, or <see cref="SplineControllerConnectionBehavior.FollowUpOtherwiseRandom"/>
        /// </summary>
        public bool AllowDirectionChange
        {
            get { return allowDirectionChange; }
            set { allowDirectionChange = value; }
        }

        /// <summary>
        /// Connections handling: Whether the current spline should be excluded from the randomly selected splines. Is used when <see cref="ConnectionBehavior"/> is equal to <see cref="SplineControllerConnectionBehavior.RandomSpline"/>, or <see cref="SplineControllerConnectionBehavior.FollowUpOtherwiseRandom"/>
        /// </summary>
        public bool RejectCurrentSpline
        {
            get { return rejectCurrentSpline; }
            set { rejectCurrentSpline = value; }
        }

        /// <summary>
        /// Connections handling: Whether splines that diverge from the current spline with more than <see cref="MaxAllowedDivergenceAngle"/> should be excluded from the randomly selected splines. Is used when <see cref="ConnectionBehavior"/> is equal to <see cref="SplineControllerConnectionBehavior.RandomSpline"/>, or <see cref="SplineControllerConnectionBehavior.FollowUpOtherwiseRandom"/>
        /// </summary>
        public bool RejectTooDivergentSplines
        {
            get { return rejectTooDivergentSplines; }
            set { rejectTooDivergentSplines = value; }
        }

        /// <summary>
        /// Connections handling: Maximum allowed divergence angle in degrees. Considered when <see cref="MaxAllowedDivergenceAngle"/> is true. Is used when <see cref="ConnectionBehavior"/> is equal to <see cref="SplineControllerConnectionBehavior.RandomSpline"/>, or <see cref="SplineControllerConnectionBehavior.FollowUpOtherwiseRandom"/>
        /// </summary>
        public float MaxAllowedDivergenceAngle
        {
            get { return maxAllowedDivergenceAngle; }
            set { maxAllowedDivergenceAngle = value; }
        }

        #endregion


        /// <summary>
        /// Event raised when moving over a Control Point
        /// </summary>
        public CurvySplineMoveEvent OnControlPointReached
        {
            get { return m_OnControlPointReached; }
            set { m_OnControlPointReached = value; }
        }

        /// <summary>
        /// Event raised when reaching the extends of the source spline
        /// </summary>
        public CurvySplineMoveEvent OnEndReached
        {
            get { return m_OnEndReached; }
            set { m_OnEndReached = value; }
        }

        /// <summary>
        /// Event raised while switching splines
        /// </summary>
        public CurvySplineMoveEvent OnSwitch
        {
            get { return m_OnSwitch; }
            set { m_OnSwitch = value; }
        }


        /// <summary>
        /// Gets whether the Controller is switching splines
        /// </summary>
        public bool IsSwitching { get; private set; }

        /// <summary>
        /// The ratio (value between 0 and 1) expressing the progress of the current spline switch. 0 means the switch just started, 1 means the switch ended.
        /// Its value is 0 if no spline switching is in progress. Spline switching is done by calling <see cref="SwitchTo"/>
        /// </summary>
        public float SwitchProgress { get { return IsSwitching ? Mathf.Clamp01((Time.time - SwitchStartTime) / SwitchDuration) : 0; } }

        /// <summary>
        /// Gets the source's length
        /// </summary>
        public override float Length
        {
            get
            {
#if CURVY_SANITY_CHECKS
                Assert.IsTrue(IsReady, ControllerNotReadyMessage);
#endif
                return (Spline) ? Spline.Length : 0;
            }
        }

        #endregion

        #region ### Private & Protected Fields & Properties ###

        private CurvySpline prePlaySpline;
        private readonly CurvySplineMoveEventArgs preAllocatedEventArgs;

        #region Switch

        /// <summary>
        /// The time at which the current spline switching started.
        /// Its value is invalid if no spline switching is in progress. Spline switching is done by calling <see cref="SwitchTo"/>
        /// </summary>
        protected float SwitchStartTime;
        /// <summary>
        /// The duration of the the current spline switching.
        /// Its value is invalid if no spline switching is in progress. Spline switching is done by calling <see cref="SwitchTo"/>
        /// </summary>
        protected float SwitchDuration;
        /// <summary>
        /// The spline to which the controller is switching.
        /// Its value is invalid if no spline switching is in progress. Spline switching is done by calling <see cref="SwitchTo"/>
        /// </summary>
        protected CurvySpline SwitchTarget;
        /// <summary>
        /// The controller's current TF on the <see cref="SwitchTarget"/>.
        /// Its value is invalid if no spline switching is in progress. Spline switching is done by calling <see cref="SwitchTo"/>
        /// </summary>
        protected float TfOnSwitchTarget;
        /// <summary>
        /// The controller's current Direction on the <see cref="SwitchTarget"/>.
        /// Its value is invalid if no spline switching is in progress. Spline switching is done by calling <see cref="SwitchTo"/>
        /// </summary>
        protected MovementDirection DirectionOnSwitchTarget;

        #endregion

        #endregion

        #region ## Unity Callbacks ###
        /*! \cond UNITY */

        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Start a spline switch. Should be called only on non stopped controllers.
        /// </summary>
        /// <remarks>While switching is not finished, movement on destination spline will not fire events nore consider connections</remarks>
        /// <param name="destinationSpline">the target spline to switch to</param>
        /// <param name="destinationTf">the target TF</param>
        /// <param name="duration">duration of the switch phase</param>
        public virtual void SwitchTo(CurvySpline destinationSpline, float destinationTf, float duration)
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(isInitialized, ControllerNotReadyMessage);
#endif
            if (PlayState == CurvyControllerState.Stopped)
            {
                DTLog.LogError("[Curvy] Contoller can not switch when stopped. The switch call will be ignored");
            }
            else
            {
                SwitchStartTime = Time.time;
                SwitchDuration = duration;
                SwitchTarget = destinationSpline;
                TfOnSwitchTarget = destinationTf;
                DirectionOnSwitchTarget = MovementDirection;
                IsSwitching = true;
            }
        }

        /// <summary>
        /// If is switching splines, instantly finishes the current switch.
        /// </summary>
        public void FinishCurrentSwitch()
        {
            if (IsSwitching)
            {
                IsSwitching = false;
                Spline = SwitchTarget;
                RelativePosition = TfOnSwitchTarget;
            }
        }

        /// <summary>
        /// If is switching splines, cancels the current switch.
        /// </summary>
        public void CancelCurrentSwitch()
        {
            if (IsSwitching)
                IsSwitching = false;
        }

        /// <summary>
        /// Get the direction change, in degrees, of controller caused by the crossing of a connection.
        /// </summary>
        /// <param name="before">The control point the controller is on before crossing the connection</param>
        /// <param name="movementMode">The movement mode the controller has before crossing the connection</param>
        /// <param name="after">The control point the controller is on after crossing the connection</param>
        /// <param name="allowMovementModeChange">If true, the controller will change movemen mode to best fit the after control point. <see cref="AllowDirectionChange"/></param>
        /// <returns>A positif angle in degrees</returns>
        public static float GetAngleBetweenConnectedSplines(CurvySplineSegment before, MovementDirection movementMode, CurvySplineSegment after, bool allowMovementModeChange)
        {
            Vector3 currentTangent = before.GetTangentFast(0) * movementMode.ToInt();
            Vector3 newTangent = after.GetTangentFast(0) * GetPostConnectionDirection(after, movementMode, allowMovementModeChange).ToInt();
            return Vector3.Angle(currentTangent, newTangent);
        }

        #endregion

        #region ### Protected Methods ###

        override public bool IsReady
        {
            get
            {
                return Spline != null && Spline.IsInitialized;
            }
        }

        override protected void SavePrePlayState()
        {
            prePlaySpline = Spline;
            base.SavePrePlayState();
        }

        override protected void RestorePrePlayState()
        {
            Spline = prePlaySpline;
            base.RestorePrePlayState();
        }

        protected override float RelativeToAbsolute(float relativeDistance)
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(IsReady, ControllerNotReadyMessage);
            Assert.IsTrue(CurvyUtility.Approximately(relativeDistance, GetClampedPosition(relativeDistance, CurvyPositionMode.Relative, Clamping, Length)));
#endif
            return Spline.TFToDistance(relativeDistance, Clamping);
        }


        protected override float AbsoluteToRelative(float worldUnitDistance)
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(IsReady, ControllerNotReadyMessage);
            Assert.IsTrue(CurvyUtility.Approximately(worldUnitDistance, GetClampedPosition(worldUnitDistance, CurvyPositionMode.WorldUnits, Clamping, Length)));
#endif
            return Spline.DistanceToTF(worldUnitDistance, Clamping);
        }

        protected override Vector3 GetInterpolatedSourcePosition(float tf)
        {
            Vector3 p = (UseCache) ? Spline.InterpolateFast(tf) : Spline.Interpolate(tf);

            return Spline.transform.TransformPoint(p);
        }


        protected override void GetInterpolatedSourcePosition(float tf, out Vector3 interpolatedPosition, out Vector3 tangent, out Vector3 up)
        {
            CurvySpline spline = Spline;
            Transform splineTransform = spline.transform;

            if (UseCache)
                spline.InterpolateAndGetTangentFast(tf, out interpolatedPosition, out tangent);
            else
                spline.InterpolateAndGetTangent(tf, out interpolatedPosition, out tangent);

            up = spline.GetOrientationUpFast(tf);
            interpolatedPosition = splineTransform.TransformPoint(interpolatedPosition);
            tangent = splineTransform.TransformDirection(tangent);
            up = splineTransform.TransformDirection(up);
        }

        protected override Vector3 GetTangent(float tf)
        {
            Vector3 t = (UseCache) ? Spline.GetTangentFast(tf) : Spline.GetTangent(tf);
            return Spline.transform.TransformDirection(t);
        }

        protected override Vector3 GetOrientation(float tf)
        {
            return Spline.transform.TransformDirection(Spline.GetOrientationUpFast(tf));
        }

        protected override void Advance(float speed, float deltaTime)
        {
            float distance = speed * deltaTime;

#if CURVY_SANITY_CHECKS
            Assert.IsTrue(distance > 0);
#endif

            if (Spline.Count != 0)
                EventAwareMove(distance);


            if (IsSwitching && SwitchTarget.Count > 0)
            {
                SimulateAdvanceOnSpline(ref TfOnSwitchTarget, ref DirectionOnSwitchTarget, SwitchTarget, speed * deltaTime);

                preAllocatedEventArgs.Set_INTERNAL(this, SwitchTarget, null, TfOnSwitchTarget, SwitchProgress, DirectionOnSwitchTarget, false);
                OnSwitch.Invoke(preAllocatedEventArgs);
                if (preAllocatedEventArgs.Cancel)
                    CancelCurrentSwitch();
            }
        }

        override protected void SimulateAdvance(ref float tf, ref MovementDirection curyDirection, float speed, float deltaTime)
        {
            SimulateAdvanceOnSpline(ref tf, ref curyDirection, Spline, speed * deltaTime);
        }

        private void SimulateAdvanceOnSpline(ref float tf, ref MovementDirection curyDirection, CurvySpline spline, float distance)
        {
            if (spline.Count > 0)
            {
                int directionInt = curyDirection.ToInt();
                switch (MoveMode)
                {
                    case MoveModeEnum.AbsolutePrecise:
                        tf = spline.DistanceToTF(spline.ClampDistance(spline.TFToDistance(tf) + distance * directionInt, ref directionInt, Clamping));
                        break;
                    case MoveModeEnum.Relative:
                        tf = CurvyUtility.ClampTF(tf + distance * directionInt, ref directionInt, Clamping);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                curyDirection = MovementDirectionMethods.FromInt(directionInt);
            }
        }

        override protected void InitializedApplyDeltaTime(float deltaTime)
        {
            if (Spline.Dirty)
                Spline.Refresh();

            base.InitializedApplyDeltaTime(deltaTime);

            if (IsSwitching && SwitchProgress >= 1)
                FinishCurrentSwitch();
        }

        override protected void ComputeTargetPositionAndRotation(out Vector3 targetPosition, out Vector3 targetUp, out Vector3 targetForward)
        {
            Vector3 switchlessPosition;
            Vector3 switchlessUp;
            Vector3 switchlessForward;
            base.ComputeTargetPositionAndRotation(out switchlessPosition, out switchlessUp, out switchlessForward);
            Quaternion switchlessRotation = Quaternion.LookRotation(switchlessForward, switchlessUp);

            if (IsSwitching)
            {
                CurvySpline preSwitchSpline = Spline;
                float preSwitchSplineTf = RelativePosition;

                m_Spline = SwitchTarget;
                RelativePosition = TfOnSwitchTarget;

                Vector3 positionOnSwitchToSpline;
                Vector3 upOnSwitchToSpline;
                Vector3 forwardOnSwitchToSpline;
                base.ComputeTargetPositionAndRotation(out positionOnSwitchToSpline, out upOnSwitchToSpline, out forwardOnSwitchToSpline);
                Quaternion rotationOnSwitchToSpline = Quaternion.LookRotation(forwardOnSwitchToSpline, upOnSwitchToSpline);

                m_Spline = preSwitchSpline;
                RelativePosition = preSwitchSplineTf;

                targetPosition = Vector3.LerpUnclamped(switchlessPosition, positionOnSwitchToSpline, SwitchProgress);
                Quaternion interpolatedRotation = Quaternion.LerpUnclamped(switchlessRotation, rotationOnSwitchToSpline, SwitchProgress);
                targetUp = interpolatedRotation * Vector3.up;
                targetForward = interpolatedRotation * Vector3.forward;
            }
            else
            {
                targetPosition = switchlessPosition;
                targetUp = switchlessUp;
                targetForward = switchlessForward;
            }
        }


        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */

        /// <summary>
        /// This method gets the controller position, but handles the looping differently than usual (it does not change a realtive position of 1 to 0), which avoids hardly solvable ambiguities in the movement logic.
        /// </summary>
        /// <remarks>This is to make controller logic simpler, since it does not need anymore to guess if a position of 0 meant controller on the end of the spline and needed looping, or meant that the controller is on the start of the spline.</remarks>
        static float MovementCompatibleGetPosition(SplineController controller, CurvyPositionMode positionMode, out CurvySplineSegment controlPoint, out bool isOnControlPoint)
        {

            float resultPosition;
            CurvySpline spline = controller.Spline;
            float rawPosition = controller.m_Position;

            float specialClampedPosition;
            switch (controller.PositionMode)
            {
                case CurvyPositionMode.Relative:
                    specialClampedPosition = (controller.Clamping == CurvyClamping.Loop && CurvyUtility.Approximately(rawPosition, 1))
                        ? 1 //This is to make controller logic simpler, since it does not need anymore to guess if a position of 0 meant controller on the end of the spline and needed looping, or meant that the controller is on the start of the spline.
                        : CurvyUtility.ClampTF(rawPosition, controller.Clamping);
                    break;
                case CurvyPositionMode.WorldUnits:
                    float length = controller.Length;
                    specialClampedPosition = (controller.Clamping == CurvyClamping.Loop && CurvyUtility.Approximately(rawPosition, length))
                        ? length //This is to make controller logic simpler, since it does not need anymore to guess if a position of 0 meant controller on the end of the spline and needed looping, or meant that the controller is on the start of the spline.
                        : CurvyUtility.ClampDistance(rawPosition, controller.Clamping, length);
                    break;
                default:
                    throw new NotSupportedException();
            }

            bool isOnSegmentLastCp;
            bool isOnSegmentFirstCp;
            float unconvertedLocalPosition;
            switch (controller.PositionMode)
            {
                case CurvyPositionMode.Relative:
                    controlPoint = spline.TFToSegment(specialClampedPosition, out unconvertedLocalPosition, CurvyClamping.Clamp); //CurvyClamping.Clamp to cancel looping handling
                    isOnSegmentFirstCp = CurvyUtility.Approximately(unconvertedLocalPosition, 0);
                    isOnSegmentLastCp = CurvyUtility.Approximately(unconvertedLocalPosition, 1);
                    break;
                case CurvyPositionMode.WorldUnits:
                    controlPoint = spline.DistanceToSegment(specialClampedPosition, out unconvertedLocalPosition, CurvyClamping.Clamp); //CurvyClamping.Clamp to cancel looping handling
                    isOnSegmentFirstCp = CurvyUtility.Approximately(unconvertedLocalPosition, 0);
                    isOnSegmentLastCp = CurvyUtility.Approximately(unconvertedLocalPosition, controlPoint.Length);
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (positionMode == controller.PositionMode)
                resultPosition = specialClampedPosition;
            else
            {
                switch (positionMode)
                {
                    case CurvyPositionMode.Relative:
                        resultPosition = spline.SegmentToTF(controlPoint, controlPoint.DistanceToLocalF(unconvertedLocalPosition));
                        break;
                    case CurvyPositionMode.WorldUnits:
                        resultPosition = controlPoint.Distance + controlPoint.LocalFToDistance(unconvertedLocalPosition);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (isOnSegmentLastCp) //Case of last cp of an open spline, but also when position is a little bit behind the actual position, due to float inaccuracies in the conversion methods
                controlPoint = spline.GetNextControlPoint(controlPoint);

            isOnControlPoint = isOnSegmentFirstCp || isOnSegmentLastCp;

            return resultPosition;
        }

        /// <summary>
        /// This method sets the controller position, but handles the looping differently than usual (it does not change a realtive position of 1 to 0), which avoids hardly solvable ambiguities in the movement logic.
        /// </summary>
        /// <remarks>This is to make controller logic simpler, since it does not need anymore to guess if a position of 0 meant controller on the end of the spline and needed looping, or meant that the controller is on the start of the spline.</remarks>
        static void MovementCompatibleSetPosition(SplineController controller, CurvyPositionMode positionMode, float position)
        {
            CurvyPositionMode controllerPositionMode = controller.PositionMode;
            CurvyClamping controllerClampingMode = controller.Clamping;

            float clampedPosition;
            {
                switch (positionMode)
                {
                    case CurvyPositionMode.Relative:
                        clampedPosition = (controllerClampingMode == CurvyClamping.Loop && CurvyUtility.Approximately(position, 1))
                            ? 1
                            : CurvyUtility.ClampTF(position, controllerClampingMode);
                        break;
                    case CurvyPositionMode.WorldUnits:
                        float length = controller.Length;
                        clampedPosition = (controllerClampingMode == CurvyClamping.Loop && CurvyUtility.Approximately(position, length))
                            ? length
                            : CurvyUtility.ClampDistance(position, controllerClampingMode, length);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            if (positionMode == controllerPositionMode)
                controller.m_Position = clampedPosition;
            else
                switch (positionMode)
                {
                    case CurvyPositionMode.Relative:
                        controller.m_Position = controller.Spline.TFToDistance(clampedPosition, controller.Clamping);
                        break;
                    case CurvyPositionMode.WorldUnits:
                        controller.m_Position = controller.Spline.DistanceToTF(clampedPosition, controller.Clamping);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }

        /// <summary>
        /// Updates position and direction while trigerring events when reaching a control point
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="positionMode">The position mode used in the computations. Could be different than SplineController.PositionMode</param>
        private void EventAwareMove(float distance)
        {
#if CURVY_SANITY_CHECKS
            MoveModeEnum moveModeAtMethodStart = MoveMode;
            Assert.IsTrue(distance > 0);
#endif

            CurvyPositionMode movementRelatedPositionMode;
            switch (MoveMode)
            {
                case MoveModeEnum.AbsolutePrecise:
                    movementRelatedPositionMode = CurvyPositionMode.WorldUnits;
                    break;
                case MoveModeEnum.Relative:
                    movementRelatedPositionMode = CurvyPositionMode.Relative;
                    break;
                default:
                    throw new NotSupportedException();
            }

            float currentDelta = distance;
            bool cancelMovement = false;

            //Handle when controller starts at special position
            if ((MovementDirection == MovementDirection.Backward && CurvyUtility.Approximately(RelativePosition, 0)) ||
                (MovementDirection == MovementDirection.Forward && CurvyUtility.Approximately(RelativePosition, 1)))
                switch (Clamping)
                {
                    case CurvyClamping.Clamp:
                        currentDelta = 0;
                        break;
                    case CurvyClamping.PingPong:
                        MovementDirection = MovementDirection.GetOpposite();
                        break;
                }

            int infiniteLoopSafety = 50;
            while (!cancelMovement && currentDelta > 0 && infiniteLoopSafety-- > 0)//OPTIM couldn't we do this without looping?
            {

#if CURVY_SANITY_CHECKS
                Assert.IsTrue(Spline.Count > 0);
                Assert.IsTrue(moveModeAtMethodStart == MoveMode);// MoveMode is not allowed to be modified while moving a Spline Controller;
#endif
                float currentPosition;
                CurvySplineSegment candidateControlPoint;
                CurvySplineSegment segmentAtIterationStart;
                bool isOnControlPointAtIterationStart;
                {
                    segmentAtIterationStart = GetCurrentControlPoint(out isOnControlPointAtIterationStart, out currentPosition, movementRelatedPositionMode);

                    if (MovementDirection == MovementDirection.Forward)
                        candidateControlPoint = Spline.GetNextControlPoint(segmentAtIterationStart);
                    else
                        candidateControlPoint = isOnControlPointAtIterationStart
                            ? Spline.GetPreviousControlPoint(segmentAtIterationStart)
                            : segmentAtIterationStart;
                }


                if (candidateControlPoint != null && Spline.IsControlPointVisible(candidateControlPoint))
                {
                    float distanceToCandidate = Mathf.Abs(GetControlPointPosition(candidateControlPoint, movementRelatedPositionMode, MovementDirection) - currentPosition);

                    if (distanceToCandidate > currentDelta)//If no more control point to reach, move the controller and exit
                    {
                        MovementCompatibleSetPosition(this, movementRelatedPositionMode, currentPosition + currentDelta * MovementDirection.ToInt());
                        break;
                    }

                    currentDelta -= distanceToCandidate;
                    //Move to next control point
                    HandleReachingNewControlPoint(candidateControlPoint, movementRelatedPositionMode, currentDelta, ref cancelMovement);
                }

                //handle connection
                {
                    bool isOnControlPoint;
                    float unusedOutParam;
                    CurvySplineSegment currentControlPoint = GetCurrentControlPoint(out isOnControlPoint, out unusedOutParam, movementRelatedPositionMode);
                    if (isOnControlPoint && currentControlPoint.Connection && currentControlPoint.Connection.ControlPointsList.Count > 1)
                    {
                        MovementDirection newDirection;
                        CurvySplineSegment postConnectionHandlingControlPoint;
                        switch (ConnectionBehavior)
                        {
                            case SplineControllerConnectionBehavior.CurrentSpline:
                                postConnectionHandlingControlPoint = currentControlPoint;
                                newDirection = MovementDirection;
                                break;
                            case SplineControllerConnectionBehavior.FollowUpSpline:
                                postConnectionHandlingControlPoint = HandleFolloUpConnectionBahavior(currentControlPoint, MovementDirection, out newDirection);
                                break;
                            case SplineControllerConnectionBehavior.FollowUpOtherwiseRandom:
                                postConnectionHandlingControlPoint = currentControlPoint.FollowUp
                                    ? HandleFolloUpConnectionBahavior(currentControlPoint, MovementDirection, out newDirection)
                                    : HandleRandomConnectionBehavior(currentControlPoint, MovementDirection, out newDirection, currentControlPoint.Connection.ControlPointsList);
                                break;
                            case SplineControllerConnectionBehavior.RandomSpline:
                                postConnectionHandlingControlPoint = HandleRandomConnectionBehavior(currentControlPoint, MovementDirection, out newDirection, currentControlPoint.Connection.ControlPointsList);
                                break;
                            case SplineControllerConnectionBehavior.Custom:
                                if (ConnectionCustomSelector == null)
                                {
                                    DTLog.LogError("[Curvy] You need to set a non null ConnectionCustomSelector when using SplineControllerConnectionBehavior.Custom");
                                    postConnectionHandlingControlPoint = currentControlPoint;
                                }
                                else
                                    postConnectionHandlingControlPoint = ConnectionCustomSelector.SelectConnectedControlPoint(this, currentControlPoint.Connection, currentControlPoint);
                                newDirection = MovementDirection;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        if (postConnectionHandlingControlPoint != currentControlPoint)
                        {
                            MovementDirection = newDirection;
                            HandleReachingNewControlPoint(postConnectionHandlingControlPoint, movementRelatedPositionMode, currentDelta, ref cancelMovement);
                        }
                    }
                }

                //handle clamping
                {
                    bool isOnControlPoint;
                    float unusedOutParam;
                    CurvySplineSegment currentControlPoint = GetCurrentControlPoint(out isOnControlPoint, out unusedOutParam, movementRelatedPositionMode);
                    if (isOnControlPoint)
                    {
                        switch (Clamping)
                        {
                            case CurvyClamping.Loop:
                                if (Spline.Closed == false)
                                {
                                    if (MovementDirection == MovementDirection.Backward && currentControlPoint == Spline.FirstVisibleControlPoint)
                                        HandleReachingNewControlPoint(Spline.LastVisibleControlPoint, movementRelatedPositionMode, currentDelta, ref cancelMovement);
                                    else if (MovementDirection == MovementDirection.Forward && currentControlPoint == Spline.LastVisibleControlPoint)
                                        HandleReachingNewControlPoint(Spline.FirstVisibleControlPoint, movementRelatedPositionMode, currentDelta, ref cancelMovement);
                                }
                                break;
                            case CurvyClamping.Clamp:
                                if ((MovementDirection == MovementDirection.Backward && currentControlPoint == Spline.FirstVisibleControlPoint) ||
                                    (MovementDirection == MovementDirection.Forward && currentControlPoint == Spline.LastVisibleControlPoint))
                                    currentDelta = 0;
                                break;
                            case CurvyClamping.PingPong:
                                if ((MovementDirection == MovementDirection.Backward && currentControlPoint == Spline.FirstVisibleControlPoint) ||
                                    (MovementDirection == MovementDirection.Forward && currentControlPoint == Spline.LastVisibleControlPoint))
                                    MovementDirection = MovementDirection.GetOpposite();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                //Handling stucked controller
                {
                    bool isOnControlPointAtIterationEnd;
                    var segmentAtIterationEnd = GetCurrentControlPoint(out isOnControlPointAtIterationEnd, out currentPosition, movementRelatedPositionMode);
                    if (segmentAtIterationStart == segmentAtIterationEnd && isOnControlPointAtIterationStart == isOnControlPointAtIterationEnd)
                    {
                        CurvySplineSegment zeroLengthSegment = null;
                        for (int i = 0; i < Spline.Count; i++)
                        {
                            if (Spline[i].Length == 0)
                            {
                                zeroLengthSegment = Spline[i];
                                break;
                            }
                        }

                        if (zeroLengthSegment != null)
                            DTLog.LogError(String.Format("[Curvy] Spline Controller '{0}' is stuck at control point '{1}'. This is probably caused by the presence of a segment with a length of 0. Please remove control point '{2}' to proceed", name, segmentAtIterationEnd, zeroLengthSegment));
                        else
                            DTLog.LogError(String.Format("[Curvy] Spline Controller '{0}' is stuck at control point '{1}'. Please raise a bug report", name, segmentAtIterationEnd));

                        break;
                    }
                }

            }

            if (infiniteLoopSafety <= 0)
                DTLog.LogError(String.Format("[Curvy] Unexpected behavior in Spline Controller '{0}'. Please raise a Bug Report.", name));

        }

        private CurvySplineSegment GetCurrentControlPoint(out bool isOnControlPoint, out float position, CurvyPositionMode positionMode)
        {
            CurvySplineSegment currentControlPoint;
            position = MovementCompatibleGetPosition(this, positionMode, out currentControlPoint, out isOnControlPoint);
            return currentControlPoint;
        }

        /// <summary>
        /// Do operations necessary when controller reaches a new control point: setting the controller position, update its spline if necessary, and send events if necessary
        /// </summary>
        private void HandleReachingNewControlPoint(CurvySplineSegment newControlPoint, CurvyPositionMode positionMode, float currentDelta, ref bool cancelMovement)
        {
            Spline = newControlPoint.Spline;
            float newCpPosition = GetControlPointPosition(newControlPoint, positionMode, MovementDirection);
            MovementCompatibleSetPosition(this, positionMode, newCpPosition);

            bool eventIsInWorldUnit;
            switch (positionMode)
            {
                case CurvyPositionMode.Relative:
                    eventIsInWorldUnit = false;
                    break;
                case CurvyPositionMode.WorldUnits:
                    eventIsInWorldUnit = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            preAllocatedEventArgs.Set_INTERNAL(this, Spline, newControlPoint, newCpPosition, currentDelta, MovementDirection, eventIsInWorldUnit);
            OnControlPointReached.Invoke(preAllocatedEventArgs);

            if (preAllocatedEventArgs.Spline.FirstVisibleControlPoint == preAllocatedEventArgs.ControlPoint ||
                preAllocatedEventArgs.Spline.LastVisibleControlPoint == preAllocatedEventArgs.ControlPoint)
                OnEndReached.Invoke(preAllocatedEventArgs);

            cancelMovement |= preAllocatedEventArgs.Cancel;

        }

        /// <summary>
        /// Get the correct control point and direction from applying the Random connection handling logic
        /// </summary>
        private CurvySplineSegment HandleRandomConnectionBehavior(CurvySplineSegment currentControlPoint, MovementDirection currentDirection, out MovementDirection newDirection, ReadOnlyCollection<CurvySplineSegment> connectedControlPoints)
        {
            //OPTIM avoid allocation
            List<CurvySplineSegment> validConnectedControlPoints = new List<CurvySplineSegment>(connectedControlPoints.Count);

            for (int index = 0; index < connectedControlPoints.Count; index++)
            {
                CurvySplineSegment controlPoint = connectedControlPoints[index];
                if (RejectCurrentSpline && controlPoint == currentControlPoint)
                    continue;

                if (RejectTooDivergentSplines)
                {
                    if (GetAngleBetweenConnectedSplines(currentControlPoint, currentDirection, controlPoint, AllowDirectionChange) > MaxAllowedDivergenceAngle)
                        continue;
                }

                validConnectedControlPoints.Add(controlPoint);
            }

            CurvySplineSegment newControlPoint = validConnectedControlPoints.Count == 0 ?
                currentControlPoint :
                validConnectedControlPoints[Random.Range(0, validConnectedControlPoints.Count)];

            newDirection = GetPostConnectionDirection(newControlPoint, currentDirection, AllowDirectionChange);

            return newControlPoint;
        }

        /// <summary>
        /// Get the direction the controller should have if moving through a specific connected Control Point
        /// </summary>
        private static MovementDirection GetPostConnectionDirection(CurvySplineSegment connectedControlPoint, MovementDirection currentDirection, bool directionChangeAllowed)
        {
            return directionChangeAllowed && connectedControlPoint.Spline.Closed == false
                ? HeadingToDirection(ConnectionHeadingEnum.Auto, connectedControlPoint, currentDirection)
                : currentDirection;
        }

        /// <summary>
        /// Get the correct control point and direction from applying the FollowUp connection handling logic
        /// </summary>
        private CurvySplineSegment HandleFolloUpConnectionBahavior(CurvySplineSegment currentControlPoint, MovementDirection currentDirection, out MovementDirection newDirection)
        {
            CurvySplineSegment newControlPoint = currentControlPoint.FollowUp
                ? currentControlPoint.FollowUp
                : currentControlPoint;

            newDirection = AllowDirectionChange && currentControlPoint.FollowUp
                ? HeadingToDirection(currentControlPoint.FollowUpHeading, currentControlPoint.FollowUp, currentDirection)
                : currentDirection;

            return newControlPoint;
        }

        /// <summary>
        /// Translates a heading value to a controller direction, based on the current control point situation
        /// </summary>
        static private MovementDirection HeadingToDirection(ConnectionHeadingEnum heading, CurvySplineSegment controlPoint, MovementDirection currentDirection)
        {
            MovementDirection newDirection;
            ConnectionHeadingEnum resolveHeading = heading.ResolveAuto(controlPoint);

            switch (resolveHeading)
            {
                case ConnectionHeadingEnum.Minus:
                    newDirection = MovementDirection.Backward;
                    break;
                case ConnectionHeadingEnum.Sharp:
                    newDirection = currentDirection;
                    break;
                case ConnectionHeadingEnum.Plus:
                    newDirection = MovementDirection.Forward;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return newDirection;
        }


        /// <summary>
        /// Get the controller position corresponding to a specific control point
        /// </summary>
        static float GetControlPointPosition(CurvySplineSegment controlPoint, CurvyPositionMode positionMode, MovementDirection movementDirection)
        {
            CurvySpline spline = controlPoint.Spline;

            float rawPosition;
            switch (positionMode)
            {
                case CurvyPositionMode.Relative:
                    rawPosition = spline.SegmentToTF(controlPoint);
                    break;
                case CurvyPositionMode.WorldUnits:
                    rawPosition = controlPoint.Distance;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            float upperLimit;
            switch (positionMode)
            {
                case CurvyPositionMode.Relative:
                    upperLimit = 1;
                    break;
                case CurvyPositionMode.WorldUnits:
                    upperLimit = spline.Length;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return movementDirection == MovementDirection.Forward && CurvyUtility.Approximately(rawPosition, 0) && spline.Closed
                ? upperLimit
                : rawPosition;
        }

        /// <summary>
        /// Used as a field condition
        /// </summary>
        private bool ShowRandomConnectionOptions { get { return ConnectionBehavior == SplineControllerConnectionBehavior.FollowUpOtherwiseRandom || ConnectionBehavior == SplineControllerConnectionBehavior.RandomSpline; } }

        /*! \endcond */

        #endregion

    }
}
