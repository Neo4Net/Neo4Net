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
namespace Neo4Net.Kernel.impl.util
{
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;

	public abstract class DirectionWrapper
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       OUTGOING() { public long getNextRel(org.neo4j.kernel.impl.store.record.RelationshipGroupRecord group) { return group.getFirstOut(); } public void setNextRel(org.neo4j.kernel.impl.store.record.RelationshipGroupRecord group, long firstNextRel) { group.setFirstOut(firstNextRel); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       INCOMING() { public long getNextRel(org.neo4j.kernel.impl.store.record.RelationshipGroupRecord group) { return group.getFirstIn(); } public void setNextRel(org.neo4j.kernel.impl.store.record.RelationshipGroupRecord group, long firstNextRel) { group.setFirstIn(firstNextRel); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       BOTH() { public long getNextRel(org.neo4j.kernel.impl.store.record.RelationshipGroupRecord group) { return group.getFirstLoop(); } public void setNextRel(org.neo4j.kernel.impl.store.record.RelationshipGroupRecord group, long firstNextRel) { group.setFirstLoop(firstNextRel); } };

		 private static readonly IList<DirectionWrapper> valueList = new List<DirectionWrapper>();

		 static DirectionWrapper()
		 {
			 valueList.Add( OUTGOING );
			 valueList.Add( INCOMING );
			 valueList.Add( BOTH );
		 }

		 public enum InnerEnum
		 {
			 OUTGOING,
			 INCOMING,
			 BOTH
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private DirectionWrapper( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public abstract long getNextRel( Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord group );

		 public abstract void setNextRel( Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord group, long firstNextRel );

		public static IList<DirectionWrapper> values()
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

		public static DirectionWrapper valueOf( string name )
		{
			foreach ( DirectionWrapper enumInstance in DirectionWrapper.valueList )
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