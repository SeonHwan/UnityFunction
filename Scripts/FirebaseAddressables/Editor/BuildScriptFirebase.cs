using System;
using System.Reflection;
using System.IO;

using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;

using UnityEngine.ResourceManagement.Util;

using NakiedLib;
using NakiedLib.FirebaseAddressables;

namespace Nakied.FirebaseAddressables.Editor
{
    /// <summary>
    /// Original Code
    /// 
    /// https://gitlab.com/robinbird-studios/libraries/unity-plugins/firebase-tools
    /// 
    /// Released under the MIT license
    /// 
    /// </summary>

    // <summary>
    /// Modifies the settings so we use the Firebase Storage Proxy Loaders to get the Remote Data files
    /// </summary>
    [CreateAssetMenu(fileName = "BuildScriptFirebaseMode.asset", menuName = "Addressable Assets/Data Builders/Firebase Build")]
    public class BuildScriptFirebase : BuildScriptPackedMode
    {
        public override string Name => "FirebaseStorage Build";

        public override void ClearCachedData()
        {
            base.ClearCachedData();

            string profileID = AddressableAssetSettingsDefaultObject.Settings.activeProfileId;

            string rPath = Utils.GetReflectionProfilePath(AddressableAssetSettingsDefaultObject.Settings.profileSettings, profileID, "RemoteBuildPath");
            Debug.Log($"Convert Path : {rPath}");

            FileUtil.DeleteFileOrDirectory(rPath);

            AssetDatabase.Refresh();
        }

        protected override string ProcessGroup(AddressableAssetGroup assetGroup, AddressableAssetsBuildContext aaContext)
        {
            var schema = assetGroup.GetSchema<BundledAssetGroupSchema>();
            if (schema == null)
                return string.Empty;

            schema.BuildPath.SetVariableByName(assetGroup.Settings, "RemoteBuildPath");
            schema.LoadPath.SetVariableByName(assetGroup.Settings, "RemoteLoadPath");

            Debug.Log($"Build Processing Asset Group : {assetGroup.name}");
            return base.ProcessGroup(assetGroup, aaContext);
        }

        protected override TResult DoBuild<TResult>(AddressablesDataBuilderInput builderInput, AddressableAssetsBuildContext aaContext)
        {
            foreach (var assetGroup in builderInput.AddressableSettings.groups)
            {
                var schema = assetGroup.GetSchema<BundledAssetGroupSchema>();
                if (schema == null)
                    continue;

                SerializedType _ty = schema.AssetBundleProviderType;
                if (_ty.Value != typeof(FirebaseStorageAssetBundleProvider))
                {
                    string _errMsg = $"Firebase Build 는 '{assetGroup.name}' Group의 AssetBundleProvider 를 FirebaseStorageAssetBundleProvider 로 바꿔줘야 합니다.";
                    return AddressableAssetBuildResult.CreateResult<TResult>(null, 0, _errMsg);
                }
            }

            //원격빌드이므로 IsRemoteCatalog 값은 true
            builderInput.AddressableSettings.BuildRemoteCatalog = true;

            var result = base.DoBuild<TResult>(builderInput, aaContext);
            
            var settingsPath = Addressables.BuildPath + "/" + builderInput.RuntimeSettingsFilename;

            var data = JsonUtility.FromJson<ResourceManagerRuntimeData>(File.ReadAllText(settingsPath));

            var remoteHash = data.CatalogLocations.Find(locationData =>
                locationData.Keys[0] == "AddressablesMainContentCatalogRemoteHash");

            if (remoteHash != null)
            {
                var newRemoteHash = new ResourceLocationData(remoteHash.Keys, remoteHash.InternalId,
                    typeof(FirebaseStorageHashProvider), remoteHash.ResourceType, remoteHash.Dependencies);

                data.CatalogLocations.Remove(remoteHash);
                data.CatalogLocations.Add(newRemoteHash);
            }

            File.WriteAllText(settingsPath, JsonUtility.ToJson(data));

            Debug.Log($"Player Version : {builderInput.PlayerVersion}");
            
            return result;
        }
    }
}
