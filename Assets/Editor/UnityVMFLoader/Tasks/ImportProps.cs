using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityVMFLoader.Nodes;

namespace UnityVMFLoader.Tasks
{
	[DependsOnTask(typeof(ImportPointEntitiesTask))]
	public class ImportPropsTask : Task
	{
		public override void Run()
		{
			var entities = Importer.GetTask<ImportPointEntitiesTask>().Entities;

			var props = entities.Where(entity => entity.ClassName.StartsWith("prop_"));

			foreach (var prop in props)
			{
				var propObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);

				propObject.name = "Prop " + prop.Identifier;

				propObject.transform.parent = (GameObject.Find("Props") ?? new GameObject("Props")).transform;

				propObject.transform.position = prop.Origin;
				propObject.transform.rotation = prop.Angles;
			}

			base.Run();
		}
	}
}
