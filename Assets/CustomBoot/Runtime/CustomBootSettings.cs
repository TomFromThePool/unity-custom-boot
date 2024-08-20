using System.Threading.Tasks;
using UnityEngine;

namespace HalliHax.CustomBoot
{
    /// <summary>
    /// Settings for the custom boot process
    /// </summary>
    public class CustomBootSettings : ScriptableObject
    {
        /// <summary>
        /// A list of prefabs which should be loaded during boot
        /// </summary>
        public GameObject[] BootPrefabs;

        /// <summary>
        /// Internal references to instances of the prefabs from <see cref="BootPrefabs"/>
        /// </summary>
        private GameObject[] Instances;
        
        
        /// <summary>
        /// Initialise the boot settings object, loading each prefab in <see cref="BootPrefabs"/>
        /// </summary>
        public async Task Initialise()
        {
            Instances = new GameObject[BootPrefabs.Length];
            for (var i = 0; i < BootPrefabs.Length; i++)
            {
                if (!BootPrefabs[i]) continue;
                
                var instance = GameObject.InstantiateAsync(BootPrefabs[i]);
                while (!instance.isDone)
                    await Task.Yield();

                Instances[i] = instance.Result[0];
            }
        }

        /// <summary>
        /// Destroy all loaded instances referenced by <see cref="Instances"/>
        /// </summary>
        public void Cleanup()
        {
            foreach (var t in Instances)
            {
                if (t)
                {
                    GameObject.Destroy(t);
                }
            }

            Instances = null;
        }
    }
}