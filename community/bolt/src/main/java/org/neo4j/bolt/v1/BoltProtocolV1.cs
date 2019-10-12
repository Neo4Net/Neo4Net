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
namespace Org.Neo4j.Bolt.v1
{
	using ChannelPipeline = io.netty.channel.ChannelPipeline;

	using BoltRequestMessageReader = Org.Neo4j.Bolt.messaging.BoltRequestMessageReader;
	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using BoltConnection = Org.Neo4j.Bolt.runtime.BoltConnection;
	using BoltConnectionFactory = Org.Neo4j.Bolt.runtime.BoltConnectionFactory;
	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineFactory = Org.Neo4j.Bolt.runtime.BoltStateMachineFactory;
	using ChunkDecoder = Org.Neo4j.Bolt.transport.pipeline.ChunkDecoder;
	using HouseKeeper = Org.Neo4j.Bolt.transport.pipeline.HouseKeeper;
	using MessageAccumulator = Org.Neo4j.Bolt.transport.pipeline.MessageAccumulator;
	using MessageDecoder = Org.Neo4j.Bolt.transport.pipeline.MessageDecoder;
	using BoltRequestMessageReaderV1 = Org.Neo4j.Bolt.v1.messaging.BoltRequestMessageReaderV1;
	using BoltResponseMessageWriterV1 = Org.Neo4j.Bolt.v1.messaging.BoltResponseMessageWriterV1;
	using Neo4jPackV1 = Org.Neo4j.Bolt.v1.messaging.Neo4jPackV1;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

	/// <summary>
	/// Bolt protocol V1. It hosts all the components that are specific to BoltV1
	/// </summary>
	public class BoltProtocolV1 : BoltProtocol
	{
		 public const long VERSION = 1;

		 private readonly Neo4jPack _neo4jPack;
		 private readonly BoltConnection _connection;
		 private readonly BoltRequestMessageReader _messageReader;

		 private readonly BoltChannel _channel;
		 private readonly LogService _logging;

		 public BoltProtocolV1( BoltChannel channel, BoltConnectionFactory connectionFactory, BoltStateMachineFactory stateMachineFactory, LogService logging )
		 {
			  this._channel = channel;
			  this._logging = logging;

			  BoltStateMachine stateMachine = stateMachineFactory.NewStateMachine( Version(), channel );
			  this._connection = connectionFactory.NewConnection( channel, stateMachine );

			  this._neo4jPack = CreatePack();
			  this._messageReader = CreateMessageReader( channel, _neo4jPack, _connection, logging );
		 }

		 /// <summary>
		 /// Install chunker, packstream, message reader, message handler, message encoder for protocol v1
		 /// </summary>
		 public override void Install()
		 {
			  ChannelPipeline pipeline = _channel.rawChannel().pipeline();

			  pipeline.addLast( new ChunkDecoder() );
			  pipeline.addLast( new MessageAccumulator() );
			  pipeline.addLast( new MessageDecoder( _neo4jPack, _messageReader, _logging ) );
			  pipeline.addLast( new HouseKeeper( _connection, _logging.getInternalLog( typeof( HouseKeeper ) ) ) );
		 }

		 protected internal virtual Neo4jPack CreatePack()
		 {
			  return new Neo4jPackV1();
		 }

		 public override long Version()
		 {
			  return VERSION;
		 }

		 protected internal virtual BoltRequestMessageReader CreateMessageReader( BoltChannel channel, Neo4jPack neo4jPack, BoltConnection connection, LogService logging )
		 {
			  BoltResponseMessageWriterV1 responseWriter = new BoltResponseMessageWriterV1( neo4jPack, connection.Output(), logging );
			  return new BoltRequestMessageReaderV1( connection, responseWriter, logging );
		 }
	}

}