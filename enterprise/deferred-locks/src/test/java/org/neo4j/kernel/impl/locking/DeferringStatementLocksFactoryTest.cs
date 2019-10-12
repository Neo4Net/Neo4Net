using System;

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

	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.DeferringStatementLocksFactory.deferred_locks_enabled;

	public class DeferringStatementLocksFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void initializeThrowsForNullLocks()
		 public virtual void InitializeThrowsForNullLocks()
		 {
			  DeferringStatementLocksFactory factory = new DeferringStatementLocksFactory();
			  try
			  {
					factory.Initialize( null, Config.defaults() );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.NullReferenceException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void initializeThrowsForNullConfig()
		 public virtual void InitializeThrowsForNullConfig()
		 {
			  DeferringStatementLocksFactory factory = new DeferringStatementLocksFactory();
			  try
			  {
					factory.Initialize( mock( typeof( Locks ) ), null );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.NullReferenceException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void newInstanceThrowsWhenNotInitialized()
		 public virtual void NewInstanceThrowsWhenNotInitialized()
		 {
			  DeferringStatementLocksFactory factory = new DeferringStatementLocksFactory();
			  try
			  {
					factory.NewInstance();
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void newInstanceCreatesSimpleLocksWhenConfigNotSet()
		 public virtual void NewInstanceCreatesSimpleLocksWhenConfigNotSet()
		 {
			  Locks locks = mock( typeof( Locks ) );
			  Locks_Client client = mock( typeof( Locks_Client ) );
			  when( locks.NewClient() ).thenReturn(client);

			  Config config = Config.defaults( deferred_locks_enabled, Settings.FALSE );

			  DeferringStatementLocksFactory factory = new DeferringStatementLocksFactory();
			  factory.Initialize( locks, config );

			  StatementLocks statementLocks = factory.NewInstance();

			  assertThat( statementLocks, instanceOf( typeof( SimpleStatementLocks ) ) );
			  assertSame( client, statementLocks.Optimistic() );
			  assertSame( client, statementLocks.Pessimistic() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void newInstanceCreatesDeferredLocksWhenConfigSet()
		 public virtual void NewInstanceCreatesDeferredLocksWhenConfigSet()
		 {
			  Locks locks = mock( typeof( Locks ) );
			  Locks_Client client = mock( typeof( Locks_Client ) );
			  when( locks.NewClient() ).thenReturn(client);

			  Config config = Config.defaults( deferred_locks_enabled, Settings.TRUE );

			  DeferringStatementLocksFactory factory = new DeferringStatementLocksFactory();
			  factory.Initialize( locks, config );

			  StatementLocks statementLocks = factory.NewInstance();

			  assertThat( statementLocks, instanceOf( typeof( DeferringStatementLocks ) ) );
			  assertThat( statementLocks.Optimistic(), instanceOf(typeof(DeferringLockClient)) );
			  assertSame( client, statementLocks.Pessimistic() );
		 }
	}

}