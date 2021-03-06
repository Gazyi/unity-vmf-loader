using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace UnityVMFLoader
{
	class Settings : EditorWindow
	{
		public static bool ImportDisplacements
		{
			get { return EditorPrefs.GetBool("UnityVMFLoader.ImportDisplacements"); }
			set { EditorPrefs.SetBool("UnityVMFLoader.ImportDisplacements", value); }
		}

		public static bool ImportBrushes
		{
			get { return EditorPrefs.GetBool("UnityVMFLoader.ImportBrushes", true); }
			set { EditorPrefs.SetBool("UnityVMFLoader.ImportBrushes", value); }
		}

		public static bool ImportWorldBrushes
		{
			get { return EditorPrefs.GetBool("UnityVMFLoader.ImportWorldBrushes", true); }
			set { EditorPrefs.SetBool("UnityVMFLoader.ImportWorldBrushes", value); }
		}

		public static bool ImportDetailBrushes
		{
			get { return EditorPrefs.GetBool("UnityVMFLoader.ImportDetailBrushes", true); }
			set { EditorPrefs.SetBool("UnityVMFLoader.ImportDetailBrushes", value); }
		}

		public static bool GenerateLightmapUVs
		{
			get { return EditorPrefs.GetBool("UnityVMFLoader.GenerateLightmapUVs", true); }
			set { EditorPrefs.SetBool("UnityVMFLoader.GenerateLightmapUVs", value); }
		}

		public static bool ImportPointEntities
		{
			get { return EditorPrefs.GetBool("UnityVMFLoader.ImportPointEntities", true); }
			set { EditorPrefs.SetBool("UnityVMFLoader.ImportPointEntities", value); }
		}

		public static bool ImportLights
		{
			get { return EditorPrefs.GetBool("UnityVMFLoader.ImportLights", true); }
			set { EditorPrefs.SetBool("UnityVMFLoader.ImportLights", value); }
		}

		public static float LightBrightnessScalar
		{
			get { return EditorPrefs.GetFloat("UnityVMFLoader.LightBrightnessScalar", 0.005f); }
			set { EditorPrefs.SetFloat("UnityVMFLoader.LightBrightnessScalar", value); }
		}

		public static bool ImportProps
		{
			get { return EditorPrefs.GetBool("UnityVMFLoader.ImportProps", true); }
			set { EditorPrefs.SetBool("UnityVMFLoader.ImportProps", value); }
		}

		public static bool ImportAssets
		{
			get { return EditorPrefs.GetBool("UnityVMFLoader.ImportAssets", true); }
			set { EditorPrefs.SetBool("UnityVMFLoader.ImportAssets", value); }
		}

		public static bool ImportMaterials
		{
			get { return EditorPrefs.GetBool("UnityVMFLoader.ImportMaterials", true); }
			set { EditorPrefs.SetBool("UnityVMFLoader.ImportMaterials", value); }
		}

		public static bool ImportModels
		{
			get { return EditorPrefs.GetBool("UnityVMFLoader.ImportModels"); }
			set { EditorPrefs.SetBool("UnityVMFLoader.ImportModels", value); }
		}

		public static bool ImportSounds
		{
			get { return EditorPrefs.GetBool("UnityVMFLoader.ImportSounds"); }
			set { EditorPrefs.SetBool("UnityVMFLoader.ImportSounds", value); }
		}

		public static string AssetPath
		{
			get { return EditorPrefs.GetString("UnityVMFLoader.AssetPath"); }
			set { EditorPrefs.SetString("UnityVMFLoader.AssetPath", value); }
		}

		public static string DestinationAssetPath
		{
			get { return EditorPrefs.GetString("UnityVMFLoader.DestinationAssetPath"); }
			set { EditorPrefs.SetString("UnityVMFLoader.DestinationAssetPath", value); }
		}

		public static string MaterialsFolder
		{
			get { return EditorPrefs.GetString("UnityVMFLoader.MaterialsFolder", "materials"); }
			set { EditorPrefs.SetString("UnityVMFLoader.MaterialsFolder", value); }
		}

		public static string DestinationMaterialsFolder
		{
			get { return EditorPrefs.GetString("UnityVMFLoader.DestinationMaterialsFolder", "Materials"); }
			set { EditorPrefs.SetString("UnityVMFLoader.DestinationMaterialsFolder", value); }
		}

		public static string ModelsFolder
		{
			get { return EditorPrefs.GetString("UnityVMFLoader.ModelsFolder", "models"); }
			set { EditorPrefs.SetString("UnityVMFLoader.ModelsFolder", value); }
		}

		public static string DestinationModelsFolder
		{
			get { return EditorPrefs.GetString("UnityVMFLoader.DestinationModelsFolder", "Models"); }
			set { EditorPrefs.SetString("UnityVMFLoader.DestinationModelsFolder", value); }
		}

		public static string SoundsFolder
		{
			get { return EditorPrefs.GetString("UnityVMFLoader.SoundsFolder", "sound"); }
			set { EditorPrefs.SetString("UnityVMFLoader.SoundsFolder", value); }
		}

		public static string DestinationSoundsFolder
		{
			get { return EditorPrefs.GetString("UnityVMFLoader.DestinationSoundsFolder", "Sounds"); }
			set { EditorPrefs.SetString("UnityVMFLoader.DestinationSoundsFolder", value); }
		}

		public void OnGUI()
		{
			title = "Unity VMF Loader";

			var size = new Vector2(275, 600);

			minSize = size;
			maxSize = size;

			// General.

			GUILayout.Label("General", EditorStyles.boldLabel);

			GUI.enabled = false;

			ImportDisplacements = EditorGUILayout.Toggle("Import displacements", ImportDisplacements);

			GUI.enabled = true;

			EditorGUILayout.Space();

			// Brushes.

			ImportBrushes = EditorGUILayout.BeginToggleGroup("Import brushes", ImportBrushes);

			ImportWorldBrushes = EditorGUILayout.Toggle("Import world brushes", ImportWorldBrushes);
			ImportDetailBrushes = EditorGUILayout.Toggle("Import detail brushes", ImportDetailBrushes);
			GenerateLightmapUVs = EditorGUILayout.Toggle("Generate lightmap UVs", GenerateLightmapUVs);

			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.Space();

			// Entities.

			ImportPointEntities = EditorGUILayout.BeginToggleGroup("Import point entities", ImportPointEntities);

			ImportProps = EditorGUILayout.Toggle("Import props", ImportProps);

			EditorGUILayout.Space();

			ImportLights = EditorGUILayout.BeginToggleGroup("Import lights", ImportLights);

			LightBrightnessScalar = EditorGUILayout.Slider("Brightness scalar", LightBrightnessScalar, 0, 0.02f);

			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.Space();

			// Assets.

			ImportAssets = EditorGUILayout.BeginToggleGroup("Import assets", ImportAssets);

			GUILayout.Label("Importing assets will import any missing assets that the map uses. Existing assets won't be reimported unless the destination paths are set up incorrectly.", EditorStyles.wordWrappedLabel);

			EditorGUILayout.Space();

			ImportMaterials = EditorGUILayout.Toggle("Import materials", ImportMaterials);

			var old = GUI.enabled;
			GUI.enabled = false;

			ImportModels = EditorGUILayout.Toggle("Import models", ImportModels);
			ImportSounds = EditorGUILayout.Toggle("Import sounds", ImportSounds);

			GUI.enabled = old;

			EditorGUILayout.Space();

			GUILayout.Label("Source paths:", EditorStyles.boldLabel);

			GUILayout.Label("The root path should point to the same folder that GameInfo.txt is in.", EditorStyles.wordWrappedLabel);

			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();

			AssetPath = EditorGUILayout.TextField("Root", AssetPath);

			if (GUILayout.Button("Browse"))
			{
				var path = EditorUtility.OpenFolderPanel("Asset path", "", "");

				if (path.Length > 0)
				{
					AssetPath = path;
				}
			}

			GUILayout.EndHorizontal();

			MaterialsFolder = EditorGUILayout.TextField("Materials", MaterialsFolder);

			old = GUI.enabled;
			GUI.enabled = false;

			ModelsFolder = EditorGUILayout.TextField("Models", ModelsFolder);
			SoundsFolder = EditorGUILayout.TextField("Sounds", SoundsFolder);

			GUI.enabled = old;

			EditorGUILayout.Space();

			GUILayout.Label("Destination paths:", EditorStyles.boldLabel);

			GUILayout.Label("The root path is relative to the assets folder. All the other paths are relative to the root path. Can be left empty to use the assets folder as root.", EditorStyles.wordWrappedLabel);

			EditorGUILayout.Space();

			DestinationAssetPath = EditorGUILayout.TextField("Root", DestinationAssetPath);

			DestinationMaterialsFolder = EditorGUILayout.TextField("Materials", DestinationMaterialsFolder);

			old = GUI.enabled;
			GUI.enabled = false;

			DestinationModelsFolder = EditorGUILayout.TextField("Models", DestinationModelsFolder);
			DestinationSoundsFolder = EditorGUILayout.TextField("Sounds", DestinationSoundsFolder);

			GUI.enabled = old;

			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.Space();
		}

		[MenuItem("Window/Unity VMF Loader")]

		static public void ShowSettings()
		{
			EditorWindow.GetWindow<Settings>();
		}
	}
}
