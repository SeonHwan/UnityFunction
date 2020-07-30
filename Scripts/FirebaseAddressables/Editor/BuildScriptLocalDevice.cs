using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;

using UnityEngine;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.ResourceManagement.ResourceProviders;

using NakiedLib;

namespace Nakied.FirebaseAddressables.Editor
{
    [CreateAssetMenu(fileName = "BuildScriptLocalDeviceMode.asset", menuName = "Addressable Assets/Data Builders/Local Device Build")]
    public class BuildScriptLocalDevice : BuildScriptPackedMode
    {
        public override string Name => "Local Device Build";

        public override void ClearCachedData()
        {
            base.ClearCachedData();

            string profileID = AddressableAssetSettingsDefaultObject.Settings.activeProfileId;

            string rPath = Utils.GetReflectionProfilePath(AddressableAssetSettingsDefaultObject.Settings.profileSettings, profileID, "LocalBuildPath");
            
            FileUtil.DeleteFileOrDirectory(rPath);

            //string addressablesPersistentPath = string.Format($"{Application.persistentDataPath}/com.unity.addressables");
            //FileUtil.DeleteFileOrDirectory(addressablesPersistentPath);

            AssetDatabase.Refresh();
        }
        
        protected override TResult DoBuild<TResult>(AddressablesDataBuilderInput builderInput, AddressableAssetsBuildContext aaContext)
        {
            foreach (var assetGroup in builderInput.AddressableSettings.groups)
            {
                var schema = assetGroup.GetSchema<BundledAssetGroupSchema>();
                if (schema == null)
                    continue;

                SerializedType _ty = schema.AssetBundleProviderType;
                if (_ty.Value != typeof(AssetBundleProvider))
                {
                    string _errMsg = $"Local Build 는 '{assetGroup.name}' Group의 AssetBundle Provider 를 AssetBundle Provider 로 바꿔줘야 합니다.";
                    return AddressableAssetBuildResult.CreateResult<TResult>(null, 0, _errMsg);
                }
            }

            //로컬빌드이므로 IsRemoteCatalog 값은 false
            builderInput.AddressableSettings.BuildRemoteCatalog = false;
            
            var result = base.DoBuild<TResult>(builderInput, aaContext);

            return result;
        }

        protected override string ProcessGroup(AddressableAssetGroup assetGroup, AddressableAssetsBuildContext aaContext)
        {
            var schema = assetGroup.GetSchema<BundledAssetGroupSchema>();
            if (schema == null)
                return string.Empty;
                
            schema.BuildPath.SetVariableByName(assetGroup.Settings, "LocalBuildPath");
            schema.LoadPath.SetVariableByName(assetGroup.Settings, "LocalLoadPath");

            Debug.Log($"Build Processing Asset Group : {assetGroup.name}");
            return base.ProcessGroup(assetGroup, aaContext);
        }
    }
}
