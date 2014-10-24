using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AltProg.CleanEmptyDir
{
    public class MainWindow : EditorWindow
    {
        List<DirectoryInfo> emptyDirs;
        Vector2 scrollPosition;

        [MenuItem("Window/AltProg Clean Empty Dir")]
        public static void ShowWindow()
        {
            var w = GetWindow<MainWindow>();
            w.title = "Clean";
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {

                if (GUILayout.Button("Find Empty Dirs"))
                {
                    FillEmptyDirList();
                }

                if ( emptyDirs != null && emptyDirs.Count > 0 )
                {
                    if (GUILayout.Button("Delete All"))
                    {
                        DeleteAllEmptyDirAndMeta();
                    }
                }

                if (emptyDirs != null)
                {
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));
                    {
                        EditorGUILayout.BeginVertical();
                        {

                            foreach (var dirInfo in emptyDirs)
                            {
                                GUILayout.Label( GetRelativePath( dirInfo.FullName, Application.dataPath ) );
                            }

                        }
                        EditorGUILayout.EndVertical();

                    }
                    EditorGUILayout.EndScrollView();
                }

            }
            EditorGUILayout.EndVertical();
        }

        void DeleteAllEmptyDirAndMeta()
        {
            foreach (var dirInfo in emptyDirs)
            {
                try
                {
                    dirInfo.Delete();
                }
                catch( Exception e )
                {
                    Debug.LogException ( e );
                }

                var metaFilePath = GetMetaFilePath( dirInfo.FullName );
                try
                {
                    File.Delete( metaFilePath );
                }
                catch( Exception e )
                {
                    Debug.LogException ( e );
                }
            }

            emptyDirs = null;
        }

        void FillEmptyDirList()
        {
            emptyDirs = new List<DirectoryInfo>();

            var assetDir = new DirectoryInfo(Application.dataPath);

            WalkDirectoryTree(assetDir, ( dirInfo ) =>
            {
                if (IsEmptyDirectory(dirInfo))
                    emptyDirs.Add(dirInfo);
            });
        }

        static void WalkDirectoryTree(DirectoryInfo root, Action<DirectoryInfo> job)
        {
            job(root);

            DirectoryInfo[] subDirs = null;

            subDirs = root.GetDirectories();

            foreach (DirectoryInfo dirInfo in subDirs)
            {
                WalkDirectoryTree(dirInfo, job);
            }
        }

        static bool IsEmptyDirectory(DirectoryInfo dirInfo)
        {
            FileInfo[] files = null;

            try
            {
                files = dirInfo.GetFiles("*.*");
            } catch (UnauthorizedAccessException)
            {
            } catch (DirectoryNotFoundException)
            {
            }

            return files == null || files.Length == 0;
        }

        static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        static string GetMetaFilePath(string dirPath)
        {
            // TODO: remove ending slash
            return dirPath + ".meta";
        }


    }

}
