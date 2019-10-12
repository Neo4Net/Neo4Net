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
namespace Neo4Net.Bolt.v3.runtime.integration
{
	using RegisterExtension = org.junit.jupiter.api.extension.RegisterExtension;

	using BoltTestUtil = Neo4Net.Bolt.testing.BoltTestUtil;
	using HelloMessage = Neo4Net.Bolt.v3.messaging.request.HelloMessage;
	using MapUtil = Neo4Net.Helpers.Collection.MapUtil;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

	internal class BoltStateMachineStateTestBase
	{
		 protected internal static readonly MapValue EmptyParams = VirtualValues.EMPTY_MAP;
		 protected internal const string USER_AGENT = "BoltConnectionIT/0.0";
		 protected internal static readonly BoltChannel BoltChannel = BoltTestUtil.newTestBoltChannel( "conn-v3-test-boltchannel-id" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RegisterExtension static final SessionExtension env = new SessionExtension();
		 internal static readonly SessionExtension Env = new SessionExtension();

		 protected internal virtual BoltStateMachineV3 NewStateMachine()
		 {
			  return ( BoltStateMachineV3 ) Env.newMachine( BoltProtocolV3.VERSION, BoltChannel );
		 }

		 protected internal static HelloMessage NewHelloMessage()
		 {
			  return new HelloMessage( MapUtil.map( "user_agent", USER_AGENT ) );
		 }
	}

}