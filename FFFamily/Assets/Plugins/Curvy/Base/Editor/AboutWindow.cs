// =====================================================================
// Copyright 2013-2018 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.DevToolsEditor.Extensions;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.CurvyEditor
{

    public class AboutWindow : EditorWindow
    {
        public static List<string> Plugins = new List<string>();

        static bool heightHasBeenSet = false;


        public static void Open()
        {
            EditorWindow.GetWindow<AboutWindow>(true, "About Curvy");
        }

        void OnEnable()
        {
            CurvyProject.Instance.ShowAboutOnLoad = false;
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label(new GUIContent(CurvyStyles.TexLogoBig));
            var r = new Rect(250, 30, 215, 40);
            DTGUI.PushContentColor(Color.black);

            var st = new GUIStyle(EditorStyles.label);
            st.alignment = TextAnchor.UpperRight;
            st.richText = true;
            GUI.Label(r, "<b>Curvy " + CurvySpline.VERSION + "</b>", st);
            r.y += 20;
            foreach (var pl in Plugins)
            {
                GUI.Label(r, "<b>" + pl + "</b>", st);
                r.y += 20;
            }
            //GUI.Label(r, "<b>DevTools " + DT.VERSION + "</b>",st);//\n(c) 2015 Fluffy Underware",st);
            r = new Rect(280, 125, 185, 20);
            GUI.Label(r, "(c) 2013-2018 ToolBuddy");
            DTGUI.PopContentColor();
            GUILayout.Space(10);

            head("What's new?");

            if (buttonCol("Release notes", "View release notes and upgrade instructions!"))
                OpenReleaseNotes();
            foot();
            head("Learning Resources");
            if (buttonCol("View Examples", "Show examples folder in the Project window"))
                ShowExamples();
            if (buttonCol("Tutorials", "Watch some tutorials"))
                OpenTutorials();
            if (buttonCol("Documentation", "Manuals! That magic source of wisdom!"))
                OpenDocs();
            if (buttonCol("API Reference", "Browse the API reference"))
                OpenAPIDocs();
            if (buttonCol("Support Forum", "Visit Support forum"))
                OpenForum();

            foot();
            head("Links");
            if (buttonCol("Website", "Visit Curvy's product website"))
                OpenWeb();

            if (buttonCol("Submit a bug report", "Found a bug? Please issue a bug report!"))
                CurvyEditorUtility.SendBugReport();
            foot();

            GUILayout.EndVertical();

            if (!heightHasBeenSet && Event.current.type == EventType.Repaint)
                setHeightToContent();
        }

        private void setHeightToContent()
        {
            var w = 500;
            var height = GUILayoutUtility.GetLastRect().height + 10f;
            position.Set(position.x, position.y, w, height);
            minSize = new Vector2(w, height);
            maxSize = new Vector2(w, height + 1);
            heightHasBeenSet = true;
        }

        void head(string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.Label(text, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void foot()
        {
            GUILayout.Space(5);
        }

        bool buttonCol(string btnText, string text)
        {
            return buttonCol(new GUIContent(btnText), text);
        }

        bool buttonCol(GUIContent btn, string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            bool res = GUILayout.Button(btn, GUILayout.Width(150));
            GUILayout.Space(30);
            EditorGUILayout.LabelField("<i>" + text + "</i>", DTStyles.HtmlLabel);
            GUILayout.EndHorizontal();
            return res;
        }

        public static void ShowExamples()
        {
            string searchString;
#if UNITY_2017_4_OR_NEWER
            searchString = "t:Folder Curvy Examples";
#else
            searchString = "Curvy Examples";
#endif
            string[] assetsGuids = AssetDatabase.FindAssets(searchString);
            if (assetsGuids.Any())
                EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(assetsGuids.First())));
            else
                DTLog.LogError("[Curvy] Could not find the \"Curvy Examples\" folder");
        }

        public static void OpenTutorials()
        {
            Application.OpenURL(CurvySpline.DOCLINK + "tutorials");
        }
        public static void OpenReleaseNotes()
        {
            Application.OpenURL(CurvySpline.DOCLINK + "releasenotes");
        }

        public static void OpenDocs()
        {
            Application.OpenURL(CurvySpline.WEBROOT + "documentation/");
        }

        public static void OpenAPIDocs()
        {
            Application.OpenURL("https://api.curvyeditor.com/" + CurvySpline.APIVERSION + "/");
        }

        public static void OpenWeb()
        {
            Application.OpenURL(CurvySpline.WEBROOT);
        }

        public static void OpenForum()
        {
            Application.OpenURL("https://forum.curvyeditor.com");
        }


    }
}
