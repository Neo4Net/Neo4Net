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
namespace Org.Neo4j.Bolt.v3
{
	using Test = org.junit.jupiter.api.Test;

	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using BoltConnection = Org.Neo4j.Bolt.runtime.BoltConnection;
	using BoltStateMachineFactory = Org.Neo4j.Bolt.runtime.BoltStateMachineFactory;
	using Neo4jPackV2 = Org.Neo4j.Bolt.v2.messaging.Neo4jPackV2;
	using BoltRequestMessageReaderV3 = Org.Neo4j.Bolt.v3.messaging.BoltRequestMessageReaderV3;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	internal class BoltProtocolV3Test
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreatePackForBoltV3() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCreatePackForBoltV3()
		 {
			  BoltProtocolV3 protocolV3 = new BoltProtocolV3( mock( typeof( BoltChannel ) ), ( ch, st ) => mock( typeof( BoltConnection ) ), mock( typeof( BoltStateMachineFactory ) ), NullLogService.Instance );

			  assertThat( protocolV3.CreatePack(), instanceOf(typeof(Neo4jPackV2)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldVersionReturnBoltV3() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldVersionReturnBoltV3()
		 {
			  BoltProtocolV3 protocolV3 = new BoltProtocolV3( mock( typeof( BoltChannel ) ), ( ch, st ) => mock( typeof( BoltConnection ) ), mock( typeof( BoltStateMachineFactory ) ), NullLogService.Instance );

			  assertThat( protocolV3.Version(), equalTo(3L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateMessageReaderForBoltV3() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCreateMessageReaderForBoltV3()
		 {
			  BoltProtocolV3 protocolV3 = new BoltProtocolV3( mock( typeof( BoltChannel ) ), ( ch, st ) => mock( typeof( BoltConnection ) ), mock( typeof( BoltStateMachineFactory ) ), NullLogService.Instance );

			  assertThat( protocolV3.CreateMessageReader( mock( typeof( BoltChannel ) ), mock( typeof( Neo4jPack ) ), mock( typeof( BoltConnection ) ), NullLogService.Instance ), instanceOf( typeof( BoltRequestMessageReaderV3 ) ) );
		 }
	}

}