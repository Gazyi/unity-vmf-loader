using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityVMFLoader.Tasks
{
	[DependsOnTask(typeof(ParseNodesTask))]
	public class GroupNodesTask : Task
	{
		public Dictionary<UnityVMFLoader.Nodes.Group, GameObject> Groups;

		public override void Run()
		{
			var root = Importer.GetTask<ParseNodesTask>().Root;

			// Create groups from the parsed tree.

			Groups = new Dictionary<UnityVMFLoader.Nodes.Group, GameObject>();

			foreach (var group in root.Children.OfType<UnityVMFLoader.Nodes.World>().First().Children.OfType<UnityVMFLoader.Nodes.Group>())
			{
				Groups[group] = new GameObject("Group " + group.Identifier);
				Groups[group].transform.parent = (GameObject.Find("Brushes") ?? new GameObject("Brushes")).transform;
			}

			base.Run();
		}
	}
}
