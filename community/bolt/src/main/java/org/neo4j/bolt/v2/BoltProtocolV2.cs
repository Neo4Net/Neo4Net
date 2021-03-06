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
namespace Org.Neo4j.Bolt.v2
{
	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using BoltConnectionFactory = Org.Neo4j.Bolt.runtime.BoltConnectionFactory;
	using BoltStateMachineFactory = Org.Neo4j.Bolt.runtime.BoltStateMachineFactory;
	using BoltProtocolV1 = Org.Neo4j.Bolt.v1.BoltProtocolV1;
	using Neo4jPackV2 = Org.Neo4j.Bolt.v2.messaging.Neo4jPackV2;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

	public class BoltProtocolV2 : BoltProtocolV1
	{
		 public new const long VERSION = 2;

		 public BoltProtocolV2( BoltChannel channel, BoltConnectionFactory connectionFactory, BoltStateMachineFactory machineFactory, LogService logging ) : base( channel, connectionFactory, machineFactory, logging )
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
	}

}