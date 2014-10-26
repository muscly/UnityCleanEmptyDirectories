using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace AltProg.CleanEmptyDir
{
    [InitializeOnLoad]
    public class Core : UnityEditor.AssetModificationProcessor
    {
        const string CLEAN_ON_SAVE_KEY = "k1";
        static bool cleanOnSave;

        public static event Action OnAutoClean;

        // UnityEditor.AssetModificationProcessor
        public static string[] OnWillSaveAssets(string[] paths)
        {
            if ( CleanOnSave )
            {
                List<DirectoryInfo> emptyDirs;
                FillEmptyDirList( out emptyDirs );
                if ( emptyDirs != null && emptyDirs.Count > 0 )
                {
                    DeleteAllEmptyDirAndMeta( ref emptyDirs );

                    Debug.Log( "[Clean] Cleaned Empty Directories on Save" );

                    if ( OnAutoClean != null )
                        OnAutoClean();
                }
            }

            return paths;
        }


        public static bool CleanOnSave
        {
            get 
            {
                return EditorPrefs.GetBool( CLEAN_ON_SAVE_KEY, false );
            }
            set
            {
                EditorPrefs.SetBool( CLEAN_ON_SAVE_KEY, value );
            }
        }


        public static void DeleteAllEmptyDirAndMeta( ref List<DirectoryInfo> emptyDirs )
        {
            foreach (var dirInfo in emptyDirs)
            {
                AssetDatabase.MoveAssetToTrash( GetRelativePathFromCd( dirInfo.FullName ) );

//                try
//                {
//                    dirInfo.Delete();
//                }
//                catch( Exception e )
//                {
//                    Debug.LogException ( e );
//                }
//
//                var metaFilePath = GetMetaFilePath( dirInfo.FullName );
//                try
//                {
//                    File.Delete( metaFilePath );
//                }
//                catch( Exception e )
//                {
//                    Debug.LogException ( e );
//                }
            }

            emptyDirs = null;
        }

        public static void FillEmptyDirList( out List<DirectoryInfo> emptyDirs )
        {
            var newEmptyDirs = new List<DirectoryInfo>();
            emptyDirs = newEmptyDirs;

            var assetDir = new DirectoryInfo(Application.dataPath);

            WalkDirectoryTree(assetDir, ( dirInfo, areSubDirsEmpty ) =>
            {
                bool isDirEmpty = areSubDirsEmpty && DirHasNoFile (dirInfo);
                if ( isDirEmpty )
                    newEmptyDirs.Add(dirInfo);
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
            } 
            catch (Exception)
            {
            } 

            return files == null || files.Length == 0;
        }

        static string GetRelativePathFromCd(string filespec)
        {
            return GetRelativePath( filespec, Directory.GetCurrentDirectory() );
        }

        public static string GetRelativePath(string filespec, string folder)
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
    
    