using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace AltProg.CleanEmptyDir
{
    public class MainWindow : EditorWindow
    {
        List<DirectoryInfo> emptyDirs;
        Vector2 scrollPosition;
        bool lastCleanOnSave;
        string delayedNotiMsg;
        UpdateChecker.Message updateMsg;
        GUIStyle updateMsgStyle;

        bool hasNoEmptyDir { get { return emptyDirs == null || emptyDirs.Count == 0; } }

        const float DIR_LABEL_HEIGHT = 21;

        [MenuItem("Window/AltProg Clean Empty Dir")]
        public static void ShowWindow()
        {
            var w = GetWindow<MainWindow>();
            w.title = "Clean";
        }

        void OnEnable()
        {
            lastCleanOnSave = Core.CleanOnSave;
            Core.OnAutoClean += Core_OnAutoClean;
            UpdateChecker.OnDone += UpdateChecker_OnDone;

            UpdateChecker.Check();
            delayedNotiMsg = "Click 'Find Empty Dirs' Button.";
        }
        
        void OnDisable()
        {
            Core.CleanOnSave = lastCleanOnSave;
            Core.OnAutoClean -= Core_OnAutoClean;
            UpdateChecker.OnDone -= UpdateChecker_OnDone;
        }

        void UpdateChecker_OnDone( UpdateChecker.Message updateMsg )
        {
            this.updateMsg = updateMsg;
        }

        void Core_OnAutoClean()
        {
            delayedNotiMsg = "Cleaned on Save";
        }

        void OnGUI()
        {
            if ( delayedNotiMsg != null )
            {
                ShowNotification( new GUIContent( delayedNotiMsg ) );
                delayedNotiMsg = null;
            }

            EditorGUILayout.BeginVertical();
            {
                if ( null != updateMsg )
                {
                    if ( updateMsgStyle == null )
                    {
                        updateMsgStyle = new GUIStyle( "CN EntryInfo" );
                        updateMsgStyle.alignment = TextAnchor.MiddleLeft;
                        updateMsgStyle.richText = true;
                    }

                    if ( GUILayout.Button( updateMsg.Msg , updateMsgStyle) )
                    {
                        Application.OpenURL( updateMsg.Link );
                    }
                }

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Find Empty Dirs"))
                    {
                        Core.FillEmptyDirList(out emptyDirs);

                        if (hasNoEmptyDir)
                        {
                            ShowNotification( new GUIContent( "No Empty Directory" ) );
                        }
                        else
                        {
                            RemoveNotification();
                        }
                    }




                    if ( ColorButton( "Delete All", ! hasNoEmptyDir, Color.red ) )
                    {
                        Core.DeleteAllEmptyDirAndMeta(ref emptyDirs);
                        ShowNotification( new GUIContent( "Deleted All" ) );
                    }
                }
                EditorGUILayout.EndHorizontal();    


                bool cleanOnSave = GUILayout.Toggle(lastCleanOnSave, " Clean Empty Dirs Automatically On Save");
                if (cleanOnSave != lastCleanOnSave)
                {
                    lastCleanOnSave = cleanOnSave;
                    Core.CleanOnSave = cleanOnSave;
                }

                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

                if ( ! hasNoEmptyDir )
                {
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));
                    {
                        EditorGUILayout.BeginVertical();
                        {
#if UNITY_4_6   // and higher
                            GUIContent folderContent = EditorGUIUtility.IconContent("Folder Icon");
#else
                            GUIContent folderContent = new GUIContent();
#endif

                            foreach (var dirInfo in emptyDirs)
                            {
                                UnityEngine.Object assetObj = Resources.LoadAssetAtPath( "Assets", typeof(UnityEngine.Object) );
                                if ( null != assetObj )
                                {
                                    folderContent.text = Core.GetRelativePath(dirInfo.FullName, Application.dataPath);
                                    GUILayout.Label( folderContent, GUILayout.Height( DIR_LABEL_HEIGHT ) );
                                }
                            }

                        }
                        EditorGUILayout.EndVertical();

                    }
                    EditorGUILayout.EndScrollView();
                }

            }
            EditorGUILayout.EndVertical();
        }


        void ColorLabel(string title, Color color)
        {
            Color oldColor = GUI.color;
            //GUI.color = color;
            GUI.enabled = false;
            GUILayout.Label(title);
            GUI.enabled = true;;
            GUI.color = oldColor;
        }
        
        bool ColorButton(string title, bool enabled, Color color)
        {
            bool oldEnabled = GUI.enabled;
            Color oldColor = GUI.color;

            GUI.enabled = enabled;
            GUI.color = color;

            bool ret = GUILayout.Button(title);

            GUI.enabled = oldEnabled;
            GUI.color = oldColor;
            
            return ret;
        }
    }

}
