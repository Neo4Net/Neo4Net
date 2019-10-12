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
namespace Neo4Net.Bolt.v3.messaging
{

	using BoltRequestMessageReader = Neo4Net.Bolt.messaging.BoltRequestMessageReader;
	using BoltResponseMessageWriter = Neo4Net.Bolt.messaging.BoltResponseMessageWriter;
	using RequestMessageDecoder = Neo4Net.Bolt.messaging.RequestMessageDecoder;
	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using MessageProcessingHandler = Neo4Net.Bolt.v1.messaging.MessageProcessingHandler;
	using ResultHandler = Neo4Net.Bolt.v1.messaging.ResultHandler;
	using DiscardAllMessageDecoder = Neo4Net.Bolt.v1.messaging.decoder.DiscardAllMessageDecoder;
	using PullAllMessageDecoder = Neo4Net.Bolt.v1.messaging.decoder.PullAllMessageDecoder;
	using ResetMessageDecoder = Neo4Net.Bolt.v1.messaging.decoder.ResetMessageDecoder;
	using BeginMessageDecoder = Neo4Net.Bolt.v3.messaging.decoder.BeginMessageDecoder;
	using CommitMessageDecoder = Neo4Net.Bolt.v3.messaging.decoder.CommitMessageDecoder;
	using GoodbyeMessageDecoder = Neo4Net.Bolt.v3.messaging.decoder.GoodbyeMessageDecoder;
	using HelloMessageDecoder = Neo4Net.Bolt.v3.messaging.decoder.HelloMessageDecoder;
	using RollbackMessageDecoder = Neo4Net.Bolt.v3.messaging.decoder.RollbackMessageDecoder;
	using RunMessageDecoder = Neo4Net.Bolt.v3.messaging.decoder.RunMessageDecoder;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.@internal.LogService;

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