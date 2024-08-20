using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HalliHax.CustomBoot
{
    /// <summary>
    /// Entrypoint for the Custom Boot initialisation
    /// </summary>
    public static class CustomBoot
    {
        /// <summary>
        /// Called as soon as the game begins
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static async void Initialise()
        {
            Application.quitting += ApplicationOnUnloading;
            await LoadCustomBootSettings();
        }

        /// <summary>
        /// Called as the game is quitting, allowing for cleanup
        /// </summary>
        private static void ApplicationOnUnloading()
        {
            Application.quitting -= ApplicationOnUnloading;
            Cleanup(runtimeBootSettingsHandle);
            Cleanup(editorBootSettingsHandle);
        }

        static void Cleanup(AsyncOperationHandle<CustomBootSettings> handle)
        {
            if (handle.IsValid())
            {
                handle.Result.Cleanup();
                Addressables.Release(handle);
            }
        }

        /// <summary>
        /// Async handle for the runtime custom boot settings scriptable object
        /// </summary>
        private static AsyncOperationHandle<CustomBootSettings> runtimeBootSettingsHandle;
        
        /// <summary>
        /// Async handle for the editor custom boot settings object
        /// </summary>
        private static AsyncOperationHandle<CustomBootSettings> editorBootSettingsHandle;
        
        /// <summary>
        /// Runtime addressable key
        /// </summary>
        private static string RuntimeAsset = $"{nameof(CustomBootSettings)}_Runtime";
        
        /// <summary>
        /// Editor addressable key
        /// </summary>
        private static string EditorAsset = $"{nameof(CustomBootSettings)}_Editor";

        /// <summary>
        /// Load the custom boot settings run the initialisation method
        /// </summary>
        static async Task LoadCustomBootSettings()
        {
            if (Application.isEditor)
            {
                editorBootSettingsHandle = await InitialiseBootSettingsAsset(EditorAsset);
                runtimeBootSettingsHandle = await InitialiseBootSettingsAsset(RuntimeAsset);
            }
            else
            {
                runtimeBootSettingsHandle = await InitialiseBootSettingsAsset(RuntimeAsset);
            }

            
        }

        /// <summary>
        /// Initialise the boot settings asset with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        static async Task<AsyncOperationHandle<CustomBootSettings>> InitialiseBootSettingsAsset(string key)
        {
            var handle = Addressables.LoadAssetAsync<CustomBootSettings>(key);
            await handle.Task;
            switch (handle.Status)
            {
                case AsyncOperationStatus.Failed:
                    Debug.LogError(handle.OperationException);
                    break;
                case AsyncOperationStatus.Succeeded:
                    await handle.Result.Initialise();
                    break;
            }

            return handle;
        }
    }
}