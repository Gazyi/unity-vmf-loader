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

			ImportLights = EditorGUILayout.Toggle("Import lights", ImportLights);

			LightBrightnessScalar = EditorGUILayout.Slider("Light brightness scalar", LightBrightnessScalar, 0, 0.02f);

			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.Space();

			// Assets.

			ImportAssets = EditorGUILayout.BeginToggleGroup("Import assets", ImportAssets);

			GUILayout.Label("Importing assets will import any missing assets that the map uses.", EditorStyles.wordWrappedLabel);

			EditorGUILayout.Space();

			ImportMaterials = EditorGUILayout.Toggle("Import materials", ImportMaterials);

			var old = GUI.enabled;
			GUI.enabled = false;

			ImportModels = EditorGUILayout.Toggle("Import models", ImportModels);
			ImportSounds = EditorGUILayout.Toggle("Import sounds", ImportSounds);

			GUI.enabled = old;

			EditorGUILayout.Space();

			GUILayout.Label("The asset path is the path to the root directory of the game. It will be used to look for the other paths.", EditorStyles.wordWrappedLabel);

			EditorGUILayout.Space();

			AssetPath = EditorGUILayout.TextField("Asset path", AssetPath);

			MaterialsFolder = EditorGUILayout.TextField("Materials folder", MaterialsFolder);

			old = GUI.enabled;
			GUI.enabled = false;

			ModelsFolder = EditorGUILayout.TextField("Models folder", ModelsFolder);
			SoundsFolder = EditorGUILayout.TextField("Sounds folder", SoundsFolder);

			GUI.enabled = old;

			EditorGUILayout.Space();

			GUILayout.Label("The destination asset path is relative to the Assets folder.", EditorStyles.wordWrappedLabel);

			EditorGUILayout.Space();

			DestinationAssetPath = EditorGUILayout.TextField("Destination asset path", DestinationAssetPath);

			DestinationMaterialsFolder = EditorGUILayout.TextField("Destination materials folder", DestinationMaterialsFolder);

			old = GUI.enabled;
			GUI.enabled = false;

			DestinationModelsFolder = EditorGUILayout.TextField("Destination models folder", DestinationModelsFolder);
			DestinationSoundsFolder = EditorGUILayout.TextField("Destination mounds folder", DestinationSoundsFolder);

			GUI.enabled = old;

			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.Space();
		}

		[MenuItem("Unity VMF Loader/Settings")]

		static public void ShowSettings()
		{
			EditorWindow.GetWindow<Settings>();
		}
	}
}
