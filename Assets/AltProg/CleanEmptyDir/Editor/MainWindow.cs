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

        [MenuItem("Window/AltProg Clean Empty Dir")]
        public static void ShowWindow()
        {
            var w = GetWindow<MainWindow>();
            w.title = "Clean";
        }

		void OnEnable()
		{
            lastCleanOnSave = Core.CleanOnSave;
		}

		void OnDisable()
		{
            Core.CleanOnSave = lastCleanOnSave;
		}

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
				bool cleanOnSave = GUILayout.Toggle ( lastCleanOnSave, "Clean Empty Dirs Automatically On Save" );
				if ( cleanOnSave != lastCleanOnSave )
                {
					lastCleanOnSave = cleanOnSave;
                    Core.CleanOnSave = cleanOnSave;
				}

				EditorGUILayout.BeginHorizontal();
				{
	                if (GUILayout.Button("Find Empty Dirs"))
	                {
	                    Core.FillEmptyDirList( out emptyDirs );
	                }

	                if ( emptyDirs == null || emptyDirs.Count == 0 )
	                {
						GUI.enabled = false;
					}

					Color old = GUI.color;
					GUI.color = Color.red;
                    if (GUILayout.Button("Delete All"))
                    {
                        Core.DeleteAllEmptyDirAndMeta( ref emptyDirs );
                    }
					GUI.color = old;

					GUI.enabled = true;
				}
				EditorGUILayout.EndHorizontal();	



                if (emptyDirs != null)
                {
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));
                    {
                        EditorGUILayout.BeginVertical();
                        {

                            foreach (var dirInfo in emptyDirs)
                            {
                                GUILayout.Label( Core.GetRelativePath( dirInfo.FullName, Application.dataPath ) );
                            }

                        }
                        EditorGUILayout.EndVertical();

                    }
                    EditorGUILayout.EndScrollView();
                }

            }
            EditorGUILayout.EndVertical();
        }





    }

}
