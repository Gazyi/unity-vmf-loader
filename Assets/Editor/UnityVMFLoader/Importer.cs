using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace UnityVMFLoader
{
	public static class Importer
	{
		public static string Path
		{
			get;
			private set;
		}

		public static event EventHandler OnFinished;

		private static List<Task> tasks;

		static Importer()
		{
			tasks = new List<Task>();
		}

		public static void AddTask<T>() where T : Task
		{
			tasks.Add(Activator.CreateInstance<T>());
		}

		public static T GetTask<T>() where T : Task
		{
			return (T) tasks.FirstOrDefault(task => task.GetType() == typeof(T));
		}

		public static Task GetTask(Type taskType)
		{
			return (Task) tasks.FirstOrDefault(task => task.GetType() == taskType);
		}

		public static void Import(string path)
		{
			Path = path;

			UnityThreadHelper.EnsureHelper();

			try
			{
				var tasksTotal = (float) tasks.Count;
				var tasksDone = 0f;

				while (tasks.Any(task => !task.Done && task.CanRun))
				{
					foreach (var task in tasks)
					{
						if (!task.Done && task.CanRun)
						{
							EditorUtility.DisplayProgressBar("Importing VMF", task.GetType().Name, tasksDone / tasksTotal);

							task.Run();
						}

						if (task.Done)
						{
							tasksDone++;
						}
					}
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

			if (OnFinished != null)
			{
				OnFinished(null, null);
			}

			tasks.Clear();
		}
	}
}
