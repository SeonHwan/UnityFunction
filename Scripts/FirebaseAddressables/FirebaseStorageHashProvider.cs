using Firebase;
using Firebase.Extensions;
using Firebase.Storage;

using System;
using System.ComponentModel;
using System.Threading.Tasks;

using UnityEngine;
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
    /// Downloads Hash of the ContentCatalog from FirebaseStorage
    /// </summary>
    [DisplayName("Firebase Hash Provider")]
    public class FirebaseStorageHashProvider : ResourceProviderBase
    {
        private ProvideHandle provideHandle;

        private FirebaseStorage mStorage = null;

        public FirebaseStorageHashProvider(FirebaseStorage storage)
        {
            mStorage = storage;
        }

        public override void Provide(ProvideHandle provideHandle)
        {
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

            string firebaseUrl = provideHandle.Location.InternalId;

            Debug.Log($"Loading manifest: {firebaseUrl}");

            if (mStorage == null)
            {
                Debug.LogError("객체 생성시 FirebaseStorage 매개변수를 전달하세요");
                return;
            }

            mStorage.GetReferenceFromUrl(firebaseUrl).GetDownloadUrlAsync().ContinueWithOnMainThread((Task<Uri> task) =>
            {
                Debug.Log("storageFilePath=" + firebaseUrl);

                if (!task.IsFaulted && !task.IsCanceled)
                {
                    string url = task.Result.ToString();
                    Debug.Log($"Loading via URL: {url}");

                    var catalogLoc =
                        new ResourceLocationBase(url, url, typeof(TextDataProvider).FullName, typeof(string));

                    provideHandle.ResourceManager.ProvideResource<string>(catalogLoc).Completed += handle =>
                    {
                        Debug.Log($"Got hash for catalog: {handle.Result}");
                        provideHandle.Complete(handle.Result, true, null);
                    };
                }
                else
                {
                    provideHandle.Complete((string)null, false, new FirebaseException());
                }
            });
        }
    }
}
