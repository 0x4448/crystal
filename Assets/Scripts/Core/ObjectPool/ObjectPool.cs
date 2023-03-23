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

        private readonly Dictionary<Type, UnityEngine.Pool.ObjectPool<PooledObject>> _pools = new();

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

        public GameObject Get<T>(Vector3 position, Quaternion rotation) where T : PooledObject
        {
            var pooledObject = _pools[typeof(T)].Get();
            pooledObject.transform.SetPositionAndRotation(position, rotation);
            return pooledObject.gameObject;
        }

        public GameObject Get<T>() where T : PooledObject
        {
            return Get<T>(Vector3.zero, Quaternion.identity);
        }

        public void Return(PooledObject pooledObject)
        {
            var type = pooledObject.GetType();
            _pools[type].Release(pooledObject);
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
            var type = config.Prefab.GetComponent<PooledObject>().GetType();
            _pools.Add(type, new(Create, Get, Return, Delete, true, config.PrewarmCount, config.MaxPoolSize));
        }

        private void CreateObjects(PrefabConfig config)
        {
            var pooledObjects = new List<PooledObject>();
            var type = config.Prefab.GetComponent<PooledObject>().GetType();

            for (var i = 0; i < config.PrewarmCount; i++)
            {
                var pooledObject = _pools[type].Get();
                pooledObjects.Add(pooledObject);
            }
            foreach (var pooledObject in pooledObjects)
            {
                _pools[type].Release(pooledObject);
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
