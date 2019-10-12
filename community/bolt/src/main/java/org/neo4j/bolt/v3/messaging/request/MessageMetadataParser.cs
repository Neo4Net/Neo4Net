using System;
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
namespace Org.Neo4j.Bolt.v3.messaging.request
{

	using BoltIOException = Org.Neo4j.Bolt.messaging.BoltIOException;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Point = Org.Neo4j.Graphdb.spatial.Point;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using Org.Neo4j.Kernel.impl.util;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using LongValue = Org.Neo4j.Values.Storable.LongValue;
	using Values = Org.Neo4j.Values.Storable.Values;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

	/// <summary>
	/// The parsing methods in this class returns null if the specified key is not found in the input message metadata map.
	/// </summary>
	internal sealed class MessageMetadataParser
	{
		 private const string TX_TIMEOUT_KEY = "tx_timeout";
		 private const string TX_META_DATA_KEY = "tx_metadata";

		 private MessageMetadataParser()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static java.time.Duration parseTransactionTimeout(org.neo4j.values.virtual.MapValue meta) throws org.neo4j.bolt.messaging.BoltIOException
		 internal static Duration ParseTransactionTimeout( MapValue meta )
		 {
			  AnyValue anyValue = meta.Get( TX_TIMEOUT_KEY );
			  if ( anyValue == Values.NO_VALUE )
			  {
					return null;
			  }
			  else if ( anyValue is LongValue )
			  {
					return Duration.ofMillis( ( ( LongValue ) anyValue ).longValue() );
			  }
			  else
			  {
					throw new BoltIOException( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid, "Expecting transaction timeout value to be a Long value, but got: " + anyValue );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static java.util.Map<String,Object> parseTransactionMetadata(org.neo4j.values.virtual.MapValue meta) throws org.neo4j.bolt.messaging.BoltIOException
		 internal static IDictionary<string, object> ParseTransactionMetadata( MapValue meta )
		 {
			  AnyValue anyValue = meta.Get( TX_META_DATA_KEY );
			  if ( anyValue == Values.NO_VALUE )
			  {
					return null;
			  }
			  else if ( anyValue is MapValue )
			  {
					MapValue mapValue = ( MapValue ) anyValue;
					TransactionMetadataWriter writer = new TransactionMetadataWriter();
					IDictionary<string, object> txMeta = new Dictionary<string, object>( mapValue.Size() );
					mapValue.Foreach( ( key, value ) => txMeta.put( key, writer.ValueAsObject( value ) ) );
					return txMeta;
			  }
			  else
			  {
					throw new BoltIOException( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid, "Expecting transaction metadata value to be a Map value, but got: " + anyValue );
			  }
		 }

		 private class TransactionMetadataWriter : BaseToObjectValueWriter<Exception>
		 {
			  protected internal override Node NewNodeProxyById( long id )
			  {
					throw new System.NotSupportedException( "Transaction metadata should not contain nodes" );
			  }

			  protected internal override Relationship NewRelationshipProxyById( long id )
			  {
					throw new System.NotSupportedException( "Transaction metadata should not contain relationships" );
			  }

			  protected internal override Point NewPoint( CoordinateReferenceSystem crs, double[] coordinate )
			  {
					return Values.pointValue( crs, coordinate );
			  }

			  internal virtual object ValueAsObject( AnyValue value )
			  {
					value.WriteTo( this );
					return value();
			  }
		 }
	}

}