﻿/*
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
namespace Org.Neo4j.Bolt.v3.messaging.decoder
{

	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using RequestMessageDecoder = Org.Neo4j.Bolt.messaging.RequestMessageDecoder;
	using BoltResponseHandler = Org.Neo4j.Bolt.runtime.BoltResponseHandler;
	using BeginMessage = Org.Neo4j.Bolt.v3.messaging.request.BeginMessage;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

	public class BeginMessageDecoder : RequestMessageDecoder
	{
		 private readonly BoltResponseHandler _responseHandler;

		 public BeginMessageDecoder( BoltResponseHandler responseHandler )
		 {
			  this._responseHandler = responseHandler;
		 }

		 public override int Signature()
		 {
			  return BeginMessage.SIGNATURE;
		 }

		 public override BoltResponseHandler ResponseHandler()
		 {
			  return _responseHandler;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.messaging.RequestMessage decode(org.neo4j.bolt.messaging.Neo4jPack_Unpacker unpacker) throws java.io.IOException
		 public override RequestMessage Decode( Org.Neo4j.Bolt.messaging.Neo4jPack_Unpacker unpacker )
		 {
			  MapValue meta = unpacker.UnpackMap();
			  return new BeginMessage( meta );
		 }
	}


}