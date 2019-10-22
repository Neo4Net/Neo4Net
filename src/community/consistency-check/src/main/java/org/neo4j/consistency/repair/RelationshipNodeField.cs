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
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

	public abstract class RelationshipNodeField
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       FIRST { public long get(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.getFirstNode(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SECOND { public long get(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.getSecondNode(); } };

		 private static readonly IList<RelationshipNodeField> valueList = new List<RelationshipNodeField>();

		 static RelationshipNodeField()
		 {
			 valueList.Add( FIRST );
			 valueList.Add( SECOND );
		 }

		 public enum InnerEnum
		 {
			 FIRST,
			 SECOND
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private RelationshipNodeField( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public abstract long get( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord rel );

		public static IList<RelationshipNodeField> values()
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

		public static RelationshipNodeField valueOf( string name )
		{
			foreach ( RelationshipNodeField enumInstance in RelationshipNodeField.valueList )
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