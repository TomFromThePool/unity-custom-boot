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
        /// Current initialisation status
        /// </summary>
        public static bool Initialised { get; private set; }

        /// <summary>
        // Called as soon as the game begins
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialise()
        {
            //We should always clean up after Addressables, so let's take care of that immediately
            Application.quitting += ApplicationOnUnloading;

            PerformInitialisation();
        }

        /// <summary>
        /// Initialise the bootstrapper
        /// </summary>
        public static void PerformInitialisation()
        {
            //In editor, perform initialisation synchronously
            if (Application.isEditor)
            {
                InitialiseBootSettingsSync();
            }
            else
            {
                //In builds, just run things asynchronously, since we can add any checks we need early on
                _ = InitialiseBootSettings();
            }
        }


        /// <summary>
        /// Called as the game is quitting, allowing for cleanup
        /// </summary>
        private static void ApplicationOnUnloading()
        {
            Application.quitting -= ApplicationOnUnloading;
            PerformDeInitialisation();
        }

        /// <summary>
        /// De-Initialise the bootstrapper
        /// </summary>
        public static void PerformDeInitialisation()
        {
            Cleanup(runtimeBootSettingsHandle);
            Cleanup(editorBootSettingsHandle);
            Initialised = false;
        }


        /// <summary>
        /// Initialise the boot settings asynchronously
        /// </summary>
        private static async Task InitialiseBootSettings()
        {
            await LoadCustomBootSettings();
            Initialised = true;
        }

        /// <summary>
        /// Initialise the boot settings synchronously
        /// </summary>
        private static void InitialiseBootSettingsSync()
        {
            LoadCustomBootSettingsSync();
            Initialised = true;
        }


        /// <summary>
        /// Clean up the boot settings
        /// </summary>
        /// <param name="handle"></param>
        private static void Cleanup(AsyncOperationHandle<CustomBootSettings> handle)
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
        /// Load the custom boot settings asynchronously and run the initialisation method
        /// </summary>
        private static async Task LoadCustomBootSettings()
        {
            if (Application.isEditor)
            {
                editorBootSettingsHandle = await InitialiseBootSettingsAsset(EditorAsset);
            }

            runtimeBootSettingsHandle = await InitialiseBootSettingsAsset(RuntimeAsset);
        }

        /// <summary>
        /// Load the custom boot settings synchronously and run the initialisation method
        /// </summary>
        private static void LoadCustomBootSettingsSync()
        {
            if (Application.isEditor)
            {
                editorBootSettingsHandle = InitialiseBootSettingsAssetSync(EditorAsset);
            }

            runtimeBootSettingsHandle = InitialiseBootSettingsAssetSync(RuntimeAsset);
        }

        /// <summary>
        /// Initialise the boot settings asset with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static async Task<AsyncOperationHandle<CustomBootSettings>> InitialiseBootSettingsAsset(string key)
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

        /// <summary>
        /// Initialise the boot settings asset with the given key synchronously
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static AsyncOperationHandle<CustomBootSettings> InitialiseBootSettingsAssetSync(string key)
        {
            var handle = Addressables.LoadAssetAsync<CustomBootSettings>(key);
            var result = handle.WaitForCompletion();
            result.InitialiseSync();
            return handle;
        }
    }
}