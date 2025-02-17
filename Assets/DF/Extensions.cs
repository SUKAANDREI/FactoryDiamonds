using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DF
{
    public static class Extensions
    {
        public static T GetRandom<T>(this List<T> source)
        {
            if (source == null || source.Count == 0) return default;
            if (source.Count == 1) return source.First();
			
            var rand = UnityEngine.Random.Range(0, source.Count);
            return source[rand];
        }
        
        public static void SetActiveSafeSelf(this MonoBehaviour mb, bool state)
        {
            if (mb == null) return;
            mb.gameObject.SetActiveSafeSelf(state);
        }
        
        public static void SetActiveSafeSelf(this GameObject go, bool state)
        {
            if (go == null || go.activeSelf == state) return;
            go.SetActive(state);
        }
        
        public static T CorrectInstantiate<T>(this T prefab, Transform parent) where T : MonoBehaviour
        {
            if (prefab == null)
                return null;

            var instance = Object.Instantiate(prefab);
            if (instance == default(T)) return instance;

            instance.CorrectSetup(parent);
            return instance;
        }
        
        public static void CorrectSetup<T>(this T instance, Transform parent) where T : MonoBehaviour
        {
            if (parent != null) instance.transform.SetParent(parent);
		
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            instance.transform.localRotation = Quaternion.identity;
        }

        public static void CorrectDestroy<T>(this T instance) where T : MonoBehaviour
        {
            if (instance == null) return;
            instance.gameObject.CorrectDestroy();
        }
        
        public static void CorrectDestroy(this GameObject instance)
        {
            if (instance == null) return;
            if (instance.activeInHierarchy)
            {
                instance.transform.SetParent(null);
                instance.SetActive(false);
            }
            
            Object.Destroy(instance);
        }
    }
}