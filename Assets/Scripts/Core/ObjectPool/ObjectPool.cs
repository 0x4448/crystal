// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySamples.Core
{
    [DefaultExecutionOrder(-500)]
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private PrefabConfig[] _prefabs;

        public static ObjectPool Singleton;

        private readonly Dictionary<GameObject, UnityEngine.Pool.ObjectPool<PooledObject>> _pools = new();

        private void Awake()
        {
            if (Singleton)
            {
                Debug.LogWarning("An object pool already exists.", Singleton.gameObject);
                Destroy(gameObject);
            }
            else
            {
                Singleton = this;
            }

            foreach (var config in _prefabs)
            {
                CreatePool(config);
                CreateObjects(config);
            }
        }

        private void OnDestroy()
        {
            Singleton = null;
        }


        public GameObject Get(GameObject prefab)
        {
            return Get(prefab, Vector3.zero, Quaternion.identity);
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var pooledObject = _pools[prefab].Get();
            pooledObject.transform.SetPositionAndRotation(position, rotation);
            return pooledObject.gameObject;
        }

        public void Return(GameObject prefab, PooledObject pooledObject)
        {
            _pools[prefab].Release(pooledObject);
        }


        private void CreatePool(PrefabConfig config)
        {
            // Each prefab has a separate container to avoid polluting the scene with many objects.
            var container = new GameObject($"{config.Prefab.name} Pool");

            // Define the four functions for Unity's ObjectPool constructor.
            PooledObject Create()
            {
                var go = Instantiate(config.Prefab, container.transform);
                go.name = $"{config.Prefab.name}({go.GetInstanceID()})";
                var pooledObject = go.GetComponent<PooledObject>();
                pooledObject.Prefab = config.Prefab;
                go.SetActive(false);
                return pooledObject;
            };

            void Get(PooledObject obj)
            {
                obj.gameObject.SetActive(true);
            }

            void Return(PooledObject obj)
            {
                obj.gameObject.SetActive(false);
            }

            void Delete(PooledObject obj)
            {
                Destroy(obj.gameObject);
            }

            // Create the ObjectPool and add it to our pools.
            _pools.Add(config.Prefab, new(Create, Get, Return, Delete, true, config.PrewarmCount, config.MaxPoolSize));
        }

        private void CreateObjects(PrefabConfig config)
        {
            var pooledObjects = new List<PooledObject>();
            for (var i = 0; i < config.PrewarmCount; i++)
            {
                var pooledObject = _pools[config.Prefab].Get();
                pooledObjects.Add(pooledObject);
            }
            foreach (var pooledObject in pooledObjects)
            {
                _pools[config.Prefab].Release(pooledObject);
            }
        }


        private void OnValidate()
        {
            if (_prefabs == null)
            {
                return;
            }

            foreach (var config in _prefabs)
            {
                if (!config.Prefab)
                {
                    continue;
                }

                var name = config.Prefab.name;

                if (!config.Prefab.TryGetComponent<PooledObject>(out var _))
                {
                    Debug.LogWarning($"{name} does not have a PooledObject component.", config.Prefab);
                    config.Prefab = null;
                }

                if (config.PrewarmCount > config.MaxPoolSize)
                {
                    config.MaxPoolSize = config.PrewarmCount * 10;
                }

                config.name = $"{name}: {config.PrewarmCount}/{config.MaxPoolSize}";
            }
        }

        [Serializable]
        private class PrefabConfig
        {
            [HideInInspector] public string name;
            public GameObject Prefab;
            public int PrewarmCount;
            public int MaxPoolSize;
        }
    }
}
