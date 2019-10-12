using System;

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
namespace Org.Neo4j.Bolt.v1.messaging.decoder
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;


	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Point = Org.Neo4j.Graphdb.spatial.Point;
	using Org.Neo4j.Kernel.impl.util;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using Org.Neo4j.Values;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using StringValue = Org.Neo4j.Values.Storable.StringValue;
	using UTF8StringValue = Org.Neo4j.Values.Storable.UTF8StringValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.NoValue.NO_VALUE;

	/// <summary>
	/// <seealso cref="AnyValueWriter Writer"/> that allows to convert <seealso cref="AnyValue"/> to any primitive Java type. It explicitly
	/// prohibits conversion of nodes, relationships, spatial and temporal types. They are not expected in auth token map.
	/// </summary>
	public class PrimitiveOnlyValueWriter : BaseToObjectValueWriter<Exception>
	{
		 public virtual object ValueAsObject( AnyValue value )
		 {
			  value.WriteTo( this );
			  return value();
		 }

		 public virtual object SensitiveValueAsObject( AnyValue value )
		 {
			  if ( value is UTF8StringValue )
			  {
					return ( ( UTF8StringValue ) value ).bytes();
			  }
			  else if ( value == NO_VALUE )
			  {
					return null;
			  }
			  else if ( value is StringValue && ( ( StringValue ) value ).Equals( "" ) )
			  {
					return ArrayUtils.EMPTY_BYTE_ARRAY;
			  }
			  return ValueAsObject( value );
		 }

		 protected internal override Node NewNodeProxyById( long id )
		 {
			  throw new System.NotSupportedException( "INIT message metadata should not contain nodes" );
		 }

		 protected internal override Relationship NewRelationshipProxyById( long id )
		 {
			  throw new System.NotSupportedException( "INIT message metadata should not contain relationships" );
		 }

		 protected internal override Point NewPoint( CoordinateReferenceSystem crs, double[] coordinate )
		 {
			  throw new System.NotSupportedException( "INIT message metadata should not contain points" );
		 }

		 public override void WriteByteArray( sbyte[] value )
		 {
			  throw new System.NotSupportedException( "INIT message metadata should not contain byte arrays" );
		 }

		 public override void WriteDuration( long months, long days, long seconds, int nanos )
		 {
			  throw new System.NotSupportedException( "INIT message metadata should not contain durations" );
		 }

		 public override void WriteDate( LocalDate localDate )
		 {
			  throw new System.NotSupportedException( "INIT message metadata should not contain dates" );
		 }

		 public override void WriteLocalTime( LocalTime localTime )
		 {
			  throw new System.NotSupportedException( "INIT message metadata should not contain local dates" );
		 }

		 public override void WriteTime( OffsetTime offsetTime )
		 {
			  throw new System.NotSupportedException( "INIT message metadata should not contain time values" );
		 }

		 public override void WriteLocalDateTime( DateTime localDateTime )
		 {
			  throw new System.NotSupportedException( "INIT message metadata should not contain local date-time values" );
		 }

		 public override void WriteDateTime( ZonedDateTime zonedDateTime )
		 {
			  throw new System.NotSupportedException( "INIT message metadata should not contain date-time values" );
		 }
	}

}