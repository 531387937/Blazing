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
    [ExecuteInEditMode]
    [Obsolete("Use CurvyInterpolatableMetadataBase<T> class instead")]
    public abstract class CurvyInterpolatableMetadataBase : CurvyMetadataBase, ICurvyInterpolatableMetadata
    {
        [Obsolete("Use CurvyInterpolatableMetadataBase<T>.MetaDataValue instead")]
        public abstract object Value { get; }
        [Obsolete("Use CurvyInterpolatableMetadataBase<T>.Interpolate instead")]
        public abstract object InterpolateObject(ICurvyMetadata b, float f);
    }
}
