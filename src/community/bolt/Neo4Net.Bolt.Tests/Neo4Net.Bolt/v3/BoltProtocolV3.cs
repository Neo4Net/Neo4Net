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
namespace Neo4Net.Bolt.v3
{
	using BoltRequestMessageReader = Neo4Net.Bolt.messaging.BoltRequestMessageReader;
	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using BoltConnectionFactory = Neo4Net.Bolt.runtime.BoltConnectionFactory;
	using BoltStateMachineFactory = Neo4Net.Bolt.runtime.BoltStateMachineFactory;
	using BoltProtocolV1 = Neo4Net.Bolt.v1.BoltProtocolV1;
	using BoltResponseMessageWriterV1 = Neo4Net.Bolt.v1.messaging.BoltResponseMessageWriterV1;
	using Neo4jPackV2 = Neo4Net.Bolt.v2.messaging.Neo4jPackV2;
	using BoltRequestMessageReaderV3 = Neo4Net.Bolt.v3.messaging.BoltRequestMessageReaderV3;
	using LogService = Neo4Net.Logging.@internal.LogService;

	/// <summary>
	/// Bolt protocol V3. It hosts all the components that are specific to BoltV3
	/// </summary>
	public class BoltProtocolV3 : BoltProtocolV1
	{
		 public new const long VERSION = 3;

		 public BoltProtocolV3( BoltChannel channel, BoltConnectionFactory connectionFactory, BoltStateMachineFactory stateMachineFactory, LogService logging ) : base( channel, connectionFactory, stateMachineFactory, logging )
		 {
		 }

		 protected internal override Neo4jPack CreatePack()
		 {
			  return new Neo4jPackV2();
		 }

		 public override long Version()
		 {
			  return VERSION;
		 }

		 protected internal override BoltRequestMessageReader CreateMessageReader( BoltChannel channel, Neo4jPack neo4jPack, BoltConnection connection, LogService logging )
		 {
			  BoltResponseMessageWriterV1 responseWriter = new BoltResponseMessageWriterV1( neo4jPack, connection.Output(), logging );
			  return new BoltRequestMessageReaderV3( connection, responseWriter, logging );
		 }
	}

}