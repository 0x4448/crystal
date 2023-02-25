// SPDX-License-Identifier: Apache-2.0
// https://github.com/0x4448/unity-samples/blob/main/LICENSE

using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using LoadSceneMode = UnityEngine.SceneManagement.LoadSceneMode;

namespace UnitySamples.Core
{
    /// <summary>
    /// A scene manager for addressable scenes.
    /// </summary>
    public static class SceneManager
    {
        /// <summary>
        /// Mapping of AssetGUID to SceneInstance.
        /// </summary>
        private static readonly Dictionary<string, SceneInstance> _sceneMap = new();

        /// <summary>
        /// Load an addressable scene.
        /// </summary>
        /// <param name="scene">The scene to load.</param>
        /// <param name="additive">Load the scene in additive mode.</param>
        /// <param name="scenesToUnload">The scenes to unload after the load operation.</param>
        public static void LoadScene(AssetReferenceScene scene, bool additive, params AssetReferenceScene[] scenesToUnload)
        {
            var mode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            var loadOperation = scene.LoadSceneAsync(mode);

            loadOperation.Completed += (AsyncOperationHandle<SceneInstance> handle) =>
            {
                _sceneMap.Add(scene.AssetGUID, handle.Result);

                foreach (var scene in scenesToUnload)
                {
                    UnloadScene(scene);
                }
            };
        }

        public static void UnloadScene(AssetReferenceScene scene)
        {
            _ = Addressables.UnloadSceneAsync(_sceneMap[scene.AssetGUID]);
        }
    }



    [System.Serializable]
    public sealed class AssetReferenceScene : AssetReference
    {
        public AssetReferenceScene(string guid) : base(guid) { }

        public override bool ValidateAsset(string path)
        {
            return path.EndsWith(".unity");
        }
    }
}
