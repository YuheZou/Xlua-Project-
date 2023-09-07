using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class BuildTool : Editor
{

    [MenuItem("Tools/Build Windows Bundle")]
    static void BundleWindowsBuild()
    {
        Build(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Tools/Build Android Bundle")]
    static void BundleAndroidBuild()
    {
        Build(BuildTarget.Android);
    }

    [MenuItem("Tools/Build IOS Bundle")]
    static void BundleIOSBuild()
    {
        Build(BuildTarget.iOS);
    }


    static void Build(BuildTarget target)
    {
        List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
        //文件信息列表
        List<string> bundleInfos = new List<string>();
        //搜索所有文件的文件名
        string[] files = Directory.GetFiles(PathUtil.BuildResourcesPath, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta"))
            {
                continue;
            }
            AssetBundleBuild assetBundle = new AssetBundleBuild();

            string fileName = PathUtil.GetStandardPath(files[i]);
            Debug.Log("files:" + files[i]);

            string assetName = PathUtil.GetUnityPath(files[i]);
            assetBundle.assetNames = new string[] { assetName };
            string bundleName = files[i].Replace(PathUtil.BuildResourcesPath, "").ToLower();
            assetBundle.assetBundleName = bundleName + ".ab";
            assetBundleBuilds.Add(assetBundle);

            //添加文件信息和依赖信息
            List<string> dependenceInfo = GetDependence(assetName);
            string bundleInfo = assetName + "|" + bundleName + ".ab"; ;

            if (dependenceInfo.Count > 0)
            {
                bundleInfo = bundleInfo + "|" + string.Join("|", dependenceInfo);       //Join方法就是把list里的元素用“|”分隔开
            }
            bundleInfos.Add(bundleInfo);
        }



        if (Directory.Exists(PathUtil.BundleOutPath))
        {
            Directory.Delete(PathUtil.BundleOutPath, true);       //true在这里表示需要递归删除，即删除所有子文件
        }
        Directory.CreateDirectory(PathUtil.BundleOutPath);

        BuildPipeline.BuildAssetBundles(PathUtil.BundleOutPath, assetBundleBuilds.ToArray(), BuildAssetBundleOptions.None, target);
        //                               输出路径                          build列表         压缩格式（默认）              目标平台

        File.WriteAllLines(PathUtil.BundleOutPath + "/" + AppConst.FileListName, bundleInfos);

        AssetDatabase.Refresh();
    }


    /* 版本文件包含  版本号   以及   文件信息
     * 文件信息由 文件路径名|bundle名|依赖文件列表 组成
     * 首先获取依赖文件列表
     */
    static List<string> GetDependence(string curFile)
    {
        List<string> dependence = new List<string>();
        string[] files = AssetDatabase.GetDependencies(curFile);
        dependence = files.Where(file => !file.EndsWith(".cs") && !file.Equals(curFile)).ToList();    //不包含自身以及脚本文件
        return dependence;
    }
    


}

