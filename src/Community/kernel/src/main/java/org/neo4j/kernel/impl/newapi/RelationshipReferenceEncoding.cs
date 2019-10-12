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
namespace Neo4Net.Kernel.Impl.Newapi
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.AbstractBaseRecord.NO_ID;

	public sealed class RelationshipReferenceEncoding
	{
		 /// <summary>
		 /// No encoding </summary>
		 public static readonly RelationshipReferenceEncoding None = new RelationshipReferenceEncoding( "None", InnerEnum.None, 0 );

		 /// <seealso cref= #encodeForFiltering(long) </seealso>
		 public static readonly RelationshipReferenceEncoding Filter = new RelationshipReferenceEncoding( "Filter", InnerEnum.Filter, 1 );

		 /// <seealso cref= #encodeForTxStateFiltering(long) </seealso>
		 public static readonly RelationshipReferenceEncoding FilterTxState = new RelationshipReferenceEncoding( "FilterTxState", InnerEnum.FilterTxState, 2 );

		 /// <seealso cref= #encodeGroup(long) </seealso>
		 public static readonly RelationshipReferenceEncoding Group = new RelationshipReferenceEncoding( "Group", InnerEnum.Group, 3 );

		 /// <seealso cref= #encodeNoOutgoingRels(int) </seealso>
		 public static readonly RelationshipReferenceEncoding NoOutgoingOfType = new RelationshipReferenceEncoding( "NoOutgoingOfType", InnerEnum.NoOutgoingOfType, 4 );

		 /// <seealso cref= #encodeNoIncomingRels(int) </seealso>
		 public static readonly RelationshipReferenceEncoding NoIncomingOfType = new RelationshipReferenceEncoding( "NoIncomingOfType", InnerEnum.NoIncomingOfType, 5 );

		 /// <seealso cref= #encodeNoLoopRels(int) </seealso>
		 public static readonly RelationshipReferenceEncoding NoLoopOfType = new RelationshipReferenceEncoding( "NoLoopOfType", InnerEnum.NoLoopOfType, 6 );

		 private static readonly IList<RelationshipReferenceEncoding> valueList = new List<RelationshipReferenceEncoding>();

		 static RelationshipReferenceEncoding()
		 {
			 valueList.Add( None );
			 valueList.Add( Filter );
			 valueList.Add( FilterTxState );
			 valueList.Add( Group );
			 valueList.Add( NoOutgoingOfType );
			 valueList.Add( NoIncomingOfType );
			 valueList.Add( NoLoopOfType );
		 }

		 public enum InnerEnum
		 {
			 None,
			 Filter,
			 FilterTxState,
			 Group,
			 NoOutgoingOfType,
			 NoIncomingOfType,
			 NoLoopOfType
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private const;
		 internal Final long;
		 internal Final long;

		 internal RelationshipReferenceEncoding( string name, InnerEnum innerEnum, long id )
		 {
			  this.Id = id;
			  this.Bits = id << 60;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public static RelationshipReferenceEncoding ParseEncoding( long reference )
		 {
			  if ( reference == NO_ID )
			  {
					return NONE;
			  }
			  return _encodings[EncodingId( reference )];
		 }

		 private static int EncodingId( long reference )
		 {
			  return ( int )( ( reference & References.FLAG_MASK ) >> 60 );
		 }

		 /// <summary>
		 /// Encode a group id as a relationship reference.
		 /// </summary>
		 public static long EncodeGroup( long groupId )
		 {
			  return groupId | GROUP.bits | References.FLAG_MARKER;
		 }

		 /// <summary>
		 /// Encode that the relationship id needs filtering by it's first element.
		 /// </summary>
		 public static long EncodeForFiltering( long relationshipId )
		 {
			  return relationshipId | FILTER.bits | References.FLAG_MARKER;
		 }

		 /// <summary>
		 /// Encode that the relationship id needs filtering by it's first element.
		 /// </summary>
		 public static long EncodeForTxStateFiltering( long relationshipId )
		 {
			  return relationshipId | FILTER_TX_STATE.bits | References.FLAG_MARKER;
		 }

		 /// <summary>
		 /// Encode that no outgoing relationships of the encoded type exist.
		 /// </summary>
		 public static long EncodeNoOutgoingRels( int type )
		 {
			  return type | NO_OUTGOING_OF_TYPE.bits | References.FLAG_MARKER;
		 }

		 /// <summary>
		 /// Encode that no incoming relationships of the encoded type exist.
		 /// </summary>
		 public static long EncodeNoIncomingRels( int type )
		 {
			  return type | NO_INCOMING_OF_TYPE.bits | References.FLAG_MARKER;
		 }

		 /// <summary>
		 /// Encode that no loop relationships of the encoded type exist.
		 /// </summary>
		 public static long EncodeNoLoopRels( int type )
		 {
			  return type | NO_LOOP_OF_TYPE.bits | References.FLAG_MARKER;
		 }

		public static IList<RelationshipReferenceEncoding> values()
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

		public static RelationshipReferenceEncoding valueOf( string name )
		{
			foreach ( RelationshipReferenceEncoding enumInstance in RelationshipReferenceEncoding.valueList )
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