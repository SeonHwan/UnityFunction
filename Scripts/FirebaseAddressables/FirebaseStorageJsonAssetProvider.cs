using Firebase;
using Firebase.Extensions;
using Firebase.Storage;

using System;
using System.ComponentModel;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
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

    /// <summary>
    /// Loads JSON data from FirebaseStorage. Needed mainly for ContentCatalog downloads
    /// </summary>
    [DisplayName("Firebase Json Asset Provider")]
    public class FirebaseStorageJsonAssetProvider : JsonAssetProvider
    {
        private ProvideHandle provideHandle;

        private FirebaseStorage mStorage = null;

        /// <summary>
        /// Unfortunately we have to override this because the method CanProvide is only called once and when the InternalId
        /// changes this Provider is still selected for non-Firebase Json data. We just call into base when that happens
        /// </summary>
        public override string ProviderId => typeof(JsonAssetProvider).FullName;

        public FirebaseStorageJsonAssetProvider(FirebaseStorage storage)
        {
            mStorage = storage;
        }

        public override void Provide(ProvideHandle provideHandle)
        {
            var url = UnityEngine.AddressableAssets.Addressables.ResourceManager.TransformInternalId(provideHandle.Location);
            if (FirebaseAddressablesManager.IsFirebaseStorageLocation(url) == false)
            {
                base.Provide(provideHandle);
                return;
            }

            this.provideHandle = provideHandle;
            if (FirebaseAddressablesManager.IsFirebaseSetupFinished)
            {
                LoadManifest();
            }
            else
            {
                FirebaseAddressablesManager.FirebaseSetupFinished += LoadManifest;
            }
        }

        private void LoadManifest()
        {
            FirebaseAddressablesManager.FirebaseSetupFinished -= LoadManifest;
            var firebaseUrl = UnityEngine.AddressableAssets.Addressables.ResourceManager.TransformInternalId(provideHandle.Location);
            Debug.Log("Loading Json at: " + firebaseUrl);

            mStorage.GetReferenceFromUrl(firebaseUrl).GetDownloadUrlAsync().ContinueWithOnMainThread((Task<Uri> task) =>
            {
                Debug.Log("storageFilePath=" + firebaseUrl);
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    string url = task.Result.ToString();

                    Debug.Log("Got URL: " + url);

                    var catalogLoc = new ResourceLocationBase(url, url, typeof(JsonAssetProvider).FullName, typeof(IResourceLocator));

                    if (provideHandle.Location.ResourceType == typeof(ContentCatalogData))
                    {
                        provideHandle.ResourceManager.ProvideResource<ContentCatalogData>(catalogLoc).Completed += handle =>
                        {
                            provideHandle.Complete(handle.Result, true, null);
                        };
                    }
                    else
                    {
                        Debug.LogError("Could not convert type because function is private. Please add own type here");
                        // provideHandle.ResourceManager.ProvideResource(catalogLoc, provideHandle.Location.ResourceType).Completed += handle =>
                        // {
                        //     provideHandle.Complete(handle.Result, true, null);
                        // };
                        provideHandle.Complete<object>(null, false, null);
                    }
                }
                else
                {
                    provideHandle.Complete<object>(null, false, new FirebaseException());
                }
            });
        }
    }
}
