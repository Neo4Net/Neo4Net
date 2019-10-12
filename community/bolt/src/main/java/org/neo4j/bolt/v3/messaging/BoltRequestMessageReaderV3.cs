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
namespace Org.Neo4j.Bolt.v3.messaging
{

	using BoltRequestMessageReader = Org.Neo4j.Bolt.messaging.BoltRequestMessageReader;
	using BoltResponseMessageWriter = Org.Neo4j.Bolt.messaging.BoltResponseMessageWriter;
	using RequestMessageDecoder = Org.Neo4j.Bolt.messaging.RequestMessageDecoder;
	using BoltConnection = Org.Neo4j.Bolt.runtime.BoltConnection;
	using BoltResponseHandler = Org.Neo4j.Bolt.runtime.BoltResponseHandler;
	using MessageProcessingHandler = Org.Neo4j.Bolt.v1.messaging.MessageProcessingHandler;
	using ResultHandler = Org.Neo4j.Bolt.v1.messaging.ResultHandler;
	using DiscardAllMessageDecoder = Org.Neo4j.Bolt.v1.messaging.decoder.DiscardAllMessageDecoder;
	using PullAllMessageDecoder = Org.Neo4j.Bolt.v1.messaging.decoder.PullAllMessageDecoder;
	using ResetMessageDecoder = Org.Neo4j.Bolt.v1.messaging.decoder.ResetMessageDecoder;
	using BeginMessageDecoder = Org.Neo4j.Bolt.v3.messaging.decoder.BeginMessageDecoder;
	using CommitMessageDecoder = Org.Neo4j.Bolt.v3.messaging.decoder.CommitMessageDecoder;
	using GoodbyeMessageDecoder = Org.Neo4j.Bolt.v3.messaging.decoder.GoodbyeMessageDecoder;
	using HelloMessageDecoder = Org.Neo4j.Bolt.v3.messaging.decoder.HelloMessageDecoder;
	using RollbackMessageDecoder = Org.Neo4j.Bolt.v3.messaging.decoder.RollbackMessageDecoder;
	using RunMessageDecoder = Org.Neo4j.Bolt.v3.messaging.decoder.RunMessageDecoder;
	using Log = Org.Neo4j.Logging.Log;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

	public class BoltRequestMessageReaderV3 : BoltRequestMessageReader
	{
		 public BoltRequestMessageReaderV3( BoltConnection connection, BoltResponseMessageWriter responseMessageWriter, LogService logService ) : base( connection, NewSimpleResponseHandler( responseMessageWriter, connection, logService ), BuildDecoders( connection, responseMessageWriter, logService ) )
		 {
		 }

		 private static IList<RequestMessageDecoder> BuildDecoders( BoltConnection connection, BoltResponseMessageWriter responseMessageWriter, LogService logService )
		 {
			  BoltResponseHandler resultHandler = new ResultHandler( responseMessageWriter, connection, InternalLog( logService ) );
			  BoltResponseHandler defaultHandler = NewSimpleResponseHandler( responseMessageWriter, connection, logService );

			  return Arrays.asList(new HelloMessageDecoder(defaultHandler), new RunMessageDecoder(defaultHandler), new DiscardAllMessageDecoder(resultHandler), new PullAllMessageDecoder(resultHandler), new BeginMessageDecoder(defaultHandler), new CommitMessageDecoder(resultHandler), new RollbackMessageDecoder(resultHandler), new ResetMessageDecoder(connection, defaultHandler), new GoodbyeMessageDecoder(connection, defaultHandler)
			 );
		 }

		 private static BoltResponseHandler NewSimpleResponseHandler( BoltResponseMessageWriter responseMessageWriter, BoltConnection connection, LogService logService )
		 {
			  return new MessageProcessingHandler( responseMessageWriter, connection, InternalLog( logService ) );
		 }

		 private static Log InternalLog( LogService logService )
		 {
			  return logService.GetInternalLog( typeof( BoltRequestMessageReaderV3 ) );
		 }
	}

}