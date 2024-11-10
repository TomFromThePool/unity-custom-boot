using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomBoot.Editor
{
    /// <summary>
    /// Edit-mode utilities.
    ///
    /// This class provides a handy mechanism to enable editor bootstrapping.
    /// When enabled, the bootstrap system will initialise during edit-mode, allowing
    /// developers to preview the effects of the bootstrap system.
    ///
    /// There are several caveats here:
    /// 1. DontDestroyOnLoad doesn't work in edit-mode. Any objects loaded by the bootstrap system
    ///     will be added to the current scene.
    /// 2. Entering playmode will cause this script to de-initialise the bootstrapper, if initialised.
    ///     This means that playmode priority is always given to the boostrapper.
    /// 3. If editor bootstrapping is enabled, then the bootstrapper will de-init and re-init
    ///     whenever the current scene changes. This may not be appropriate for all workflows!
    /// 4. If bootstrapping is enabled, and the current scene is saved, then the bootstrapper will be
    ///     de-initialised prior to the scene being saved to disk, and then re-initialised, thereby
    ///     avoiding scene pollution.
    /// </summary>
    [InitializeOnLoad]
    public static class CustomBootEditorUtils
    {
        /// <summary>
        /// Editor prefs key for the edit-mode bootstrapper.
        /// </summary>
        private const string INITIALISE_IN_EDITOR = "bootstrap.editor_init_enabled";

        /// <summary>
        /// Menu path for the edit-mode bootstrapper
        /// </summary>
        private const string EDITOR_INIT_MENU = "Bootstrap/Editor Initialise";

        static CustomBootEditorUtils()
        {
            InitPlayModeListener();

            if (Application.isPlaying) return;
            
            //Don't initialise if we're about to change playmode!
            if (EditorInitialisationEnabled && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                DoInit();
            }
        }

        /// <summary>
        /// Initialise the playmode change listener.
        /// This also sets up a listener for scene saving, so that we can
        /// de-init and reinit the bootstrapper when the user saves changes, thereby
        /// avoiding bootstrapped objects polluting the saved scene.
        /// </summary>
        private static void InitPlayModeListener()
        {
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
            EditorSceneManager.sceneSaving += OnSceneSaving;
        }

        /// <summary>
        /// Handle the <see cref="EditorSceneManager.sceneSaving"/> event.
        /// This de-initialises the bootstrapper, removing objects from the scene,
        /// and then listens for the <see cref="EditorSceneManager.sceneSaved"/> event
        /// in order to re-initialise the bootstrapper.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="path"></param>
        private static void OnSceneSaving(Scene scene, string path)
        {
            if (HalliHax.CustomBoot.CustomBoot.Initialised)
            {
                DoDeInit();
                EditorSceneManager.sceneSaved += EditorSceneManagerOnsceneSaved;

                void EditorSceneManagerOnsceneSaved(Scene scene1)
                {
                    EditorSceneManager.sceneSaved -= EditorSceneManagerOnsceneSaved;
                    CheckInit();
                }
            }
        }

        /// <summary>
        /// Deinitialise the playmode change listener
        /// </summary>
        private static void DeInitPlayModeListener()
        {
            EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
        }

        /// <summary>
        /// Is play-mode bootstrapping enabled?
        /// </summary>
        private static bool EditorInitialisationEnabled
        {
            get => EditorPrefs.GetBool(INITIALISE_IN_EDITOR, false);
            set => EditorPrefs.SetBool(INITIALISE_IN_EDITOR, value);
        }


        /// <summary>
        /// Menu handler for the edit-mode bootstrapper
        /// </summary>
        [MenuItem(EDITOR_INIT_MENU)]
        private static void EditorInitialise()
        {
            EditorInitialisationEnabled = !EditorInitialisationEnabled;
            CheckInit();
        }

        /// <summary>
        /// Menu validator for the edit-mode bootstrapper
        /// </summary>
        /// <returns></returns>
        [MenuItem(EDITOR_INIT_MENU, true)]
        private static bool EditorInitialiseValidate()
        {
            Menu.SetChecked(EDITOR_INIT_MENU, EditorPrefs.GetBool(INITIALISE_IN_EDITOR, false));
            return true;
        }

        /// <summary>
        /// Perform initialisation or de-initialisation depending on the current context
        /// </summary>
        private static void CheckInit()
        {
            if (EditorInitialisationEnabled && !HalliHax.CustomBoot.CustomBoot.Initialised && !Application.isPlaying)
            {
                DoInit();
            }
            else if (HalliHax.CustomBoot.CustomBoot.Initialised)
            {
                DoDeInit();
            }
        }

        /// <summary>
        /// Perform the initialisation process.
        /// This adds a listener for the <see cref="EditorSceneManager.sceneClosing"/> event so that we
        /// can handle scene changes once we're initialised
        /// </summary>
        private static void DoInit()
        {
            EditorSceneManager.sceneClosing += OnSceneClosing;
            HalliHax.CustomBoot.CustomBoot.PerformInitialisation();
        }

        /// <summary>
        /// Handle the <see cref="EditorSceneManager.sceneClosing"/> event
        /// This de-initialises the bootstrapper and then sets up a listener for the
        /// <see cref="EditorSceneManager.activeSceneChangedInEditMode"/> event so that
        /// we can safely re-initialise the bootstrapper
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="removingscene"></param>
        private static void OnSceneClosing(Scene scene, bool removingscene)
        {
            if (!Application.isPlaying && HalliHax.CustomBoot.CustomBoot.Initialised)
            {
                DoDeInit();
                EditorSceneManager.activeSceneChangedInEditMode += OnSceneLoaded;
            }
        }

        /// <summary>
        /// Handles the <see cref="EditorSceneManager.activeSceneChangedInEditMode"/> event,
        /// allowing the boostrapper to reinitialise if required
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="scene"></param>
        private static void OnSceneLoaded(Scene arg0, Scene scene)
        {
            if (!Application.isPlaying)
            {
                EditorSceneManager.activeSceneChangedInEditMode -= OnSceneLoaded;
                CheckInit();
            }
        }

        /// <summary>
        /// De-Initialises the bootstrapper
        /// </summary>
        private static void DoDeInit()
        {
            HalliHax.CustomBoot.CustomBoot.PerformDeInitialisation();
            EditorSceneManager.sceneClosing -= OnSceneClosing;
        }


        /// <summary>
        /// Handles playmode change events. This will de-initialise the bootstrapper
        /// when exiting edit-mode, and call <see cref="CheckInit"/> when entering edit-mode
        /// </summary>
        /// <param name="obj"></param>
        private static void EditorApplicationOnplayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.ExitingEditMode:
                    if (HalliHax.CustomBoot.CustomBoot.Initialised)
                    {
                        DoDeInit();
                    }
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    CheckInit();
                    break;
            }
        }
    }
}