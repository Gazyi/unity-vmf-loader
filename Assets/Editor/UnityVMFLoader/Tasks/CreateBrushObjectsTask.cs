using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityVMFLoader.Nodes;

namespace UnityVMFLoader.Tasks
{
	[DependsOnTask(typeof(ImportBrushesTask), typeof(GroupNodesTask))]
	public class CreateBrushObjectsTask : Task
	{
		public Dictionary<Solid, GameObject> GameObjects = new Dictionary<Solid, GameObject>();

		public override void Run()
		{
			var solids = Importer.GetTask<ImportBrushesTask>().Solids;
			var groups = Importer.GetTask<GroupNodesTask>().Groups;

			foreach (var solid in solids)
			{
				var gameObject = new GameObject("Solid " + solid.Identifier);

				GameObjects[solid] = gameObject;

				var mesh = (Mesh) solid;

				// The vertices of the mesh are in world coordinates so we'll need to center them ...

				var center = mesh.vertices.Average();

				var vertices = mesh.vertices;

				for (var vertex = 0; vertex < vertices.Count(); vertex++)
				{
					vertices[vertex] -= center;
				}

				mesh.vertices = vertices;

				mesh.RecalculateBounds();

				// ... and move the object itself to those world coordinates.

				gameObject.transform.position = center;

				// Add the required components and stuff.

				gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;

				gameObject.AddComponent<MeshCollider>();

				var meshRenderer = gameObject.AddComponent<UnityEngine.MeshRenderer>();

				// Assign the placeholder material.

				var meshMaterials = meshRenderer.sharedMaterials;

				for (var i = 0; i < meshMaterials.Length; i++)
				{
					meshMaterials[i] = (Material) AssetDatabase.LoadAssetAtPath("Assets/Materials/Dev/dev_measuregeneric01b.mat", typeof(Material));
				}

				meshRenderer.sharedMaterials = meshMaterials;

				// In order to make lightmap baking work, make object static.

				gameObject.isStatic = true;

				// If the solid is in a group, move it there.

				var editor = solid.Parent.Children.OfType<Nodes.Editor>().FirstOrDefault();

				if (editor != null)
				{
					var pair = groups.FirstOrDefault(x => x.Key.Identifier == editor.GroupIdentifier);

					if (pair.Value != null)
					{
						gameObject.transform.parent = pair.Value.transform;
					}
				}
			}

			base.Run();
		}
	}
}
