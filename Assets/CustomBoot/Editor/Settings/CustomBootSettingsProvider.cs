using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace HalliHax.CustomBoot.Editor
{
    /// <summary>
    /// Settings provider for the custom boot behaviour
    /// </summary>
    public class CustomBootSettingsProvider : SettingsProvider
    {
        /// <summary>
        /// Internal reference to the serialized boot settings object
        /// </summary>
        private SerializedObject customBootSettings;

        
        private CustomBootSettingsProvider(string path, SettingsScope scopes = SettingsScope.Project, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        /// <summary>
        /// Initialise the UI for the settings provider
        /// </summary>
        /// <param name="searchContext"></param>
        /// <param name="rootElement"></param>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            customBootSettings = CustomBootSettingsUtil.GetSerializedSettings();
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CustomBoot/Editor/StyleSheets/CustomBootStyles.uss");
            rootElement.styleSheets.Add(styleSheet);
            rootElement.AddToClassList("settings");
            var title = new Label()
            {
                text = "Custom Boot"
            };
            title.AddToClassList("title");
            rootElement.Add(title);

            var properties = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Column
                }
            };
            properties.AddToClassList("property-list");

            var runtimeProp = customBootSettings.FindProperty(nameof(CustomBootProjectSettings.RuntimeSettings));
            var editorProp = customBootSettings.FindProperty(nameof(CustomBootProjectSettings.EditorSettings));
            
            properties.Add(CreateBootSettingsEditor(runtimeProp));
            properties.Add(CreateBootSettingsEditor(editorProp));
            rootElement.Add(properties);

            rootElement.Bind(customBootSettings);
        }

        /// <summary>
        /// Draw an editor for the CustomBootSettings object associated with the given property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static VisualElement CreateBootSettingsEditor(SerializedProperty property)
        {
            var bootSettingsObject = new SerializedObject(AssetDatabase.LoadAssetAtPath<CustomBootSettings>(AssetDatabase.GUIDToAssetPath((property.boxedValue as AssetReference).AssetGUID)));
            var propertyEditorContainer = new VisualElement();
            DrawObject(propertyEditorContainer, bootSettingsObject);
            return propertyEditorContainer;
        }

        /// <summary>
        /// Generic SerializedObject property editor
        /// </summary>
        /// <param name="container"></param>
        /// <param name="o"></param>
        private static void DrawObject(VisualElement container, SerializedObject o)
        {
            var l = new Label(o.targetObject.name);
            container.Add(l);
            var f = GetVisibleSerializedFields(o.targetObject.GetType());
            foreach (var field in f)
            {
                var prop = o.FindProperty(field.Name);
                var pField = new PropertyField(prop);
                container.Add(pField);
            }
            container.Bind(o);
        }
        
        /// <summary>
        /// Retrieve all accessible serialised fields for the given type
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        private static FieldInfo[] GetVisibleSerializedFields(Type T)
        {
            var publicFields = T.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var infoFields = publicFields.Where(t => t.GetCustomAttribute<HideInInspector>() == null).ToList();
            
            var privateFields = T.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            infoFields.AddRange(privateFields.Where(t => t.GetCustomAttribute<SerializeField>() != null));

            return infoFields.ToArray();
        }

        /// <summary>
        /// Create the Settings Provider. Internally, this will ensure the settings object is created.
        /// </summary>
        /// <returns></returns>
        [SettingsProvider]
        public static SettingsProvider CreateCustomBootSettingsProvider()
        {
            if (!CustomBootSettingsUtil.IsSettingsAvailable())
            {
                CustomBootSettingsUtil.GetOrCreateSettings();
            }
            
            if (CustomBootSettingsUtil.IsSettingsAvailable())
            {
                var provider = new CustomBootSettingsProvider("Project/Custom Boot", SettingsScope.Project);
                return provider;
            }

            return null;
        }
    }
}