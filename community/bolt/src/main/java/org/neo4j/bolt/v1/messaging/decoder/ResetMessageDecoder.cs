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
namespace Org.Neo4j.Bolt.v1.messaging.decoder
{

	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using RequestMessageDecoder = Org.Neo4j.Bolt.messaging.RequestMessageDecoder;
	using BoltConnection = Org.Neo4j.Bolt.runtime.BoltConnection;
	using BoltResponseHandler = Org.Neo4j.Bolt.runtime.BoltResponseHandler;
	using ResetMessage = Org.Neo4j.Bolt.v1.messaging.request.ResetMessage;

	public class ResetMessageDecoder : RequestMessageDecoder
	{
		 private readonly BoltConnection _connection;
		 private readonly BoltResponseHandler _responseHandler;

		 public ResetMessageDecoder( BoltConnection connection, BoltResponseHandler responseHandler )
		 {
			  this._connection = connection;
			  this._responseHandler = responseHandler;
		 }

		 public override int Signature()
		 {
			  return ResetMessage.SIGNATURE;
		 }

		 public override BoltResponseHandler ResponseHandler()
		 {
			  return _responseHandler;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.messaging.RequestMessage decode(org.neo4j.bolt.messaging.Neo4jPack_Unpacker unpacker) throws java.io.IOException
		 public override RequestMessage Decode( Org.Neo4j.Bolt.messaging.Neo4jPack_Unpacker unpacker )
		 {
			  _connection.interrupt();
			  return ResetMessage.INSTANCE;
		 }
	}

}