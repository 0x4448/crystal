// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace UnitySamples.Core
{
    public class Startup : MonoBehaviour
    {
        [SerializeField] private GameObject[] _managers;
        [SerializeField] private AssetReferenceScene _nextScene;

        private void Awake()
        {
            foreach (var manager in _managers)
            {
                var gameObject = Instantiate(manager);
                gameObject.name = manager.name;
            }

            SceneManager.LoadScene(_nextScene, additive: false);
        }
    }
}
