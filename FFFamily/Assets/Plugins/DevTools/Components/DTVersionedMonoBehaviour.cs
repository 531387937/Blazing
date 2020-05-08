// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using System;
using UnityEngine;
using System.Collections;

namespace FluffyUnderware.DevTools
{
    public class DTVersionedMonoBehaviour : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        string m_Version;

        /// <summary>
        /// Gets the version of this component
        /// </summary>
        public string Version
        {
            get { return m_Version; }
            protected set { m_Version = value; }
        }

        /// <summary>
        /// (Editor only) Checks for a new version and calls UpgradeVersion() if neccessary
        /// </summary>
        [Obsolete("This upgrading mechanism is tied to the Editor. Meaning it does not work when in Play mode. A better way to handle upgrading would be to use Unity's ISerializationCallbackReceiver")]
        protected void CheckForVersionUpgrade()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var versionInfo = GetType().GetCustomAttributes(typeof(DTVersionAttribute), true);
                if (versionInfo.Length > 0)
                {
                    string newVersion = ((DTVersionAttribute)versionInfo[0]).Version;
                    if (!string.IsNullOrEmpty(newVersion) && string.Compare(Version,newVersion)==-1)//Version != newVersion)
                    {
                        if (UpgradeVersion(Version, newVersion))
                            m_Version = newVersion;
                    }

                }
            }
#endif
        }

        /// <summary>
        /// (Editor only) Performs a version upgrade
        /// </summary>
        /// <param name="oldVersion">the old version (serialized in the Component)</param>
        /// <param name="newVersion">the new version (read from the DTVersion attribute)</param>
        /// <returns>true to store the new version in the component, false to fail</returns>
        [Obsolete("This upgrading mechanism is tied to the Editor. Meaning it does not work when in Play mode. A better way to handle upgrading would be to use Unity's ISerializationCallbackReceiver")]
        protected virtual bool UpgradeVersion(string oldVersion, string newVersion)
        {
            if (string.IsNullOrEmpty(oldVersion))
                Debug.LogFormat("[{0}] Upgrading '{1}' to version {2}! PLEASE SAVE THE SCENE!", GetType().Name, name, newVersion);
            else
                Debug.LogFormat("[{0}] Upgrading '{1}' from version {2} to {3}! PLEASE SAVE THE SCENE!", GetType().Name, name, oldVersion, newVersion);
            return true;
        }

        /// <summary>
        /// Destroys the gameobject
        /// </summary>
        public void Destroy()
        {
            if (Application.isPlaying)
                GameObject.Destroy(gameObject);
            else
                GameObject.DestroyImmediate(gameObject);
        }
    }
}
