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
namespace Neo4Net.Bolt.messaging
{

	using Neo4jPackV1 = Neo4Net.Bolt.v1.messaging.Neo4jPackV1;
	using Neo4jPackV2 = Neo4Net.Bolt.v2.messaging.Neo4jPackV2;

	public sealed class StructType
	{
		 public static readonly StructType Node = new StructType( "Node", InnerEnum.Node, Neo4Net.Bolt.v1.messaging.Neo4jPackV1.NODE, "Node" );
		 public static readonly StructType Relationship = new StructType( "Relationship", InnerEnum.Relationship, Neo4Net.Bolt.v1.messaging.Neo4jPackV1.RELATIONSHIP, "Relationship" );
		 public static readonly StructType UnboundRelationship = new StructType( "UnboundRelationship", InnerEnum.UnboundRelationship, Neo4Net.Bolt.v1.messaging.Neo4jPackV1.UNBOUND_RELATIONSHIP, "Relationship" );
		 public static readonly StructType Path = new StructType( "Path", InnerEnum.Path, Neo4Net.Bolt.v1.messaging.Neo4jPackV1.PATH, "Path" );
		 public static readonly StructType Point_2d = new StructType( "Point_2d", InnerEnum.Point_2d, Neo4Net.Bolt.v2.messaging.Neo4jPackV2.POINT_2_D, "Point" );
		 public static readonly StructType Point_3d = new StructType( "Point_3d", InnerEnum.Point_3d, Neo4Net.Bolt.v2.messaging.Neo4jPackV2.POINT_3_D, "Point" );
		 public static readonly StructType Date = new StructType( "Date", InnerEnum.Date, Neo4Net.Bolt.v2.messaging.Neo4jPackV2.DATE, "LocalDate" );
		 public static readonly StructType Time = new StructType( "Time", InnerEnum.Time, Neo4Net.Bolt.v2.messaging.Neo4jPackV2.TIME, "OffsetTime" );
		 public static readonly StructType LocalTime = new StructType( "LocalTime", InnerEnum.LocalTime, Neo4Net.Bolt.v2.messaging.Neo4jPackV2.LOCAL_TIME, "LocalTime" );
		 public static readonly StructType LocalDateTime = new StructType( "LocalDateTime", InnerEnum.LocalDateTime, Neo4Net.Bolt.v2.messaging.Neo4jPackV2.LOCAL_DATE_TIME, "LocalDateTime" );
		 public static readonly StructType DateTimeWithZoneOffset = new StructType( "DateTimeWithZoneOffset", InnerEnum.DateTimeWithZoneOffset, Neo4Net.Bolt.v2.messaging.Neo4jPackV2.DATE_TIME_WITH_ZONE_OFFSET, "OffsetDateTime" );
		 public static readonly StructType DateTimeWithZoneName = new StructType( "DateTimeWithZoneName", InnerEnum.DateTimeWithZoneName, Neo4Net.Bolt.v2.messaging.Neo4jPackV2.DATE_TIME_WITH_ZONE_NAME, "ZonedDateTime" );
		 public static readonly StructType Duration = new StructType( "Duration", InnerEnum.Duration, Neo4Net.Bolt.v2.messaging.Neo4jPackV2.DURATION, "Duration" );

		 private static readonly IList<StructType> valueList = new List<StructType>();

		 static StructType()
		 {
			 valueList.Add( Node );
			 valueList.Add( Relationship );
			 valueList.Add( UnboundRelationship );
			 valueList.Add( Path );
			 valueList.Add( Point_2d );
			 valueList.Add( Point_3d );
			 valueList.Add( Date );
			 valueList.Add( Time );
			 valueList.Add( LocalTime );
			 valueList.Add( LocalDateTime );
			 valueList.Add( DateTimeWithZoneOffset );
			 valueList.Add( DateTimeWithZoneName );
			 valueList.Add( Duration );
		 }

		 public enum InnerEnum
		 {
			 Node,
			 Relationship,
			 UnboundRelationship,
			 Path,
			 Point_2d,
			 Point_3d,
			 Date,
			 Time,
			 LocalTime,
			 LocalDateTime,
			 DateTimeWithZoneOffset,
			 DateTimeWithZoneName,
			 Duration
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private readonly;

		 internal StructType( string name, InnerEnum innerEnum, sbyte signature, string description )
		 {
			  this._signature = signature;
			  this._description = description;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public sbyte Signature()
		 {
			  return _signature;
		 }

		 public string Description()
		 {
			  return _description;
		 }

		 internal Private static;

		 public static StructType ValueOf( sbyte signature )
		 {
			  return _knownTypesBySignature.get( signature );
		 }

		 public static StructType ValueOf( char signature )
		 {
			  return _knownTypesBySignature.get( ( sbyte )signature );
		 }

		 private static IDictionary<sbyte, StructType> KnownTypesBySignature()
		 {
			  StructType[] types = StructType.values();
			  IDictionary<sbyte, StructType> result = new Dictionary<sbyte, StructType>( types.Length * 2 );
			  foreach ( StructType type in types )
			  {
					result[type._signature] = type;
			  }
			  return unmodifiableMap( result );
		 }

		public static IList<StructType> values()
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

		public static StructType valueOf( string name )
		{
			foreach ( StructType enumInstance in StructType.valueList )
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