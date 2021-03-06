﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.causalclustering.discovery.procedures
{
	using Test = org.junit.Test;

	using RaftMachine = Org.Neo4j.causalclustering.core.consensus.RaftMachine;
	using Org.Neo4j.Collection;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asList;

	public class RoleProcedureTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnLeader()
		 {
			  // given
			  RaftMachine raft = mock( typeof( RaftMachine ) );
			  when( raft.Leader ).thenReturn( true );
			  RoleProcedure proc = new CoreRoleProcedure( raft );

			  // when
			  RawIterator<object[], ProcedureException> result = proc.Apply( null, null, null );

			  // then
			  assertEquals( RoleInfo.LEADER.name(), Single(result)[0] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFollower() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnFollower()
		 {
			  // given
			  RaftMachine raft = mock( typeof( RaftMachine ) );
			  when( raft.Leader ).thenReturn( false );
			  RoleProcedure proc = new CoreRoleProcedure( raft );

			  // when
			  RawIterator<object[], ProcedureException> result = proc.Apply( null, null, null );

			  // then
			  assertEquals( RoleInfo.FOLLOWER.name(), Single(result)[0] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnReadReplica() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnReadReplica()
		 {
			  // given
			  RoleProcedure proc = new ReadReplicaRoleProcedure();

			  // when
			  RawIterator<object[], ProcedureException> result = proc.Apply( null, null, null );

			  // then
			  assertEquals( RoleInfo.READ_REPLICA.name(), Single(result)[0] );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Object[] single(org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> result) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private object[] Single( RawIterator<object[], ProcedureException> result )
		 {
			  return Iterators.single( asList( result ).GetEnumerator() );
		 }
	}

}