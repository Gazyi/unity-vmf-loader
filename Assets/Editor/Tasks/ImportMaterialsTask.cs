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
				return Path.Combine(Settings.AssetPath, Settings.MaterialsFolder);
			}
		}

		public static string DestinationPath
		{
			get
			{
				return Path.Combine(Settings.DestinationAssetPath, Settings.DestinationMaterialsFolder);
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

			materials = materials.Where(x => AssetDatabase.LoadAssetAtPath(Path.Combine("Assets", Path.Combine(DestinationPath, x + ".mat")), typeof(Material)) == null).ToList();

			if (Settings.AssetPath == "")
			{
				Debug.LogWarning("No asset path specified in settings - skipping asset import.");

				base.Run();
				return;
			}

			foreach (var material in materials)
			{
				CreateTextures(material);
				CreateMaterials(material);
			}

			AssignMaterials();

			base.Run();
		}

		private string GetVMTParameter(string material, string parameter)
		{
			var baseTextureRegex = new Regex("\"?\\$" + parameter + "\"?\\s+\"?([^\"]*)\"?", RegexOptions.IgnoreCase);

			var match = baseTextureRegex.Match(File.ReadAllText(material));

			if (!match.Success)
			{
				return "";
			}

			return match.Groups[1].Value;
		}

		private void ImportTexture(string textureName)
		{
			var textureFullPath = Path.Combine(SourcePath, textureName + ".vtf");
			var textureDestinationFullPath = Path.Combine(Application.dataPath, Path.Combine(DestinationPath, textureName + ".tga"));
			var textureDestinationDirectory = Path.GetDirectoryName(textureDestinationFullPath);

			if (!Directory.Exists(textureDestinationDirectory))
			{
				Directory.CreateDirectory(textureDestinationDirectory);
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
					Arguments = String.Format("-i \"{0}\" -o \"{1}\"", textureFullPath, textureDestinationFullPath)
				}
			);

			while (!process.StandardError.EndOfStream)
			{
				Debug.LogWarning(process.StandardError.ReadLine());
			}

			while (!process.HasExited);

			AssetDatabase.Refresh();
		}

		private void CreateTextures(string materialName)
		{
			var vmtPath = Path.Combine(SourcePath, materialName + ".vmt");

			if (!File.Exists(vmtPath))
			{
				return;
			}

			var baseTextureParameter = GetVMTParameter(vmtPath, "basetexture");
			var bumpMapParameter = GetVMTParameter(vmtPath, "bumpmap");

			if (String.IsNullOrEmpty(baseTextureParameter))
			{
				Debug.LogWarning("Material \"" + vmtPath + "\" doesn't have a $basetexture.");

				return;
			}

			ImportTexture(baseTextureParameter);
			ImportTexture(bumpMapParameter);
		}

		private void CreateMaterials(string materialName)
		{
			var vmtPath = Path.Combine(SourcePath, materialName + ".vmt");

			if (!File.Exists(vmtPath))
			{
				return;
			}

			EventHandler createMaterialOnFinished;

			createMaterialOnFinished = (caller, e) =>
			{
				UnityThreadHelper.Dispatcher.Dispatch
				(
					() =>
					{
						Importer.OnFinished -= createMaterialOnFinished;

						var translucentParameter = GetVMTParameter(vmtPath, "(?:translucent|alphatest)");
						var baseTextureParameter = GetVMTParameter(vmtPath, "basetexture");
						var bumpMapParameter = GetVMTParameter(vmtPath, "bumpmap");

						if (String.IsNullOrEmpty(baseTextureParameter))
						{
							Debug.LogWarning("Material \"" + vmtPath + "\" doesn't have a $basetexture.");

							return;
						}

						var texturePath = Path.Combine("Assets", Path.Combine(DestinationPath, baseTextureParameter + ".tga"));
						var texture = (Texture2D) AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));

						if (texture == null)
						{
							Debug.LogWarning(String.Format("Couldn't find texture \"{0}\".", texturePath));

							return;
						}

						Texture2D normalMap = null;

						if (!String.IsNullOrEmpty(bumpMapParameter))
						{
							var normalMapPath = Path.Combine("Assets", Path.Combine(DestinationPath, bumpMapParameter + ".tga"));

							normalMap = (Texture2D) AssetDatabase.LoadAssetAtPath(normalMapPath, typeof(Texture2D));

							if (normalMap == null)
							{
								Debug.LogWarning(String.Format("Couldn't find normalMap map \"{0}\".", normalMapPath));
							}
						}

						var shader = "";

						if (!String.IsNullOrEmpty(translucentParameter) && translucentParameter[0] == '1')
						{
							shader += "Transparent/";
						}

						if (!String.IsNullOrEmpty(bumpMapParameter) && normalMap != null)
						{
							shader += "Bumped ";
						}

						shader += "Diffuse";

						var material = new Material(Shader.Find(shader));

						material.mainTexture = texture;

						if (!String.IsNullOrEmpty(bumpMapParameter) && normalMap != null)
						{
							material.SetTexture("_BumpMap", normalMap);
						}

						var materialPath = Path.Combine(DestinationPath, materialName + ".mat");

						var materialDirectory = Path.GetDirectoryName(Path.Combine(Application.dataPath, materialPath));

						if (!Directory.Exists(materialDirectory))
						{
							Directory.CreateDirectory(materialDirectory);
						}

						AssetDatabase.CreateAsset(material, Path.Combine("Assets", materialPath));
					}
				);
			};

			Importer.OnFinished += createMaterialOnFinished;
		}

		private void AssignMaterials()
		{
			var gameObjects = Importer.GetTask<CreateBrushObjectsTask>().GameObjects;

			EventHandler assignMaterialsOnFinished;

			assignMaterialsOnFinished = (caller, e) =>
			{
				UnityThreadHelper.Dispatcher.Dispatch
				(
					() =>
					{
						Importer.OnFinished -= assignMaterialsOnFinished;

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
							var textureCoordinates = new Vector2[mesh.vertices.Length];

							var submesh = 0;

							foreach (var side in solid.Children.OfType<Side>())
							{
								// Assign the material.

								var path = Path.Combine("Assets", Path.Combine(DestinationPath, side.Material + ".mat"));

								var sideMaterial = (Material) AssetDatabase.LoadAssetAtPath(path, typeof(Material));

								if (sideMaterial == null)
								{
									Debug.LogWarning("Can't find material \"" + path + "\" for a side.");

									continue;
								}

								meshMaterials[submesh] = sideMaterial;

								// Minimize shift values.

								var texture = sideMaterial.mainTexture;

								side.UAxisTranslation = side.UAxisTranslation % texture.width;
								side.VAxisTranslation = side.VAxisTranslation % texture.height;

								if (side.UAxisTranslation < -texture.width / 2f)
								{
									side.UAxisTranslation += texture.width;
								}

								if (side.VAxisTranslation < -texture.height / 2f)
								{
									side.VAxisTranslation += texture.height;
								}

								// Calculate texture coordinates.

								foreach (var index in mesh.GetIndices(submesh))
								{
									var vertex = gameObject.transform.position + mesh.vertices[index];

									var u = Vector3.Dot(vertex, side.UAxis) / (texture.width * side.UAxisScale) + side.UAxisTranslation / texture.width;
									var v = Vector3.Dot(vertex, side.VAxis) / (texture.height * side.VAxisScale) + side.VAxisTranslation / texture.height;

									textureCoordinates[index] = new Vector2(u, -v);
								}

								submesh++;
							}

							meshRenderer.sharedMaterials = meshMaterials;
							mesh.uv = textureCoordinates;

							CalculateMeshTangents(mesh);
						}
					}
				);
			};

			Importer.OnFinished += assignMaterialsOnFinished;
		}

		// http://answers.unity3d.com/questions/7789/calculating-tangents-vector4.html

		private static void CalculateMeshTangents(Mesh mesh)
		{
			int[] triangles = mesh.triangles;
			Vector3[] vertices = mesh.vertices;
			Vector2[] uv = mesh.uv;
			Vector3[] normals = mesh.normals;

			int triangleCount = triangles.Length;
			int vertexCount = vertices.Length;

			Vector3[] tan1 = new Vector3[vertexCount];
			Vector3[] tan2 = new Vector3[vertexCount];

			Vector4[] tangents = new Vector4[vertexCount];

			for (long a = 0; a < triangleCount; a += 3)
			{
				long i1 = triangles[a + 0];
				long i2 = triangles[a + 1];
				long i3 = triangles[a + 2];

				Vector3 v1 = vertices[i1];
				Vector3 v2 = vertices[i2];
				Vector3 v3 = vertices[i3];

				Vector2 w1 = uv[i1];
				Vector2 w2 = uv[i2];
				Vector2 w3 = uv[i3];

				float x1 = v2.x - v1.x;
				float x2 = v3.x - v1.x;
				float y1 = v2.y - v1.y;
				float y2 = v3.y - v1.y;
				float z1 = v2.z - v1.z;
				float z2 = v3.z - v1.z;

				float s1 = w2.x - w1.x;
				float s2 = w3.x - w1.x;
				float t1 = w2.y - w1.y;
				float t2 = w3.y - w1.y;

				float r = 1.0f / (s1 * t2 - s2 * t1);

				Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
				Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

				tan1[i1] += sdir;
				tan1[i2] += sdir;
				tan1[i3] += sdir;

				tan2[i1] += tdir;
				tan2[i2] += tdir;
				tan2[i3] += tdir;
			}


			for (long a = 0; a < vertexCount; ++a)
			{
				Vector3 n = normals[a];
				Vector3 t = tan1[a];

				Vector3.OrthoNormalize(ref n, ref t);
				tangents[a].x = t.x;
				tangents[a].y = t.y;
				tangents[a].z = t.z;

				tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
			}

			mesh.tangents = tangents;
		}
	}
}
