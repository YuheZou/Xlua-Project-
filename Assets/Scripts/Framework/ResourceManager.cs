using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UObject = UnityEngine.Object;

public class ResourceManager : MonoBehaviour
{
    internal class BundleInfo
    {
        public string AssetName;
        public string BundleName;
        public List<string> Dependences;
    }

    //存放bundle信息的集合
    private Dictionary<string, BundleInfo> m_BundleInfos = new Dictionary<string, BundleInfo>();
    
    //解析版本文件的方法
    private void ParseVersionFile()
    {
        //版本文件的路径
        string url = Path.Combine(PathUtil.BundleResourcePath, AppConst.FileListName);
        string[] data = File.ReadAllLines(url);


        //解析文件信息
        for (int i = 0; i < data.Length; i++)
        {
            BundleInfo bundleInfo = new BundleInfo();
            string[] info = data[i].Split('|');

            bundleInfo.AssetName = info[0];
            bundleInfo.BundleName = info[1];
            bundleInfo.Dependences = new List<string>(info.Length - 2);

            for (int j = 2; j < info.Length; j++)
            {
                bundleInfo.Dependences.Add(info[j]);
            }
            m_BundleInfos.Add(bundleInfo.AssetName, bundleInfo);

        }
    }

    //异步加载资源                     资源名            完成回调
    IEnumerator LoadBundleAsync(string assetName, Action<UObject> action = null)
    {
        string bundleName = m_BundleInfos[assetName].BundleName;
        string bundlePath = Path.Combine(PathUtil.BundleResourcePath, bundleName);
        List<string> dependences = m_BundleInfos[assetName].Dependences;
        if (dependences != null && dependences.Count > 0)
        {
            for (int i = 0; i < dependences.Count; i++)
            {
                yield return LoadBundleAsync(dependences[i]);
            }
        }
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return request;

        AssetBundleRequest bundleRequest = request.assetBundle.LoadAssetAsync(assetName);
        yield return bundleRequest;

        action?. Invoke(bundleRequest?.asset);       //检查置空
    }

    //为外部创造公共接口
    public void LoadAsset(string assetName, Action<UObject> action)
    {
        StartCoroutine(LoadBundleAsync(assetName, action));
    }

    void Start()
    {
        ParseVersionFile();
        //加载文件
        LoadAsset("Assets/BuildResources/UI/Prefabs/TestUI.prefab", OnComplete);
    }

    private void OnComplete(UObject obj)
    {
        //实例化
        GameObject Go = Instantiate(obj) as GameObject;
        Go.transform.SetParent(this.transform);
        Go.SetActive(true);
        Go.transform.localPosition = Vector3.zero;
    }
}
