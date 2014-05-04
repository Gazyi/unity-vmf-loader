using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityVMFLoader.Nodes
{
	public class Dispinfo : Node
	{
		public int Power = 4;

		public float Elevation;

		public Vector3 Origin;

		public override void Parse(string key, string value)
		{
			base.Parse(key, value);

			switch(key)
			{
				case "power":

					Power = int.Parse(value);

					if (Power < 2 || Power > 4)
					{
						Debug.LogWarning("Displacement on side " + Parent.Identifier + " has invalid power. Only 2, 3 and 4 are accepted. Using 4 instead.");

						Power = 4;
					}

					break;

				case "startposition":

					var origin = value.Trim('[', ']').Split(' ').Select(v => float.Parse(v)).ToArray();

					Origin = new Vector3(origin[0], origin[1], origin[2]).SourceToUnity();

					break;

				case "elevation":

					Elevation = float.Parse(value);

					break;
			}
		}
	}
}
