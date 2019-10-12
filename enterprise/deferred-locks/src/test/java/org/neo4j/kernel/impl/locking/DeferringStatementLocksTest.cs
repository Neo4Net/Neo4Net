/*
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
namespace Org.Neo4j.Kernel.impl.locking
{
	using Test = org.junit.Test;

	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Org.Neo4j.Storageengine.Api.@lock.ResourceType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	public class DeferringStatementLocksTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseCorrectClientForImplicitAndExplicit()
		 public virtual void ShouldUseCorrectClientForImplicitAndExplicit()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Locks_Client client = mock(Locks_Client.class);
			  Locks_Client client = mock( typeof( Locks_Client ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DeferringStatementLocks statementLocks = new DeferringStatementLocks(client);
			  DeferringStatementLocks statementLocks = new DeferringStatementLocks( client );

			  // THEN
			  assertSame( client, statementLocks.Pessimistic() );
			  assertThat( statementLocks.Optimistic(), instanceOf(typeof(DeferringLockClient)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDoNothingWithClientWhenPreparingForCommitWithNoLocksAcquired()
		 public virtual void ShouldDoNothingWithClientWhenPreparingForCommitWithNoLocksAcquired()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Locks_Client client = mock(Locks_Client.class);
			  Locks_Client client = mock( typeof( Locks_Client ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DeferringStatementLocks statementLocks = new DeferringStatementLocks(client);
			  DeferringStatementLocks statementLocks = new DeferringStatementLocks( client );

			  // WHEN
			  statementLocks.PrepareForCommit( LockTracer.NONE );

			  // THEN
			  verify( client ).prepare();
			  verifyNoMoreInteractions( client );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrepareExplicitForCommitWhenLocksAcquire()
		 public virtual void ShouldPrepareExplicitForCommitWhenLocksAcquire()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Locks_Client client = mock(Locks_Client.class);
			  Locks_Client client = mock( typeof( Locks_Client ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DeferringStatementLocks statementLocks = new DeferringStatementLocks(client);
			  DeferringStatementLocks statementLocks = new DeferringStatementLocks( client );

			  // WHEN
			  statementLocks.Optimistic().acquireExclusive(LockTracer.NONE, ResourceTypes.Node, 1);
			  statementLocks.Optimistic().acquireExclusive(LockTracer.NONE, ResourceTypes.Relationship, 42);
			  verify( client, never() ).acquireExclusive(eq(LockTracer.NONE), any(typeof(ResourceType)), anyLong());
			  statementLocks.PrepareForCommit( LockTracer.NONE );

			  // THEN
			  verify( client ).prepare();
			  verify( client ).acquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );
			  verify( client ).acquireExclusive( LockTracer.NONE, ResourceTypes.Relationship, 42 );
			  verifyNoMoreInteractions( client );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopUnderlyingClient()
		 public virtual void ShouldStopUnderlyingClient()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Locks_Client client = mock(Locks_Client.class);
			  Locks_Client client = mock( typeof( Locks_Client ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DeferringStatementLocks statementLocks = new DeferringStatementLocks(client);
			  DeferringStatementLocks statementLocks = new DeferringStatementLocks( client );

			  // WHEN
			  statementLocks.Stop();

			  // THEN
			  verify( client ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseUnderlyingClient()
		 public virtual void ShouldCloseUnderlyingClient()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Locks_Client client = mock(Locks_Client.class);
			  Locks_Client client = mock( typeof( Locks_Client ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DeferringStatementLocks statementLocks = new DeferringStatementLocks(client);
			  DeferringStatementLocks statementLocks = new DeferringStatementLocks( client );

			  // WHEN
			  statementLocks.Close();

			  // THEN
			  verify( client ).close();
		 }
	}

}