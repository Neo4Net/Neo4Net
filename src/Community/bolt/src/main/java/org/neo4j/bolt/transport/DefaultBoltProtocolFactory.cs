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
namespace Neo4Net.Bolt.transport
{
	using BoltConnectionFactory = Neo4Net.Bolt.runtime.BoltConnectionFactory;
	using BoltStateMachineFactory = Neo4Net.Bolt.runtime.BoltStateMachineFactory;
	using BoltProtocolV1 = Neo4Net.Bolt.v1.BoltProtocolV1;
	using BoltProtocolV2 = Neo4Net.Bolt.v2.BoltProtocolV2;
	using BoltProtocolV3 = Neo4Net.Bolt.v3.BoltProtocolV3;
	using LogService = Neo4Net.Logging.@internal.LogService;

	public class DefaultBoltProtocolFactory : BoltProtocolFactory
	{
		 private readonly BoltConnectionFactory _connectionFactory;
		 private readonly LogService _logService;
		 private readonly BoltStateMachineFactory _stateMachineFactory;

		 public DefaultBoltProtocolFactory( BoltConnectionFactory connectionFactory, BoltStateMachineFactory stateMachineFactory, LogService logService )
		 {
			  this._connectionFactory = connectionFactory;
			  this._stateMachineFactory = stateMachineFactory;
			  this._logService = logService;
		 }

		 public override BoltProtocol Create( long protocolVersion, BoltChannel channel )
		 {
			  if ( protocolVersion == BoltProtocolV1.VERSION )
			  {
					return new BoltProtocolV1( channel, _connectionFactory, _stateMachineFactory, _logService );
			  }
			  else if ( protocolVersion == BoltProtocolV2.VERSION )
			  {
					return new BoltProtocolV2( channel, _connectionFactory, _stateMachineFactory, _logService );
			  }
			  else if ( protocolVersion == BoltProtocolV3.VERSION )
			  {
					return new BoltProtocolV3( channel, _connectionFactory, _stateMachineFactory, _logService );
			  }
			  else
			  {
					return null;
			  }
		 }
	}

}