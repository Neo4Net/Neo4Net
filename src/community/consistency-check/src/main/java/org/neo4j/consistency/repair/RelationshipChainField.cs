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
namespace Neo4Net.Consistency.repair
{
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") public enum RelationshipChainField
	public abstract class RelationshipChainField
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       FIRST_NEXT { public long relOf(org.neo4j.kernel.impl.store.record.RelationshipRecord rel) { return rel.getFirstNextRel(); } public boolean endOfChain(org.neo4j.kernel.impl.store.record.RelationshipRecord rel) { return rel.getFirstNextRel() == org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.intValue(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       FIRST_PREV { public long relOf(org.neo4j.kernel.impl.store.record.RelationshipRecord rel) { return rel.getFirstPrevRel(); } public boolean endOfChain(org.neo4j.kernel.impl.store.record.RelationshipRecord rel) { return rel.isFirstInFirstChain(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SECOND_NEXT { public long relOf(org.neo4j.kernel.impl.store.record.RelationshipRecord rel) { return rel.getSecondNextRel(); } public boolean endOfChain(org.neo4j.kernel.impl.store.record.RelationshipRecord rel) { return rel.getSecondNextRel() == org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.intValue(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SECOND_PREV { public long relOf(org.neo4j.kernel.impl.store.record.RelationshipRecord rel) { return rel.getSecondPrevRel(); } public boolean endOfChain(org.neo4j.kernel.impl.store.record.RelationshipRecord rel) { return rel.isFirstInSecondChain(); } };

		 private static readonly IList<RelationshipChainField> valueList = new List<RelationshipChainField>();

		 static RelationshipChainField()
		 {
			 valueList.Add( FIRST_NEXT );
			 valueList.Add( FIRST_PREV );
			 valueList.Add( SECOND_NEXT );
			 valueList.Add( SECOND_PREV );
		 }

		 public enum InnerEnum
		 {
			 FIRST_NEXT,
			 FIRST_PREV,
			 SECOND_NEXT,
			 SECOND_PREV
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private RelationshipChainField( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public abstract long relOf( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord rel );

		 public abstract bool endOfChain( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord rel );

		public static IList<RelationshipChainField> values()
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

		public static RelationshipChainField valueOf( string name )
		{
			foreach ( RelationshipChainField enumInstance in RelationshipChainField.valueList )
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