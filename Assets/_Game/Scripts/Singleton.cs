using UnityEngine;
using System;

namespace Moball {
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> {
        static T s_instance;
        static bool s_isInitializing = false;

        protected abstract void Initialize();

        public static bool TryGetInstance(out T instance) {
            if (s_isInitializing) {
                throw new InvalidOperationException(
                    "Singleton initialization dependency cycle detected"
                );
            }
            if (!s_instance) {
                var found = FindObjectOfType<T>();
                if (!found) {
                    instance = null;
                    return false;
                }
                InitializeAndAssign(found);
            }
            instance = s_instance;
            return true;
        }

        public static T Instance {
            get {
                if (TryGetInstance(out var instance)) {
                    return instance;
                }
                throw new InvalidOperationException($"Could not find instance of {typeof(T).Name} in scene");
            }
        }

        void Awake() {
            if (!s_instance) {
                InitializeAndAssign((T) this);
            } else if (s_instance != this) {
                throw new InvalidOperationException(
                    $"Tried to create two instances of singleton {typeof(T).Name}"
                );
            }
        }

        protected virtual void OnDestroy() {
            s_instance = null;
        }

        static void InitializeAndAssign(T instance) {
            if (s_instance) {
                throw new InvalidOperationException("Singleton already exists");
            }
            s_isInitializing = true;
            try {
                instance.Initialize();
            } catch (Exception e) {
                Debug.LogError(
                    $"Initialization of {instance.GetType().Name} failed with error: {e}",
                    instance
                );
            }
            s_isInitializing = false;
            s_instance = instance;
        }
    }
}