using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityVMFLoader
{
	public abstract class Task
	{
		public bool CanRun
		{
			get
			{
				if (!Dependencies.ContainsKey(GetType()))
				{
					return true;
				}

				return Dependencies[GetType()].All(task => Importer.GetTask(task).Done);
			}
		}

		public bool Done;

		protected static readonly Dictionary<Type, List<Type>> Dependencies;

		static Task()
		{
			Dependencies = new Dictionary<Type, List<Type>>();

			// Get all Types with Task as the base type.

			var taskTypes = Assembly.GetCallingAssembly().GetTypes().Where(type => type.BaseType == typeof(Task));

			foreach (var taskType in taskTypes)
			{
				// Find the first (and the only) DependsOnTaskAttribute of the type.

				var attributes = Attribute.GetCustomAttributes(taskType);

				var dependencyAttribute = attributes.OfType<DependsOnTaskAttribute>().FirstOrDefault();

				// If there are no dependencies, move over to the next Task type.

				if (dependencyAttribute == null)
				{
					continue;
				}

				Dependencies[taskType] = dependencyAttribute.RequiredTasks.ToList();
			}
		}

		public virtual void Run()
		{
			Done = true;
		}
	}
}
