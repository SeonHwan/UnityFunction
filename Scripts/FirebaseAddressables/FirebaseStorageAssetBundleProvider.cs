using Firebase;
using Firebase.Extensions;
using Firebase.Storage;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;


namespace NakiedLib.FirebaseAddressables
{
    /// <summary>
    /// Original Code
    /// 
    /// https://gitlab.com/robinbird-studios/libraries/unity-plugins/firebase-tools
    /// 
    /// Released under the MIT license
    /// 
    /// </summary>
    [DisplayName("Firebase AssetBundle Provider")]
    public class FirebaseStorageAssetBundleProvider : AssetBundleProvider
    {
        FirebaseStorage mStorage = null;

        bool IsFirebaseStorageLocation(string internalId)
        {
            return internalId.StartsWith(FirebaseAddressablesConstants.GS_URL_START);
        }

        public FirebaseStorageAssetBundleProvider(FirebaseStorage storage)
        {
            mStorage = storage;
        }

        public readonly Dictionary<string, AsyncOperationHandle<IAssetBundleResource>> bundleOperationHandles = new Dictionary<string, AsyncOperationHandle<IAssetBundleResource>>();

        public override void Release(IResourceLocation location, object asset)
        {
            base.Release(location, asset);

            // We have to make sure that the actual Bundle Load operation for this asset also gets released together with the Firebase Resource
            if (bundleOperationHandles.TryGetValue(location.InternalId, out AsyncOperationHandle<IAssetBundleResource> operation))
            {
                if (operation.IsValid())
                {
                    Addressables.ResourceManager.Release(operation);
                }
                bundleOperationHandles.Remove(location.InternalId);
            }
        }

        public override void Provide(ProvideHandle provideHandle)
        {
            string path = provideHandle.ResourceManager.TransformInternalId(provideHandle.Location);
            if (IsFirebaseStorageLocation(path))
            {
                LoadResource(provideHandle);
            }
            else
            {
                base.Provide(provideHandle);
            }
        }

        private void LoadResource(ProvideHandle provideHandle)
        {
            string firebaseUrl = provideHandle.Location.InternalId;

            if(mStorage == null)
            {
                Debug.LogError("객체 생성시 FirebaseStorage 매개변수를 전달하세요");
                return;
            }

            mStorage.GetReferenceFromUrl(firebaseUrl).GetDownloadUrlAsync().ContinueWithOnMainThread((Task<Uri> task) =>
            {
                Debug.Log("storageFilePath=" + firebaseUrl);
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    Debug.Log($"Success Path : {firebaseUrl}");
                    Uri uri = task.Result;
                    string url = uri.ToString();

                    FirebaseAddressablesCache.SetInternalIdToStorageUrlMapping(firebaseUrl, url);
                    IResourceLocation[] dependencies;
                    IList<IResourceLocation> originalDependencies = provideHandle.Location.Dependencies;
                    if (originalDependencies != null)
                    {
                        dependencies = new IResourceLocation[originalDependencies.Count];
                        for (int i = 0; i < originalDependencies.Count; i++)
                        {
                            var dependency = originalDependencies[i];

                            dependencies[i] = dependency;
                        }
                    }
                    else
                    {
                        dependencies = new IResourceLocation[0];
                    }
                    var bundleLoc = new ResourceLocationBase(url, url, GetType().FullName,
                        typeof(IResourceLocator), dependencies)
                    {
                        Data = provideHandle.Location.Data,
                        PrimaryKey = provideHandle.Location.PrimaryKey
                    };

                    AsyncOperationHandle<IAssetBundleResource> asyncOperationHandle;
                    if (bundleOperationHandles.TryGetValue(firebaseUrl, out asyncOperationHandle))
                    {
                        // Release already running handler
                        if (asyncOperationHandle.IsValid())
                        {
                            provideHandle.ResourceManager.Release(asyncOperationHandle);
                        }
                    }
                    asyncOperationHandle = provideHandle.ResourceManager.ProvideResource<IAssetBundleResource>(bundleLoc);
                    bundleOperationHandles.Add(firebaseUrl, asyncOperationHandle);
                    asyncOperationHandle.Completed += handle =>
                    {
                        provideHandle.Complete(handle.Result, true, null);
                    };

                }
                else
                {
                    Debug.LogError($"Fail path : {firebaseUrl}");
                    if (task.Exception != null)
                        Debug.LogException(task.Exception);

                    provideHandle.Complete(this, false, new FirebaseException());
                }
            });
        }
    }
}
