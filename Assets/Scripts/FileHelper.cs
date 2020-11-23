using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Common
{
    //CustomLuaClassAttribute    
    static public class FileHelper
    {
        const string SAVE_PATH = "savePath.txt";

        static public void CreateDirectoryFromFile(string path)
        {
            path = path.Replace('\\', '/');
            var ind = path.LastIndexOf('/');
            if (ind >= 0)
            {
                path = path.Substring(0, ind);
            }
            else
            {
                return;
            }
            CreateDirectory(path);
        }
        static public void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        static public void SaveFile(string path, string content, bool needUtf8 = false)
        {
            CheckFileSavePath(path);
            if (needUtf8)
            {
                var encoding = new UTF8Encoding(false);
                File.WriteAllText(path, content, encoding);
            }
            else
            {
                File.WriteAllText(path, content, Encoding.Default);
            }
        }

        static public void SaveLine(string path, string content)
        {
            CheckFileSavePath(path);
            StreamWriter f = new StreamWriter(path, true);
            f.WriteLine(content);
            f.Close();
        }

        static public void SaveString(string path, string content)
        {
            //CheckFileSavePath(path);
            StreamWriter f = new StreamWriter(path, true);
            f.Write(content);
            f.Close();
        }
        static public void WriteLine(string path, string content)
        {
            StreamWriter f = new StreamWriter(path, true);
            f.WriteLine(content);
            f.Close();
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        static public void WriteAllText(string path, string content)
        {
            var encoding = new UTF8Encoding(false);
            File.WriteAllText(path, content, encoding);
        }

        static public void DelFolder(string path)
        {
            if (!IsDirectoryExists(path))
            {
                return;
            }
            Directory.Delete(path, true);
        }

        static public void DelFile(string path)
        {
            if (!IsFileExists(path))
            {
                return;
            }
            File.Delete(path);
        }

        static public void CleanFolder(string path)
        {
            if (!IsDirectoryExists(path))
            {
                return;
            }
            DOCleanFolder(path);
        }

        static void DOCleanFolder(string path)
        {
            DirectoryInfo source = new DirectoryInfo(path);
            FileInfo[] files = source.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                File.Delete(files[i].FullName);
            }
            DirectoryInfo[] dirs = source.GetDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                DOCleanFolder(dirs[i].FullName);
            }
        }


        /// <summary>
        /// 读取文件的字符串
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public string ReadFileText(string path, bool reportError = true)
        {
            if (!File.Exists(path))
            {
                if (reportError)
                {
                    Debug.LogError("unable to load file " + path);
                }
                return "";
            }
            var encoding = new UTF8Encoding(false);
            string str = File.ReadAllText(path, encoding);
            return str;
        }

        /// <summary>
        /// 检查某文件夹路径是否存在，如不存在，创建
        /// </summary>
        /// <param name="path"></param>
        static public bool CheckDirection(string path)
        {
            if (!Directory.Exists(path))
            {
                var info = Directory.CreateDirectory(path);
                if (info.Exists)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 单纯检查某个文件夹路径是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public bool IsDirectoryExists(string path)
        {
            if (Directory.Exists(path))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查某个文件是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public bool IsFileExists(string path)
        {
            if (File.Exists(path))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tarPath"></param>
        static public void CopyFile(string path, string tarPath)
        {
            if (!IsFileExists(path))
            {
                return;
            }
            CheckFileSavePath(tarPath);
            File.Copy(path, tarPath, true);
        }


        static public void CopyDirectory(string srcDir,
                                         string tgtDir,
                                         string[] skips_dir_contains = null,
                                         string[] skips_ext = null)
        {
            DirectoryInfo source = new DirectoryInfo(srcDir);
            DirectoryInfo target = new DirectoryInfo(tgtDir);

            if (target.FullName.StartsWith(source.FullName))
            {
				throw new Exception("父目录不能拷贝到子目录");
            }

            if (skips_dir_contains != null)
            {
                for (int j = 0; j < skips_dir_contains.Length; j++)
                {
                    if (srcDir.Contains(skips_dir_contains[j]))
                    {
                        Debug.LogFormat("skip dir {0}", srcDir);
                        return;
                    }
                }

            }


            if (!source.Exists)
            {
                return;
            }

            if (!target.Exists)
            {
                target.Create();
            }

            FileInfo[] files = source.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                bool isSkiped = false;
                if (skips_ext != null)
                {
                    for (int j = 0; j < skips_ext.Length; j++)
                    {
                        if (files[i].Name.EndsWith(skips_ext[j]))
                        {
                            isSkiped = true;
                            break;
                        }
                    }
                }
                if (isSkiped)
                {
                    //Debug.LogFormat("skip file {0}", files[i].FullName);
                    continue;
                }
#if UNITY_EDITOR_OSX
                File.Copy(files[i].FullName, target.FullName + @"/" + files[i].Name, true);
#else
                File.Copy(files[i].FullName, target.FullName + @"\" + files[i].Name, true);
#endif
            }

            DirectoryInfo[] dirs = source.GetDirectories();

            for (int j = 0; j < dirs.Length; j++)
            {
#if UNITY_EDITOR_OSX
                CopyDirectory(dirs[j].FullName,
                              target.FullName + @"/" + dirs[j].Name,
                              skips_dir_contains,
                              skips_ext);
#else
                CopyDirectory(dirs[j].FullName,
                              target.FullName + @"\" + dirs[j].Name,
                              skips_dir_contains,
                              skips_ext);
#endif
            }
        }

        static public bool CheckFileSavePath(string path)
        {
            path = path.Replace('\\', '/');
            var ind = path.LastIndexOf('/');
            if (ind >= 0)
            {
                path = path.Substring(0, ind);
            }
            else
            {
                return true;
            }
            return CheckDirection(path);
        }

        #region NotToLua
        /// <summary>
        /// 获取所有行
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public string[] ReadAllLines(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            return File.ReadAllLines(path);
        }

        /// <summary>
        /// 写入所有行
        /// </summary>
        /// <param name="path"></param>
        /// <param name="lines"></param>
        static public void SaveAllLines(string path, string[] lines)
        {
            CheckFileSavePath(path);
            File.WriteAllLines(path, lines);
        }
        /// <summary>
        /// 保存bytes
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        static public void SaveBytes(string path, byte[] bytes)
        {
            CheckFileSavePath(path);
            File.WriteAllBytes(path, bytes);
        }

        /// <summary>
        /// 读取bytes
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public byte[] ReadFileBytes(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            byte[] str = File.ReadAllBytes(path);
            return str;
        }
        /// <summary>
        /// 获取某目录下所有指定类型的文件的路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="exName"></param>
        /// <returns></returns>
        static public List<string> GetAllFiles(string path, string exName)
        {
            if (!IsDirectoryExists(path))
            {
                return null;
            }
            List<string> names = new List<string>();
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = root.GetFiles();
            string ex;
            for (int i = 0; i < files.Length; i++)
            {
                ex = GetExName(files[i].FullName);
                if (ex != exName)
                {
                    continue;
                }
                names.Add(files[i].FullName);
            }
            DirectoryInfo[] dirs = root.GetDirectories();
            if (dirs.Length > 0)
            {
                for (int i = 0; i < dirs.Length; i++)
                {
                    List<string> subNames = GetAllFiles(dirs[i].FullName, exName);
                    if (subNames.Count > 0)
                    {
                        for (int j = 0; j < subNames.Count; j++)
                        {
                            names.Add(subNames[j]);
                        }
                    }
                }
            }

            return names;

        }

        static public List<FileInfo> GetAllFileInfos(string path, string exName)
        {
            if (!IsDirectoryExists(path))
            {
                return null;
            }
            List<FileInfo> names = new List<FileInfo>();
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = root.GetFiles();
            string ex;
            for (int i = 0; i < files.Length; i++)
            {
                ex = GetExName(files[i].FullName);
                if (string.IsNullOrEmpty(exName) == false)
                {
                    if (ex != exName)
                    {
                        continue;
                    }
                }
                names.Add(files[i]);
            }
            DirectoryInfo[] dirs = root.GetDirectories();
            if (dirs.Length > 0)
            {
                for (int i = 0; i < dirs.Length; i++)
                {
                    List<FileInfo> subNames = GetAllFileInfos(dirs[i].FullName, exName);
                    if (subNames.Count > 0)
                    {
                        for (int j = 0; j < subNames.Count; j++)
                        {
                            names.Add(subNames[j]);
                        }
                    }
                }
            }

            return names;

        }

        /// <summary>
        /// 获取某目录下所有除了指定类型外的文件的路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="exName"></param>
        /// <returns></returns>
        static public List<string> GetAllFilesExcept(string path, string[] exName)
        {
            List<string> names = new List<string>();
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = root.GetFiles();
            string ex;
            for (int i = 0; i < files.Length; i++)
            {
                ex = GetExName(files[i].FullName);
                if (Array.IndexOf(exName, ex) != -1)
                {
                    continue;
                }
                names.Add(files[i].FullName);
            }
            DirectoryInfo[] dirs = root.GetDirectories();
            if (dirs.Length > 0)
            {
                for (int i = 0; i < dirs.Length; i++)
                {
                    List<string> subNames = GetAllFilesExcept(dirs[i].FullName, exName);
                    if (subNames.Count > 0)
                    {
                        for (int j = 0; j < subNames.Count; j++)
                        {
                            names.Add(subNames[j]);
                        }
                    }
                }
            }

            return names;

        }

        /// <summary>
        /// 获取指定路径下第一层的子文件夹路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public List<string> GetSubFolders(string path)
        {
            if (!IsDirectoryExists(path))
            {
                return null;
            }
            DirectoryInfo root = new DirectoryInfo(path);

            DirectoryInfo[] dirs = root.GetDirectories();
            List<string> folders = new List<string>();
            if (dirs.Length > 0)
            {
                for (int i = 0; i < dirs.Length; i++)
                {
                    folders.Add(dirs[i].FullName);
                }
            }

            return folders;

        }

        /// <summary>
        /// 获取指定路径下一层的指定格式的文件的路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="exName"></param>
        /// <returns></returns>
        static public List<string> GetSubFiles(string path, string exName)
        {
            List<string> names = new List<string>();
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = root.GetFiles();
            string ex;
            for (int i = 0; i < files.Length; i++)
            {
                ex = GetExName(files[i].FullName);
                if (ex != exName)
                {
                    continue;
                }
                names.Add(files[i].FullName);
            }
            return names;
        }

        /// <summary>
        /// 获取指定路径下一层的除指定格式以外的文件的路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="exName"></param>
        /// <returns></returns>
        static public List<string> GetSubFilesExcept(string path, string exName)
        {
            List<string> names = new List<string>();
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = root.GetFiles();
            string ex;
            for (int i = 0; i < files.Length; i++)
            {
                ex = GetExName(files[i].FullName);
                if (ex == exName)
                {
                    continue;
                }
                names.Add(files[i].FullName);
            }

            return names;
        }

        static public AssetBundle LoadAbFormFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            AssetBundle ab = AssetBundle.LoadFromFile(path);
            if (ab != null)
            {
                return ab;
            }
            return null;
        }

        /// <summary>
        /// 获得包名
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static public string GetPackName(string str)
        {
            string rexStr = @"(?<=\\)[^\\\.]+$";
            return GetFirstMatch(str, rexStr);
        }

        /// <summary>
        /// 获得表名
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static public string GetTableName(string str)
        {
            string rexStr = @"(?<=\\)[^\\\.]+(?=\.)";
            return GetFirstMatch(str, rexStr);
        }

        /// <summary>
        /// 获得文件名
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static public string GetFileName(string str)
        {
            string rexStr = @"(?<=\\)[^\\]+$|(?<=/)[^/]+$";
            return GetFirstMatch(str, rexStr);
        }

        /// <summary>
        /// 获得扩展名
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static public string GetExName(string str)
        {
            string rexStr = @"(?<=\\[^\\]+.)[^\\.]+$|(?<=/[^/]+.)[^/.]+$";
            return GetFirstMatch(str, rexStr);

        }

        /// <summary>
        /// 去除扩展名
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static public string RemoveExName(string str)
        {
            string returnStr = str;
            string rexStr = @"[^\.]+(?=\.)";
            string xStr = GetFirstMatch(str, rexStr);
            if (!string.IsNullOrEmpty(xStr))
            {
                returnStr = xStr;
            }
            return returnStr;
        }

		/// <summary>
		/// 获取第一个匹配
		/// </summary>
		/// <param name="str"></param>
		/// <param name="regexStr"></param>
		/// <returns></returns>
		static public string GetFirstMatch(string str, string regexStr)
		{
			Match m = Regex.Match(str, regexStr);
			if (!string.IsNullOrEmpty(m.ToString()))
			{
				return m.ToString();
			}
			return null;
		}
        #endregion
    }
}
