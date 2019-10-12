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
namespace Org.Neo4j.Bolt.v1.messaging.decoder
{

	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using RequestMessageDecoder = Org.Neo4j.Bolt.messaging.RequestMessageDecoder;
	using BoltResponseHandler = Org.Neo4j.Bolt.runtime.BoltResponseHandler;
	using InitMessage = Org.Neo4j.Bolt.v1.messaging.request.InitMessage;
	using AuthToken = Org.Neo4j.Kernel.api.security.AuthToken;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

	public class InitMessageDecoder : RequestMessageDecoder
	{
		 private readonly BoltResponseHandler _responseHandler;

		 public InitMessageDecoder( BoltResponseHandler responseHandler )
		 {
			  this._responseHandler = responseHandler;
		 }

		 public override int Signature()
		 {
			  return InitMessage.SIGNATURE;
		 }

		 public override BoltResponseHandler ResponseHandler()
		 {
			  return _responseHandler;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.messaging.RequestMessage decode(org.neo4j.bolt.messaging.Neo4jPack_Unpacker unpacker) throws java.io.IOException
		 public override RequestMessage Decode( Org.Neo4j.Bolt.messaging.Neo4jPack_Unpacker unpacker )
		 {
			  string userAgent = unpacker.UnpackString();
			  IDictionary<string, object> authToken = ReadMetaDataMap( unpacker );
			  return new InitMessage( userAgent, authToken );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.Map<String,Object> readMetaDataMap(org.neo4j.bolt.messaging.Neo4jPack_Unpacker unpacker) throws java.io.IOException
		 public static IDictionary<string, object> ReadMetaDataMap( Org.Neo4j.Bolt.messaging.Neo4jPack_Unpacker unpacker )
		 {
			  MapValue metaDataMapValue = unpacker.UnpackMap();
			  PrimitiveOnlyValueWriter writer = new PrimitiveOnlyValueWriter();
			  IDictionary<string, object> metaDataMap = new Dictionary<string, object>( metaDataMapValue.Size() );
			  metaDataMapValue.Foreach((key, value) =>
			  {
				object convertedValue = AuthToken.containsSensitiveInformation( key ) ? writer.SensitiveValueAsObject( value ) : writer.ValueAsObject( value );
				metaDataMap[key] = convertedValue;
			  });
			  return metaDataMap;
		 }
	}

}