﻿using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityVMFLoader
{
	public static class VMFParser
	{
		public static Node Parse(string path)
		{
			var lines = File.ReadAllLines(path).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

			var root = new Node();
			var active = root;

			foreach (var line in lines)
			{
				var firstCharacter = line[0];

				switch (firstCharacter)
				{
					case '{':

						break;

					case '}':

						// End of current node.

						active = active.Parent ?? root;

						break;

					case '"':

						// A key-value.

						var keyEnd = line.IndexOf('"', 1);
						var key = line.Substring(1, keyEnd - 1);

						var valueStart = line.IndexOf('"', keyEnd + 1);
						var value = line.Substring(valueStart, line.Length - valueStart).Trim('"');

						active.Parse(key, value);

						break;

					default:

						// Start of a new node.

						var parent = active;

						switch (line)
						{
							case "world":

								active = new World();

								break;

							case "group":

								active = new Group();

								break;

							case "editor":

								active = new Editor();

								break;

							case "entity":

								active = new Entity();

								break;

							case "solid":

								active = new Solid();

								break;

							case "side":

								active = new Side();

								break;

							default:

								active = new Node();

								break;
						}

						active.Key = line;
						active.Parent = parent;

						break;
				}
			}

			// Create groups from the parsed tree.

			var groups = new Dictionary<Group, GameObject>();

			foreach (var group in root.Children.OfType<World>().First().Children.OfType<Group>())
			{
				groups[group] = new GameObject("Group " + group.Identifier);
			}

			// Create solids from the parsed tree.

			var solids = root.Children.OfType<World>().First().Children.OfType<Solid>();

			foreach (var entity in root.Children.OfType<Entity>())
			{
				solids = solids.Concat(entity.Children.OfType<Solid>());
			}

			foreach (var solid in solids)
			{
				GameObject gameObject = new GameObject("Solid " + solid.Identifier);

				gameObject.AddComponent<MeshRenderer>();
				gameObject.AddComponent<MeshFilter>();

				gameObject.GetComponent<MeshFilter>().sharedMesh = (Mesh) solid;

				var mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

				// The vertices of the mesh are in world coordinates so we'll need to center them.

				var center = new Vector3();

				foreach (var vertex in mesh.vertices)
				{
					center += vertex;
				}

				center /= mesh.vertices.Count();

				var vertices = mesh.vertices;

				for (var vertex = 0; vertex < vertices.Count(); vertex++)
				{
					vertices[vertex] -= center;
				}

				mesh.vertices = vertices;

				mesh.RecalculateBounds();

				// And move the object itself to those world coordinates.

				gameObject.transform.position = center;

				// If the solid is in a group, move it there.

				var editor = solid.Parent.Children.OfType<Editor>().FirstOrDefault();

				if (editor != null)
				{
					var pair = groups.FirstOrDefault(x => x.Key.Identifier == editor.GroupIdentifier);

					if (pair.Value != null)
					{
						gameObject.transform.parent = pair.Value.transform;
					}
				}
			}

			// Destroy the GameObjects of groups with a single child or none.

			var groupsCopy = groups.ToDictionary(entry => entry.Key, entry => entry.Value);

			foreach (var pair in groupsCopy.Where(x => x.Value.GetComponentsInChildren<Transform>().Length < 3))
			{
				var child = pair.Key.Children.FirstOrDefault();

				if (child != null)
				{
					child.Parent = pair.Key.Parent;
				}

				UnityEngine.Object.DestroyImmediate(pair.Value);

				groups.Remove(pair.Key);
			}

			return root;
		}
	}

	public class Node
	{
		public string Key;

		public ReadOnlyCollection<Node> Children
		{
			get
			{
				return children.AsReadOnly();
			}
		}

		private readonly List<Node> children = new List<Node>();

		public Node Parent
		{
			get
			{
				return parent;
			}

			set
			{
				if (parent != null)
				{
					parent.children.Remove(this);
				}

				parent = value;

				if (parent != null && value != null)
				{
					parent.children.Add(this);
				}
			}
		}

		private Node parent;

		public uint Identifier;

		public virtual void Parse(string key, string value)
		{
			switch (key)
			{
				case "id":

					Identifier = Convert.ToUInt32(value);

					break;
			}
		}
	}

	public class World : Node
	{

	}

	public class Group : Node
	{

	}

	public class Editor : Node
	{
		public uint GroupIdentifier;

		public override void Parse(string key, string value)
		{
			base.Parse(key, value);

			switch (key)
			{
				case "groupid":

					GroupIdentifier = Convert.ToUInt32(value);

					break;
			}
		}
	}

	public class Entity : Node
	{

	}

	public class Solid : Node
	{
		static public explicit operator Mesh(Solid solid)
		{
			var mesh = new Mesh();

			var combines = new CombineInstance[solid.Children.OfType<Side>().Count()];

			var i = 0;

			foreach (var side in solid.Children.OfType<Side>())
			{
				combines[i++].mesh = (Mesh) side;
			}

			mesh.CombineMeshes(combines, true, false);
			mesh.Optimize();

			return mesh;
		}
	}

	public class Side : Node
	{
		public Vector3 PointA;
		public Vector3 PointB;
		public Vector3 PointC;

		private static readonly Regex planeRegex;

		static Side()
		{
			planeRegex = new Regex(@"(?:\((\-?\d+(?:.\S+)?) (\-?\d+(?:.\S+)?) (\-?\d+(?:.\S+)?)\) ?){3}", RegexOptions.Compiled);
		}

		public override void Parse(string key, string value)
		{
			base.Parse(key, value);

			switch (key)
			{
				case "plane":

					// (-98.0334 356.145 -1.90735e-006) (-98.0334 356.145 0.999998) (-122 334.941 0.999998)

					var match = planeRegex.Match(value);

					if (!match.Success)
					{
						throw new Exception("Failed to match a plane on side " + Identifier + ".");
					}

					PointA = new Vector3
					(
						float.Parse(match.Groups[1].Captures[0].Value),
						float.Parse(match.Groups[2].Captures[0].Value),
						float.Parse(match.Groups[3].Captures[0].Value)
					);

					PointB = new Vector3
					(
						float.Parse(match.Groups[1].Captures[1].Value),
						float.Parse(match.Groups[2].Captures[1].Value),
						float.Parse(match.Groups[3].Captures[1].Value)
					);

					PointC = new Vector3
					(
						float.Parse(match.Groups[1].Captures[2].Value),
						float.Parse(match.Groups[2].Captures[2].Value),
						float.Parse(match.Groups[3].Captures[2].Value)
					);

					// Source uses Z for up, but Unity uses Y.

					var y = PointA.y;

					PointA.y = PointA.z;
					PointA.z = y;

					y = PointB.y;

					PointB.y = PointB.z;
					PointB.z = y;

					y = PointC.y;

					PointC.y = PointC.z;
					PointC.z = y;

					break;
			}
		}

		static public explicit operator Mesh(Side side)
		{
			var mesh = new Mesh();

			var vertices = new Vector3[4];

			var vertex = 0;

			const float inchesInMeters = 0.0254f;

			vertices[vertex++] = side.PointA * inchesInMeters;
			vertices[vertex++] = side.PointB * inchesInMeters;
			vertices[vertex++] = side.PointC * inchesInMeters;
			vertices[vertex++] = (side.PointC + (side.PointA - side.PointB)) * inchesInMeters;

			mesh.vertices = vertices;

			var textureCoordinates = new Vector2[vertices.Length];

			for (var i = 0; i < vertices.Length; i++)
			{
				textureCoordinates[i] = new Vector2(vertices[i].x, vertices[i].z);
			}

			mesh.uv = textureCoordinates;

			mesh.RecalculateNormals();

			mesh.triangles = new[]
			{
				0, 1, 2,
				2, 3, 0
			};

			return mesh;
		}
	}
}
