﻿/*
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
namespace Neo4Net.Bolt.v1
{
	using ChannelPipeline = io.netty.channel.ChannelPipeline;

	using BoltRequestMessageReader = Neo4Net.Bolt.messaging.BoltRequestMessageReader;
	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using BoltConnectionFactory = Neo4Net.Bolt.runtime.BoltConnectionFactory;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineFactory = Neo4Net.Bolt.runtime.BoltStateMachineFactory;
	using ChunkDecoder = Neo4Net.Bolt.transport.pipeline.ChunkDecoder;
	using HouseKeeper = Neo4Net.Bolt.transport.pipeline.HouseKeeper;
	using MessageAccumulator = Neo4Net.Bolt.transport.pipeline.MessageAccumulator;
	using MessageDecoder = Neo4Net.Bolt.transport.pipeline.MessageDecoder;
	using BoltRequestMessageReaderV1 = Neo4Net.Bolt.v1.messaging.BoltRequestMessageReaderV1;
	using BoltResponseMessageWriterV1 = Neo4Net.Bolt.v1.messaging.BoltResponseMessageWriterV1;
	using Neo4NetPackV1 = Neo4Net.Bolt.v1.messaging.Neo4NetPackV1;
	using LogService = Neo4Net.Logging.Internal.LogService;

	/// <summary>
	/// Bolt protocol V1. It hosts all the components that are specific to BoltV1
	/// </summary>
	public class BoltProtocolV1 : BoltProtocol
	{
		 public const long VERSION = 1;

		 private readonly Neo4NetPack _Neo4NetPack;
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

			  this._Neo4NetPack = CreatePack();
			  this._messageReader = CreateMessageReader( channel, _Neo4NetPack, _connection, logging );
		 }

		 /// <summary>
		 /// Install chunker, packstream, message reader, message handler, message encoder for protocol v1
		 /// </summary>
		 public override void Install()
		 {
			  ChannelPipeline pipeline = _channel.rawChannel().pipeline();

			  pipeline.addLast( new ChunkDecoder() );
			  pipeline.addLast( new MessageAccumulator() );
			  pipeline.addLast( new MessageDecoder( _Neo4NetPack, _messageReader, _logging ) );
			  pipeline.addLast( new HouseKeeper( _connection, _logging.getInternalLog( typeof( HouseKeeper ) ) ) );
		 }

		 protected internal virtual Neo4NetPack CreatePack()
		 {
			  return new Neo4NetPackV1();
		 }

		 public override long Version()
		 {
			  return VERSION;
		 }

		 protected internal virtual BoltRequestMessageReader CreateMessageReader( BoltChannel channel, Neo4NetPack Neo4NetPack, BoltConnection connection, LogService logging )
		 {
			  BoltResponseMessageWriterV1 responseWriter = new BoltResponseMessageWriterV1( Neo4NetPack, connection.Output(), logging );
			  return new BoltRequestMessageReaderV1( connection, responseWriter, logging );
		 }
	}

}