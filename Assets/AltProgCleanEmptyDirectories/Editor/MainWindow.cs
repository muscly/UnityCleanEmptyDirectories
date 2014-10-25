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

            WalkDirectoryTree(assetDir, ( dirInfo, areSubDirsEmpty ) =>
            {
				bool isDirEmpty = areSubDirsEmpty && DirHasNoFile (dirInfo);
				if ( isDirEmpty )
                    emptyDirs.Add(dirInfo);
				return isDirEmpty;
            });
        }

		// return: Is this directory empty?
		delegate bool IsEmptyDirectory( DirectoryInfo dirInfo, bool areSubDirsEmpty );

		// return: Is this directory empty?
        static bool WalkDirectoryTree(DirectoryInfo root, IsEmptyDirectory pred)
        {
            DirectoryInfo[] subDirs = root.GetDirectories();

			bool areSubDirsEmpty = true;
            foreach (DirectoryInfo dirInfo in subDirs)
            {
				if ( false == WalkDirectoryTree(dirInfo, pred) )
					areSubDirsEmpty = false;
            }

            bool isRootEmpty = pred(root, areSubDirsEmpty);
			return isRootEmpty;
        }

        static bool DirHasNoFile(DirectoryInfo dirInfo)
        {
            FileInfo[] files = null;

            try
            {
                files = dirInfo.GetFiles("*.*");
				files = files.Where ( x => ! IsMetaFile(x.Name)).ToArray ();

				foreach( var file in files )
				{
					Debug.Log ( string.Format ( "dir:{0}, file:{1}", dirInfo.Name, file.Name ) );
				}
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

		static bool IsMetaFile(string path)
		{
			return path.EndsWith(".meta");
		}


    }

}
