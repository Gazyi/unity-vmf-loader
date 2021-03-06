using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityVMFLoader.Nodes;

namespace UnityVMFLoader.Tasks
{
	[DependsOnTask(typeof(ParseNodesTask))]
	public class ImportBrushEntitiesTask : Task
	{
		public IEnumerable<Entity> Entities;

		public override void Run()
		{
			var root = Importer.GetTask<ParseNodesTask>().Root;

			var entities = root.Children.OfType<Entity>().Where(x => x.Children.OfType<Solid>().Any());

			Entities = entities;

			base.Run();
		}
	}
}
