using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityVMFLoader.Nodes
{
	// Only used inside Dispinfo nodes.

	public class Normals : Node
	{
		public Vector3[,] Grid;

		public override void Parse(string key, string value)
		{
			base.Parse(key, value);

			/*
				"row0" "0 0 1 0 0 1 0 0 1 0 0 1 0 0 1"
				"row1" "0 0 1 0 0 1 0 0 1 0 0 1 0 0 1"
				"row2" "0 0 1 0 0 1 0 0 1 0 0 1 0 0 1"
				"row3" "0 0 1 0 0 1 0 0 1 0 0 1 0 0 1"
				"row4" "0 0 1 0 0 1 0 0 1 0 0 1 1.1128e-005 0.000215195 1"
			*/

			if (Grid == null)
			{
				var size = (int) Math.Pow(2, ((Dispinfo) Parent).Power) + 1;

				Grid = new Vector3[size, size];
			}

			if (key.Substring(0, 3) != "row")
			{
				return;
			}

			var rowNumber = (int) Char.GetNumericValue(key[3]);
			var columnNumber = 0;

			var columns = value.Split(' ').Select(v => float.Parse(v)).ToArray();

			while (columnNumber < columns.Length)
			{
				var position = columns.Skip(columnNumber).Take(3).ToArray();

				Grid[rowNumber, columnNumber / 3] = new Vector3(position[0], position[1], position[2]).SourceToUnity();

				columnNumber += 3;
			}
		}
	}
}
