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
namespace Neo4Net.Bolt.v1.messaging.decoder
{

	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using RequestMessageDecoder = Neo4Net.Bolt.messaging.RequestMessageDecoder;
	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;

	public class DiscardAllMessageDecoder : RequestMessageDecoder
	{
		 private readonly BoltResponseHandler _responseHandler;

		 public DiscardAllMessageDecoder( BoltResponseHandler responseHandler )
		 {
			  this._responseHandler = responseHandler;
		 }

		 public override int Signature()
		 {
			  return DiscardAllMessage.SIGNATURE;
		 }

		 public override BoltResponseHandler ResponseHandler()
		 {
			  return _responseHandler;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.messaging.RequestMessage decode(org.neo4j.bolt.messaging.Neo4jPack_Unpacker unpacker) throws java.io.IOException
		 public override RequestMessage Decode( Neo4Net.Bolt.messaging.Neo4jPack_Unpacker unpacker )
		 {
			  return DiscardAllMessage.INSTANCE;
		 }
	}

}