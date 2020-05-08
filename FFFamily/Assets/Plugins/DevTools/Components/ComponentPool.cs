// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

#if UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0
#define PRE_UNITY_5_4
#endif

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevTools.Extensions;
#if !PRE_UNITY_5_4
using UnityEngine.SceneManagement;
#endif

namespace FluffyUnderware.DevTools
{
    public class ComponentPool : MonoBehaviour, IPool, ISerializationCallbackReceiver
    {
        [SerializeField, HideInInspector]
        private string m_Identifier;

        [Inline]
        [SerializeField]
        private PoolSettings m_Settings;

        public PoolSettings Settings
        {
            get { return m_Settings; }
            set
            {
                if (m_Settings != value)
                    m_Settings = value;
                if (m_Settings != null)
                    m_Settings.OnValidate();
            }
        }

        private PoolManager mManager;

        public PoolManager Manager
        {
            get
            {
                if (mManager == null)
                    mManager = GetComponent<PoolManager>();
                return mManager;
            }
        }

        /// <summary>
        /// Due to bad design decisions, Identifier is used to store the type of the pooled objects. And the setter does nothing
        /// </summary>
        public string Identifier
        {
            get { return m_Identifier; }
            set
            {
                throw new InvalidOperationException("Component pool's identifier should always indicate the pooled type's assembly qualified name");
                /*Here is why:
                In the Initialize method, m_Identifier is set as a fully qualified type name.
                The Type getter uses m_Identifier as a fully qualified type name.
                If needed, add a field that contains the pooled type name, and use it instead of Identifier when you need to find the righ pool for the right component type*/
            }
        }

        /// <summary>
        /// The type of the pooled objects
        /// </summary>
        public Type Type
        {
            get
            {
                Type type = Type.GetType(Identifier);
                if (type == null)
                    DTLog.LogWarning("[DevTools] ComponentPool's Type is an unknown type " + m_Identifier);
                return type;
            }
        }


        public int Count
        {
            get { return mObjects.Count; }
        }

        private List<Component> mObjects = new List<Component>();

        private double mLastTime;
        private double mDeltaTime;

        public void Initialize(Type type, PoolSettings settings)
        {
            m_Identifier = type.AssemblyQualifiedName;
            m_Settings = settings;
            mLastTime = DTTime.TimeSinceStartup + UnityEngine.Random.Range(0, Settings.Speed);
            if (Settings.Prewarm)
                Reset();
        }

        private void Start()
        {
            if (Settings.Prewarm)
                Reset();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Settings = m_Settings;
        }
#endif

        private void OnEnable()
        {
#if !PRE_UNITY_5_4
            SceneManager.sceneLoaded += OnSceneLoaded;
#endif
        }

        private void OnDisable()
        {
        }

        public void Update()
        {
            if (Application.isPlaying)
            {
                mDeltaTime += DTTime.TimeSinceStartup - mLastTime;
                mLastTime = DTTime.TimeSinceStartup;

                if (Settings.Speed > 0)
                {
                    int c = (int)(mDeltaTime / Settings.Speed);
                    mDeltaTime -= c;

                    if (Count > Settings.Threshold)
                    {
                        c = Mathf.Min(c, Count - Settings.Threshold);
                        while (c-- > 0)
                        {
                            if (Settings.Debug)
                                log("Threshold exceeded: Deleting item");
                            destroy(mObjects[0]);
                            mObjects.RemoveAt(0);
                        }
                    }
                    else if (Count < Settings.MinItems)
                    {
                        c = Mathf.Min(c, Settings.MinItems - Count);
                        while (c-- > 0)
                        {
                            if (Settings.Debug)
                                log("Below MinItems: Adding item");
                            mObjects.Add(create());
                        }
                    }
                }
                else
                    mDeltaTime = 0;
            }
        }

        public void Reset()
        {
            if (Application.isPlaying)
            {
                while (Count < Settings.MinItems)
                {
                    mObjects.Add(create());
                }
                while (Count > Settings.Threshold)
                {
                    destroy(mObjects[0]);
                    mObjects.RemoveAt(0);
                }
                if (Settings.Debug)
                    log("Prewarm/Reset");
            }
        }

#if !PRE_UNITY_5_4
        public void OnSceneLoaded(Scene scn, LoadSceneMode mode)
        {
            for (int i = mObjects.Count - 1; i >= 0; i--)
                if (mObjects[i] == null)
                    mObjects.RemoveAt(i);
        }
#endif

        public void Clear()
        {
            if (Settings.Debug)
                log("Clear");
            for (int i = 0; i < Count; i++)
                destroy(mObjects[i]);
            mObjects.Clear();
        }

        public void Push(Component item)
        {
            sendBeforePush(item);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(item.gameObject);
            }
            else
#endif
                if (item != null)
            {
                mObjects.Add(item);
                item.transform.parent = Manager.transform;
                item.gameObject.hideFlags = (Settings.Debug)
                    ? HideFlags.DontSave
                    : HideFlags.HideAndDontSave;
                if (Settings.AutoEnableDisable)
                    item.gameObject.SetActive(false);
            }
        }

        public Component Pop(Transform parent = null)
        {
            Component item = null;
            if (Count > 0)
            {
                item = mObjects[0];
                mObjects.RemoveAt(0);
            }
            else
            {
                if (Settings.AutoCreate || !Application.isPlaying)
                {
                    if (Settings.Debug)
                        log("Auto create item");
                    item = create();
                }
            }
            if (item)
            {
                item.gameObject.hideFlags = HideFlags.None;
                item.transform.parent = parent;
                if (Settings.AutoEnableDisable)
                    item.gameObject.SetActive(true);
                sendAfterPop(item);
                if (Settings.Debug)
                    log("Pop " + item);
            }

            return item;
        }

        public T Pop<T>(Transform parent) where T : Component
        {
            return Pop(parent) as T;
        }

        private Component create()
        {
            var go = new GameObject();
            go.name = Identifier;
            go.transform.parent = Manager.transform;
            if (Settings.AutoEnableDisable)
                go.SetActive(false);
            var c = go.AddComponent(Type);
            return c;
        }

        private void destroy(Component item)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        private void setParent(Component item, Transform parent)
        {
            if (item != null)
                item.transform.parent = parent;
        }

        private void sendAfterPop(Component item)
        {
            GameObject itemGameObject = item.gameObject;
            if (itemGameObject.activeSelf && itemGameObject.activeInHierarchy)
                //Send message works only for active game objects. Source: https://docs.unity3d.com/ScriptReference/GameObject.SendMessage.html
                itemGameObject.SendMessage("OnAfterPop", SendMessageOptions.DontRequireReceiver);
            else
            {
                if (item is IPoolable)
                    ((IPoolable)item).OnAfterPop();
                else
                    DTLog.LogWarning("[Curvy] sendAfterPop could not send message because the receiver " + item.name + " is not active");
            }
        }

        private void sendBeforePush(Component item)
        {
            GameObject itemGameObject = item.gameObject;
            if (itemGameObject.activeSelf && itemGameObject.activeInHierarchy)
                //Send message works only for active game objects. Source: https://docs.unity3d.com/ScriptReference/GameObject.SendMessage.html
                itemGameObject.SendMessage("OnBeforePush", SendMessageOptions.DontRequireReceiver);
            else
            {
                if (item is IPoolable)
                    ((IPoolable)item).OnBeforePush();
                else
                    DTLog.LogWarning("[Curvy] sendBeforePush could not send message because the receiver " + item.name + " is not active");
            }
        }

        private void log(string msg)
        {
            Debug.Log(string.Format("[{0}] ({1} items) {2}", Identifier, Count, msg));
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (Type.GetType(m_Identifier) == null)
            {

                //Handles cases where the component is migrated to another assembly (for example if using Unity's assembly definitions feature

                const char separator = ',';
                const string separatorString = ",";
                string[] splittedAssemblyQualifiedName = m_Identifier.Split(separator);
                if (splittedAssemblyQualifiedName.Length >= 5)
                {
                    string typeName = String.Join(separatorString, splittedAssemblyQualifiedName.SubArray(0, splittedAssemblyQualifiedName.Length - 4));
                    //As you can see with this example: 
                    //"FluffyUnderware.Curvy.CurvySplineSegment, ToolBuddy.Curvy, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
                    //the 4 last elements do not contain the type name. Keep in mind that a type name can include a ',' like  Dictionary<int, List<double>>

#if NETFX_CORE
                    Type[] knownTypes = this.GetType().GetAllTypes();
#else
                    Type[] knownTypes = TypeExt.GetLoadedTypes();

#endif
                    Type type = knownTypes.FirstOrDefault(t => t.FullName == typeName);
                    if (type != null)
                        m_Identifier = type.AssemblyQualifiedName;
                }
            }
        }
    }
}
