using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HalliHax.CustomBoot.Editor
{
    /// <summary>
    /// Helper methods for CustomBoot configuration
    /// </summary>
    public class CustomBootSettingsUtil
    {
        /// <summary>
        /// Path to the ProjectSettings file
        /// </summary>
        private const string PROJECT_SETTINGS_PATH = "ProjectSettings/CustomBoot.asset";

        /// <summary>
        /// Path to the runtime custom boot settings file
        /// </summary>
        private const string RUNTIME_CUSTOM_BOOT_SETTINGS_PATH =
            "Assets/CustomBoot/Settings/Runtime/CustomBootSettings_Runtime.asset";

        /// <summary>
        /// Path to the editor custom boot settings file
        /// </summary>
        private const string EDITOR_CUSTOM_BOOT_SETTINGS_PATH =
            "Assets/CustomBoot/Settings/Editor/CustomBootSettings_Editor.asset";

        /// <summary>
        /// Determine whether the settings asset file is available
        /// </summary>
        /// <returns></returns>
        internal static bool IsSettingsAvailable()
        {
            return File.Exists(PROJECT_SETTINGS_PATH);
        }


        /// <summary>
        /// Retrieve the settings object if it exists, otherwise create and return it.
        /// </summary>
        /// <returns></returns>
        internal static CustomBootProjectSettings GetOrCreateSettings()
        {
            CustomBootProjectSettings projectSettings;

            //Check whether the settings file already exists
            if (IsSettingsAvailable())
            {
                //If it exists, load it
                projectSettings = InternalEditorUtility.LoadSerializedFileAndForget(PROJECT_SETTINGS_PATH).First() as
                    CustomBootProjectSettings;
            }
            else
            {
                //If it doesn't exist, create a new ScriptableObject
                projectSettings = ScriptableObject.CreateInstance<CustomBootProjectSettings>();

                //Configure the settings file
                CreateBootSettingsAssets(out var runtimeEntry, out var editorEntry);
                projectSettings.RuntimeSettings = new AssetReference(runtimeEntry.guid);
                projectSettings.EditorSettings = new AssetReference(editorEntry.guid);

                //And save it!
                InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { projectSettings },
                    PROJECT_SETTINGS_PATH, true);
            }

            //Finally, return our settings object
            return projectSettings;
        }

        /// <summary>
        /// Create the Runtime and Editor CustomBootSettings assets.
        /// </summary>
        /// <param name="runtimeEntry"></param>
        /// <param name="editorEntry"></param>
        private static void CreateBootSettingsAssets(out AddressableAssetEntry runtimeEntry,
            out AddressableAssetEntry editorEntry)
        {
            //Create two assets representing our boot configurations
            var runtimeSettings =
                GetOrCreateBootSettingsAsset(RUNTIME_CUSTOM_BOOT_SETTINGS_PATH, out var runtimeCreated);
            var editorSettings = GetOrCreateBootSettingsAsset(EDITOR_CUSTOM_BOOT_SETTINGS_PATH, out var editorCreated);

            //Save the AssetDatabase state if either asset is new
            if (runtimeCreated || editorCreated)
            {
                AssetDatabase.SaveAssets();
            }

            //Configure the Addressables system with the new assets.
            AddSettingsToAddressables(runtimeSettings, editorSettings, out runtimeEntry, out editorEntry);
        }

        /// <summary>
        /// Load, or create, a CustomBootSettings asset at the given path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="wasCreated"></param>
        /// <returns></returns>
        private static CustomBootSettings GetOrCreateBootSettingsAsset(string path, out bool wasCreated)
        {
            var settings = AssetDatabase.LoadAssetAtPath<CustomBootSettings>(path);
            if (!settings)
            {
                //Make sure full path is created
                var dirPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                }

                settings = ScriptableObject.CreateInstance<CustomBootSettings>();
                AssetDatabase.CreateAsset(settings, path);
                wasCreated = true;
            }
            else
            {
                wasCreated = false;
            }

            return settings;
        }

        /// <summary>
        /// Add the CustomBootSettings asset to the relevant Addressables groups.
        /// </summary>
        /// <param name="runtimeSettings"></param>
        /// <param name="editorSettings"></param>
        /// <param name="runtimeEntry"></param>
        /// <param name="editorEntry"></param>
        private static void AddSettingsToAddressables(CustomBootSettings runtimeSettings,
            CustomBootSettings editorSettings, out AddressableAssetEntry runtimeEntry,
            out AddressableAssetEntry editorEntry)
        {
            InitialiseAddressableGroups(out var runtimeGroup, out var editorGroup);
            runtimeEntry =
                CreateCustomBootSettingsEntry(runtimeSettings, runtimeGroup, $"{nameof(CustomBootSettings)}_Runtime");
            editorEntry =
                CreateCustomBootSettingsEntry(editorSettings, editorGroup, $"{nameof(CustomBootSettings)}_Editor");
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Create an Addressables entry for the given CustomBootSettings object, and add it to the given group.
        /// </summary>
        /// <param name="bootSettings"></param>
        /// <param name="group"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static AddressableAssetEntry CreateCustomBootSettingsEntry(CustomBootSettings bootSettings,
            AddressableAssetGroup group, string key)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entry = settings.CreateOrMoveEntry(
                AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(bootSettings)),
                group);
            entry.address = key;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            return entry;
        }

        /// <summary>
        /// Ensure the Runtime and Editor Addressables groups exist
        /// </summary>
        /// <param name="runtimeGroup"></param>
        /// <param name="editorGroup"></param>
        private static void InitialiseAddressableGroups(out AddressableAssetGroup runtimeGroup,
            out AddressableAssetGroup editorGroup)
        {
            runtimeGroup = GetOrCreateGroup($"{nameof(CustomBoot)}_Runtime", true);
            editorGroup = GetOrCreateGroup($"{nameof(CustomBoot)}_Editor", false);
        }

        /// <summary>
        /// Retrieve or create an Addressables group.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="includeInBuild"></param>
        /// <returns></returns>
        private static AddressableAssetGroup GetOrCreateGroup(string name, bool includeInBuild)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var group = settings.FindGroup(name);
            if (group == null)
            {
                group = settings.CreateGroup(name, false, false, true, settings.DefaultGroup.Schemas);
                group.GetSchema<BundledAssetGroupSchema>().IncludeInBuild = includeInBuild;
            }

            return group;
        }

        /// <summary>
        /// Retrieve the serialised representation of the settings object
        /// </summary>
        /// <returns></returns>
        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}