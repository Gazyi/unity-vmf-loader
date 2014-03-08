using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using UnityVMFLoader.Nodes;

using Debug = UnityEngine.Debug;

namespace UnityVMFLoader.Tasks
{
	[DependsOnTask(typeof(CreateBrushObjectsTask))]
	public class ImportMaterialsTask : Task
	{
		public static string SourcePath
		{
			get
			{
				return Path.Combine(Path.Combine("Assets", Settings.AssetPath), Settings.MaterialsFolder);
			}
		}

		public static string DestinationPath
		{
			get
			{
				return Path.Combine(Path.Combine(Application.dataPath, Settings.DestinationAssetPath), Settings.DestinationMaterialsFolder);
			}
		}

		public static string AbsolutePathToRelative(string parent, string child)
		{
			return (new Uri(parent).MakeRelativeUri(new Uri(child))).ToString();
		}

		public override void Run()
		{
			var solids = Importer.GetTask<ImportBrushesTask>().Solids;

			// Get all unique materials used in the solids.

			var materials = solids.Select(solid => solid.Children.OfType<Side>().Select(side => side.Material)).SelectMany(x => x).Distinct().ToList();

			// Narrow it down to those that don't already exist in the assets.

			materials = materials.Where
			(
				material =>

				AssetDatabase.LoadAssetAtPath
				(
					AbsolutePathToRelative
					(
						Application.dataPath,
						Path.Combine(DestinationPath, material + ".tga")
					),

					typeof(Texture)
				)

				== null
			)
			.ToList();

			// Use vtf2tga to make them into assets.

			if (Settings.AssetPath == "")
			{
				Debug.LogWarning("No asset path specified in settings - skipping asset import.");

				base.Run();

				return;
			}

			foreach (var materialName in materials)
			{
				var materialFullPath = Path.Combine(SourcePath, materialName + ".vmt");

				if (!File.Exists(materialFullPath))
				{
					Debug.LogWarning(String.Format("Material \"{0}\" not found.", materialName));

					continue;
				}

				var materialFile = File.ReadAllText(Path.Combine(SourcePath, materialName + ".vmt"));

				// Grab the $basetexture and import that.

				var baseTextureRegex = new Regex("\"?\\$basetexture\"?\\s+\"?([^\"]*)\"?", RegexOptions.IgnoreCase);

				var match = baseTextureRegex.Match(materialFile);

				if (!match.Success)
				{
					Debug.LogWarning(String.Format("Can't find $basetexture in material \"{0}\".", materialName));

					continue;
				}

				var textureName = match.Groups[1].Value;

				var textureFullPath = Path.Combine(SourcePath, textureName + ".vtf");

				var destinationFullPath = Path.Combine(DestinationPath, textureName + ".tga");

				var directory = Path.GetDirectoryName(destinationFullPath);

				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				var process = Process.Start
				(
					new ProcessStartInfo
					{
						CreateNoWindow = true,
						UseShellExecute = false,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						FileName = Path.Combine(Application.dataPath, "vtf2tga.exe"),
						WindowStyle = ProcessWindowStyle.Hidden,
						Arguments = String.Format("-i \"{0}\" -o \"{1}\"", textureFullPath, destinationFullPath)
					}
				);

				while (!process.StandardError.EndOfStream)
				{
					Debug.LogWarning(process.StandardError.ReadLine());
				}

				while (!process.HasExited) {}

				AssetDatabase.Refresh();

				EventHandler createMaterialOnFinished;

				createMaterialOnFinished = (caller, e) =>
				{
					UnityThreadHelper.Dispatcher.Dispatch
					(
						() =>
						{
							Importer.OnFinished -= createMaterialOnFinished;

							// Create the material.

							var material = new Material(Shader.Find("Diffuse"));

							var texturePath = AbsolutePathToRelative(Application.dataPath, destinationFullPath);

							var texture = (Texture2D) AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));

							if (texture == null)
							{
								Debug.LogWarning(String.Format("Couldn't find texture \"{0}\".", texturePath));
							}
							else
							{
								material.mainTexture = texture;

								AssetDatabase.CreateAsset(material, texturePath.Replace(".tga", ".mat"));
							}
						}
					);
				};

				Importer.OnFinished += createMaterialOnFinished;
			}

			var gameObjects = Importer.GetTask<CreateBrushObjectsTask>().GameObjects;

			EventHandler assignMaterialsOnFinished;

			assignMaterialsOnFinished = (caller, e) =>
			{
				UnityThreadHelper.Dispatcher.Dispatch
				(
					() =>
					{
						Importer.OnFinished -= assignMaterialsOnFinished;

						// Assign the material.

						foreach (var keyvalue in gameObjects)
						{
							Solid solid = keyvalue.Key;
							GameObject gameObject = keyvalue.Value;

							if (gameObject == null)
							{
								Debug.LogWarning("Tried to assign a material to a solid with a null GameObject!");

								continue;
							}

							var meshFilter = gameObject.GetComponent<MeshFilter>();
							var meshRenderer = gameObject.GetComponent<UnityEngine.MeshRenderer>();

							var mesh = meshFilter.sharedMesh;

							var meshMaterials = new Material[mesh.subMeshCount];

							var i = 0;

							foreach (var side in solid.Children.OfType<Side>())
							{
								var path = ImportMaterialsTask.AbsolutePathToRelative
								(
									Application.dataPath,
									Path.Combine(ImportMaterialsTask.DestinationPath, side.Material + ".mat")
								);

								var sideMaterial = (Material) AssetDatabase.LoadAssetAtPath(path, typeof(Material));

								if (sideMaterial == null)
								{
									Debug.LogWarning("Can't find material \"" + path + "\" for a side.");
								}

								meshMaterials[i++] = sideMaterial;
							}

							meshRenderer.sharedMaterials = meshMaterials;
						}
					}
				);
			};

			Importer.OnFinished += assignMaterialsOnFinished;

			base.Run();
		}
	}
}
