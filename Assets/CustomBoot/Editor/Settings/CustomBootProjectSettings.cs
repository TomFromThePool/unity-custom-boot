using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HalliHax.CustomBoot.Editor
{
    /// <summary>
    /// A scriptable object used to store references to CustomBoot settings for both Runtime and Editor.
    /// </summary>
    public class CustomBootProjectSettings : ScriptableObject
    {
        /// <summary>
        /// The Addressables reference for the runtime settings
        /// </summary>
        public AssetReference RuntimeSettings;
        
        /// <summary>
        /// The Addressables reference for the editor settings
        /// </summary>
        public AssetReference EditorSettings;
    }
}