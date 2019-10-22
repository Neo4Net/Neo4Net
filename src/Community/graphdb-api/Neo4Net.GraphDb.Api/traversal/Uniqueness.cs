using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.GraphDb.traversal
{
	/// <summary>
	/// A catalog of convenient uniqueness factories.
	/// </summary>
	public sealed class Uniqueness : UniquenessFactory
	{
		 /// <summary>
		 /// A node cannot be traversed more than once. This is what the legacy
		 /// traversal framework does.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NODE_GLOBAL { public UniquenessFilter create(Object optionalParameter) { acceptNull(optionalParameter); return new GloballyUnique(PrimitiveTypeFetcher.NODE); } public boolean eagerStartBranches() { return true; } },
		 /// <summary>
		 /// For each returned node there's a unique path from the start node to it.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NODE_PATH { public UniquenessFilter create(Object optionalParameter) { acceptNull(optionalParameter); return new PathUnique(PrimitiveTypeFetcher.NODE); } public boolean eagerStartBranches() { return true; } },
		 /// <summary>
		 /// This is like <seealso cref="Uniqueness.NODE_GLOBAL"/>, but only guarantees
		 /// uniqueness among the most recent visited nodes, with a configurable
		 /// count. Traversing a huge graph is quite memory intensive in that it keeps
		 /// track of <i>all</i> the nodes it has visited. For huge graphs a traverser
		 /// can hog all the memory in the JVM, causing <seealso cref="System.OutOfMemoryException"/>.
		 /// Together with this <seealso cref="Uniqueness"/> you can supply a count, which is
		 /// the number of most recent visited nodes. This can cause a node to be
		 /// visited more than once, but scales infinitely.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NODE_RECENT { public UniquenessFilter create(Object optionalParameter) { acceptIntegerOrNull(optionalParameter); return new RecentlyUnique(PrimitiveTypeFetcher.NODE, optionalParameter); } public boolean eagerStartBranches() { return true; } },
		 /// <summary>
		 /// Entities on the same level are guaranteed to be unique.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NODE_LEVEL { public UniquenessFilter create(Object optionalParameter) { acceptNull(optionalParameter); return new LevelUnique(PrimitiveTypeFetcher.NODE); } public boolean eagerStartBranches() { return true; } },

		 /// <summary>
		 /// A relationship cannot be traversed more than once, whereas nodes can.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       RELATIONSHIP_GLOBAL { public UniquenessFilter create(Object optionalParameter) { acceptNull(optionalParameter); return new GloballyUnique(PrimitiveTypeFetcher.RELATIONSHIP); } public boolean eagerStartBranches() { return true; } },
		 /// <summary>
		 /// For each returned node there's a (relationship wise) unique path from the
		 /// start node to it.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       RELATIONSHIP_PATH { public UniquenessFilter create(Object optionalParameter) { acceptNull(optionalParameter); return new PathUnique(PrimitiveTypeFetcher.RELATIONSHIP); } public boolean eagerStartBranches() { return false; } },
		 /// <summary>
		 /// Same as for <seealso cref="Uniqueness.NODE_RECENT"/>, but for relationships.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       RELATIONSHIP_RECENT { public UniquenessFilter create(Object optionalParameter) { acceptIntegerOrNull(optionalParameter); return new RecentlyUnique(PrimitiveTypeFetcher.RELATIONSHIP, optionalParameter); } public boolean eagerStartBranches() { return true; } },
		 /// <summary>
		 /// Entities on the same level are guaranteed to be unique.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       RELATIONSHIP_LEVEL { public UniquenessFilter create(Object optionalParameter) { acceptNull(optionalParameter); return new LevelUnique(PrimitiveTypeFetcher.RELATIONSHIP); } public boolean eagerStartBranches() { return true; } },

		 /// <summary>
		 /// No restriction (the user will have to manage it).
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NONE { public UniquenessFilter create(Object optionalParameter) { acceptNull(optionalParameter); return notUniqueInstance; } public boolean eagerStartBranches() { return true; } };

		 private static readonly IList<Uniqueness> valueList = new List<Uniqueness>();

		 static Uniqueness()
		 {
			 valueList.Add( NODE_GLOBAL );
			 valueList.Add( NODE_PATH );
			 valueList.Add( NODE_RECENT );
			 valueList.Add( NODE_LEVEL );
			 valueList.Add( RELATIONSHIP_GLOBAL );
			 valueList.Add( RELATIONSHIP_PATH );
			 valueList.Add( RELATIONSHIP_RECENT );
			 valueList.Add( RELATIONSHIP_LEVEL );
			 valueList.Add( NONE );
		 }

		 public enum InnerEnum
		 {
			 NODE_GLOBAL,
			 NODE_PATH,
			 NODE_RECENT,
			 NODE_LEVEL,
			 RELATIONSHIP_GLOBAL,
			 RELATIONSHIP_PATH,
			 RELATIONSHIP_RECENT,
			 RELATIONSHIP_LEVEL,
			 NONE
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private Uniqueness( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 private static readonly UniquenessFilter notUniqueInstance = new NotUnique();

		 public static readonly Uniqueness private static void acceptNull( Object optionalParameter )
		 {
			 if ( optionalParameter != null ) { throw new IllegalArgumentException( "Only accepts null parameter, was " + optionalParameter ); }
		 }
		 private static void acceptIntegerOrNull( Object parameter )
		 {
			 if ( parameter == null ) { return; } boolean isDecimalNumber = parameter instanceof Number && !( parameter instanceof float? || parameter instanceof double? ); if ( !isDecimalNumber ) { throw new IllegalArgumentException( "Doesn't accept non-decimal values" + ", like '" + parameter + "'" ); }
		 }
		 = new Uniqueness("private static void acceptNull(Object optionalParameter) { if(optionalParameter != null) { throw new IllegalArgumentException("Only accepts null parameter, was " + optionalParameter); } } private static void acceptIntegerOrNull(Object parameter) { if(parameter == null) { return; } boolean isDecimalNumber = parameter instanceof Number && !(parameter instanceof System.Nullable<float> || parameter instanceof System.Nullable<double>); if(!isDecimalNumber) { throw new IllegalArgumentException("Doesn't accept non - decimal values" + ", like '" + parameter + "'"); } }", InnerEnum.private static void acceptNull(Object optionalParameter)
		 {
			 if ( optionalParameter != null ) { throw new IllegalArgumentException( "Only accepts null parameter, was " + optionalParameter ); }
		 }
		 private static void acceptIntegerOrNull( Object parameter )
		 {
			 if ( parameter == null ) { return; } boolean isDecimalNumber = parameter instanceof Number && !( parameter instanceof float? || parameter instanceof double? ); if ( !isDecimalNumber ) { throw new IllegalArgumentException( "Doesn't accept non-decimal values" + ", like '" + parameter + "'" ); }
		 });

		public static IList<Uniqueness> values()
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

		public static Uniqueness valueOf( string name )
		{
			foreach ( Uniqueness enumInstance in Uniqueness.valueList )
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