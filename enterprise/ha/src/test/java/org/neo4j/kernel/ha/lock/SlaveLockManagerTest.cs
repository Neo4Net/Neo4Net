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
namespace Org.Neo4j.Kernel.ha.@lock
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RequestContextFactory = Org.Neo4j.Kernel.ha.com.RequestContextFactory;
	using Master = Org.Neo4j.Kernel.ha.com.master.Master;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using CommunityLockManger = Org.Neo4j.Kernel.impl.locking.community.CommunityLockManger;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using Clocks = Org.Neo4j.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Suppliers.singleton;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.DEFAULT_DATABASE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLog.getInstance;

	public class SlaveLockManagerTest
	{
		 private RequestContextFactory _requestContextFactory;
		 private Master _master;
		 private DatabaseAvailabilityGuard _databaseAvailabilityGuard;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _requestContextFactory = new RequestContextFactory( 1, singleton( mock( typeof( TransactionIdStore ) ) ) );
			  _master = mock( typeof( Master ) );
			  _databaseAvailabilityGuard = new DatabaseAvailabilityGuard( DEFAULT_DATABASE_NAME, Clocks.systemClock(), Instance );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shutsDownLocalLocks()
		 public virtual void ShutsDownLocalLocks()
		 {
			  Locks localLocks = mock( typeof( Locks ) );
			  SlaveLockManager slaveLockManager = NewSlaveLockManager( localLocks );

			  slaveLockManager.Close();

			  verify( localLocks ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doesNotCreateClientsAfterShutdown()
		 public virtual void DoesNotCreateClientsAfterShutdown()
		 {
			  SlaveLockManager slaveLockManager = NewSlaveLockManager( new CommunityLockManger( Config.defaults(), Clocks.systemClock() ) );

			  assertNotNull( slaveLockManager.NewClient() );

			  slaveLockManager.Close();

			  try
			  {
					slaveLockManager.NewClient();
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

		 private SlaveLockManager NewSlaveLockManager( Locks localLocks )
		 {
			  return new SlaveLockManager( localLocks, _requestContextFactory, _master, _databaseAvailabilityGuard, NullLogProvider.Instance, Config.defaults() );
		 }
	}

}