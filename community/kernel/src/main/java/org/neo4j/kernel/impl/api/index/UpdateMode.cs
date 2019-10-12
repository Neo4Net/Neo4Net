using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Kernel.Impl.Api.index
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.binarySearch;

	public abstract class UpdateMode
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ADDED { public boolean forLabel(long[] before, long[] after, long label) { return binarySearch(after, label) >= 0; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       CHANGED { public boolean forLabel(long[] before, long[] after, long label) { return ADDED.forLabel(before, after, label) && REMOVED.forLabel(before, after, label); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       REMOVED { public boolean forLabel(long[] before, long[] after, long label) { return binarySearch(before, label) >= 0; } };

		 private static readonly IList<UpdateMode> valueList = new List<UpdateMode>();

		 static UpdateMode()
		 {
			 valueList.Add( ADDED );
			 valueList.Add( CHANGED );
			 valueList.Add( REMOVED );
		 }

		 public enum InnerEnum
		 {
			 ADDED,
			 CHANGED,
			 REMOVED
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private UpdateMode( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public static readonly UpdateMode[] MODES = UpdateMode.values();

		 public abstract bool forLabel( long[] before, long[] after, long label );

		public static IList<UpdateMode> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static UpdateMode valueOf( string name )
		{
			foreach ( UpdateMode enumInstance in UpdateMode.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}