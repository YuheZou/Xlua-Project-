using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        //加载文件
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/ui/prefabs/testui.prefab.ab");
        yield return request;
        AssetBundleRequest bundleRequest = request.assetBundle.LoadAssetAsync("Assets/BuildResources/UI/Prefabs/TestUI.prefab");
        yield return bundleRequest;
        
        //实例化
        GameObject Go = Instantiate(bundleRequest.asset) as GameObject;
        Go.transform.SetParent(this.transform);
        Go.SetActive(true);
        Go.transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
