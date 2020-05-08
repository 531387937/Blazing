// =====================================================================
// Copyright 2013-2018 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Base class for Metadata classes that support interpolation.
    /// </summary>
    /// <typeparam name="T">The Type of the Metadata's value</typeparam>
    [ExecuteInEditMode]
#pragma warning disable 618
    public abstract class CurvyInterpolatableMetadataBase<T> : CurvyInterpolatableMetadataBase, ICurvyInterpolatableMetadata<T>
#pragma warning restore 618
    {
        /// <summary>
        /// The value stored within this Metadata instance
        /// </summary>
        public abstract T MetaDataValue { get; }

        /// <summary>
        /// Interpolates between the current Metadata's value and the one from the next Control Point's Metadata.
        /// </summary>
        /// <param name="nextMetadata">The Metadata from the Control Point next to the current one</param>
        /// <param name="interpolationTime">The local F value on the segment defined by the current Control Point and the next one</param>
        /// <returns></returns>
        public abstract T Interpolate(CurvyInterpolatableMetadataBase<T> nextMetadata, float interpolationTime);

        //Implementing members from obsolete classes and interfaces that are kept in Curvy 6.0.0 to avoid breaking user's old code. Will be removed in Curvy 7.0.0
        #region Obsolete members
        [Obsolete("Use MetaDataValue instead")]
        public override object Value { get { return MetaDataValue; } }
        [Obsolete("Use Interpolate(CurvyInterpolatableMetadataBase<T>, float) instead")]
        public override object InterpolateObject(ICurvyMetadata b, float f) { return Interpolate((CurvyInterpolatableMetadataBase<T>)b, f); }
        [Obsolete("Use Interpolate(CurvyInterpolatableMetadataBase<T>, float) instead")]
        public T Interpolate(ICurvyMetadata b, float f) { return Interpolate((CurvyInterpolatableMetadataBase<T>)b, f); }
        #endregion

    }
}
