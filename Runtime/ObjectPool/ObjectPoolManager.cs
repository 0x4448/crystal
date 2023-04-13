// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UnitySamples.Core
{
    public class ObjectPoolManager : MonoBehaviour
    {
        [SerializeField] private PrefabConfig[] _prefabs;

        public static ObjectPoolManager Singleton { get; private set; }

        /// <summary>
        /// Mapping of prefabs to their pools.
        /// </summary>
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _pools = new();
        /// <summary>
        /// Mapping of instantiated objects to the pool they came from.
        /// </summary>
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _parents = new();

        private void Awake()
        {
            if (Singleton)
            {
                Debug.LogWarning("An object pool manager already exists.", Singleton.gameObject);
                Destroy(gameObject);
            }
            else
            {
                Singleton = this;
            }

            foreach (var config in _prefabs)
            {
                var pool = CreatePool(config);
                _pools.Add(config.Prefab, pool);

                // Fill the pool.
                var pooledObjects = new List<GameObject>();
                for (var i = 0; i < config.PrewarmCount; i++)
                {
                    var pooledObject = Get(config.Prefab);
                    pooledObjects.Add(pooledObject);
                }
                foreach (var pooledObject in pooledObjects)
                {
                    Return(pooledObject);
                }
            }
        }

        private void OnDestroy()
        {
            Singleton = null;
        }


        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var pool = _pools[prefab];
            var pooledObject = pool.Get();
            _ = _parents.TryAdd(pooledObject, pool);
            pooledObject.transform.SetPositionAndRotation(position, rotation);
            return pooledObject;
        }

        public GameObject Get(GameObject prefab)
        {
            return Get(prefab, Vector3.zero, Quaternion.identity);
        }

        public void Return(GameObject pooledObject)
        {
            if (_parents.TryGetValue(pooledObject, out var pool))
            {
                pool.Release(pooledObject);
            }
            else
            {
                Debug.LogWarning($"{pooledObject.name} was not created from a pool.", pooledObject);
                pooledObject.SetActive(false);
            }
        }


        private ObjectPool<GameObject> CreatePool(PrefabConfig config)
        {
            // Each prefab has a separate container to avoid polluting the scene with many objects.
            var container = new GameObject($"{config.Prefab.name} Pool");

            // Define the four functions for Unity's ObjectPool constructor.
            GameObject Create()
            {
                var go = Instantiate(config.Prefab, container.transform);
                go.name = $"{config.Prefab.name}({go.GetInstanceID()})";
                go.SetActive(false);
                return go;
            };

            void Get(GameObject obj)
            {
                obj.SetActive(true);
            }

            void Return(GameObject obj)
            {
                obj.SetActive(false);
            }

            void Delete(GameObject obj)
            {
                _ = _parents.Remove(obj);
                Destroy(obj);
            }

            return new(Create, Get, Return, Delete, true, config.PrewarmCount, config.MaxPoolSize);
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
