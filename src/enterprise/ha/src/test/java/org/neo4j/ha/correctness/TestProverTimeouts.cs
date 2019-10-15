﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.ha.correctness
{
	using Test = org.junit.Test;

	using Neo4Net.cluster.com.message;
	using ProposerMessage = Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.ProposerMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class TestProverTimeouts
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsShouldBeLogicalAndNotExact() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void EqualsShouldBeLogicalAndNotExact()
		 {
			  // Given
			  ProverTimeouts timeouts1 = new ProverTimeouts( new URI( "http://asd" ) );
			  ProverTimeouts timeouts2 = new ProverTimeouts( new URI( "http://asd" ) );

			  timeouts1.SetTimeout( "a", Message.Internal( ProposerMessage.join ) );
			  timeouts1.SetTimeout( "b", Message.Internal( ProposerMessage.join ) );
			  timeouts1.SetTimeout( "c", Message.Internal( ProposerMessage.join ) );

			  timeouts2.SetTimeout( "b", Message.Internal( ProposerMessage.join ) );
			  timeouts2.SetTimeout( "c", Message.Internal( ProposerMessage.join ) );

			  // When
			  timeouts1.CancelTimeout( "a" );

			  // Then
			  assertEquals( timeouts1, timeouts2 );
		 }

	}

}