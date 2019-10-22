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
namespace Neo4Net.Bolt.v2
{
	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using BoltConnectionFactory = Neo4Net.Bolt.runtime.BoltConnectionFactory;
	using BoltStateMachineFactory = Neo4Net.Bolt.runtime.BoltStateMachineFactory;
	using BoltProtocolV1 = Neo4Net.Bolt.v1.BoltProtocolV1;
	using Neo4NetPackV2 = Neo4Net.Bolt.v2.messaging.Neo4NetPackV2;
	using LogService = Neo4Net.Logging.Internal.LogService;

	public class BoltProtocolV2 : BoltProtocolV1
	{
		 public new const long VERSION = 2;

		 public BoltProtocolV2( BoltChannel channel, BoltConnectionFactory connectionFactory, BoltStateMachineFactory machineFactory, LogService logging ) : base( channel, connectionFactory, machineFactory, logging )
		 {
		 }

		 protected internal override Neo4NetPack CreatePack()
		 {
			  return new Neo4NetPackV2();
		 }

		 public override long Version()
		 {
			  return VERSION;
		 }
	}

}