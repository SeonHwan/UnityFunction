using Firebase.Storage;
using System;
using UnityEngine.AddressableAssets;

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
    public class FirebaseAddressablesManager
    {
        private static bool isFirebaseSetupFinished;

        /// <summary>
        /// Set this bool as soon as the app is ready to download from Firebase Storage. If you require authentication
        /// to access items on Firebase Storage you should set this after your User has logged in.
        /// The Addressables Pipeline will wait and 'load' until you set this to true.
        /// </summary>
        public static bool IsFirebaseSetupFinished
        {
            get => isFirebaseSetupFinished;
            set
            {
                if (isFirebaseSetupFinished != value)
                {
                    isFirebaseSetupFinished = value;
                    FireFirebaseSetupFinished();
                }
            }
        }

        public static event Action FirebaseSetupFinished;

        public static void Init(FirebaseStorage storage)
        {
            //캐시에 저장한 번들들을 모두 날리는 함수
            //Caching.ClearCache();

            //Addressables 에서 Firebase Remote bundle 지원을 위한 초기화
            Addressables.ResourceManager.ResourceProviders.Add(new FirebaseStorageAssetBundleProvider(storage));
            Addressables.ResourceManager.ResourceProviders.Add(new FirebaseStorageJsonAssetProvider(storage));
            Addressables.ResourceManager.ResourceProviders.Add(new FirebaseStorageHashProvider(storage));

            Addressables.InternalIdTransformFunc += FirebaseAddressablesCache.IdTransformFunc;

            IsFirebaseSetupFinished = true;
        }

        public static bool IsFirebaseStorageLocation(string internalId)
        {
            return internalId.StartsWith(FirebaseAddressablesConstants.GS_URL_START);

        }

        private static void FireFirebaseSetupFinished()
        {
            FirebaseSetupFinished?.Invoke();
        }
    }
}
