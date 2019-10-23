using System;
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
namespace Neo4Net.Bolt.v3.messaging.request
{

	using BoltIOException = Neo4Net.Bolt.messaging.BoltIOException;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Point = Neo4Net.GraphDb.Spatial.Point;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Neo4Net.Kernel.impl.util;
	using AnyValue = Neo4Net.Values.AnyValue;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using LongValue = Neo4Net.Values.Storable.LongValue;
	using Values = Neo4Net.Values.Storable.Values;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

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
//ORIGINAL LINE: static java.time.Duration parseTransactionTimeout(org.Neo4Net.values.virtual.MapValue meta) throws org.Neo4Net.bolt.messaging.BoltIOException
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
					throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "Expecting transaction timeout value to be a Long value, but got: " + anyValue );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static java.util.Map<String,Object> parseTransactionMetadata(org.Neo4Net.values.virtual.MapValue meta) throws org.Neo4Net.bolt.messaging.BoltIOException
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
					throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "Expecting transaction metadata value to be a Map value, but got: " + anyValue );
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