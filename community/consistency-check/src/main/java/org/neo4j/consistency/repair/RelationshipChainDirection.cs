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
namespace Org.Neo4j.Consistency.repair
{
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;

	public sealed class RelationshipChainDirection
	{
		 public static readonly RelationshipChainDirection Next = new RelationshipChainDirection( "Next", InnerEnum.Next, RelationshipChainField.FirstNext, RelationshipChainField.SecondNext );
		 public static readonly RelationshipChainDirection Prev = new RelationshipChainDirection( "Prev", InnerEnum.Prev, RelationshipChainField.FirstPrev, RelationshipChainField.SecondPrev );

		 private static readonly IList<RelationshipChainDirection> valueList = new List<RelationshipChainDirection>();

		 static RelationshipChainDirection()
		 {
			 valueList.Add( Next );
			 valueList.Add( Prev );
		 }

		 public enum InnerEnum
		 {
			 Next,
			 Prev
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private readonly;

		 internal RelationshipChainDirection( string name, InnerEnum innerEnum, RelationshipChainField first, RelationshipChainField second )
		 {
			  this._first = first;
			  this._second = second;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public RelationshipChainField FieldFor( long nodeId, Org.Neo4j.Kernel.impl.store.record.RelationshipRecord rel )
		 {
			  if ( rel.FirstNode == nodeId )
			  {
					return _first;
			  }
			  else if ( rel.SecondNode == nodeId )
			  {
					return _second;
			  }
			  throw new System.ArgumentException( format( "%s does not reference node %d", rel, nodeId ) );
		 }

		public static IList<RelationshipChainDirection> values()
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

		public static RelationshipChainDirection valueOf( string name )
		{
			foreach ( RelationshipChainDirection enumInstance in RelationshipChainDirection.valueList )
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