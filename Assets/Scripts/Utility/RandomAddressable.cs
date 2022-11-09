using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class RandomAddressable
{
    public static GameObject Instantiate(AssetLabelReference label,Vector3 position,Quaternion rotation,Transform parent)
    {
        return Addressables.InstantiateAsync(LoadLocation(label), position, rotation, parent).WaitForCompletion();
    }
    public static AsyncOperationHandle<T> LoadAsset<T>(AssetLabelReference label)
    {
        return Addressables.LoadAssetAsync<T>(LoadLocation(label));
    }

    

    internal static IResourceLocation LoadLocation(AssetLabelReference label)
    {
        IList<IResourceLocation> locations = Addressables.LoadResourceLocationsAsync(label).WaitForCompletion();
        return locations[Random.Range(0, locations.Count)];
    }



}
