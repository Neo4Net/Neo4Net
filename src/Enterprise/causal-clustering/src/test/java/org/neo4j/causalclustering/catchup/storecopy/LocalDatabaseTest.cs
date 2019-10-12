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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;

	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using NullLog = Neo4Net.Logging.NullLog;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.DEFAULT_DATABASE_NAME;

	public class LocalDatabaseTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void availabilityGuardRaisedOnCreation()
		 public virtual void AvailabilityGuardRaisedOnCreation()
		 {
			  DatabaseAvailabilityGuard guard = NewAvailabilityGuard();
			  assertTrue( guard.Available );
			  LocalDatabase localDatabase = NewLocalDatabase( guard );

			  assertNotNull( localDatabase );
			  AssertDatabaseIsStoppedAndUnavailable( guard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void availabilityGuardDroppedOnStart()
		 public virtual void AvailabilityGuardDroppedOnStart()
		 {
			  AvailabilityGuard guard = NewAvailabilityGuard();
			  assertTrue( guard.Available );

			  LocalDatabase localDatabase = NewLocalDatabase( guard );
			  assertFalse( guard.Available );

			  localDatabase.Start();
			  assertTrue( guard.Available );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void availabilityGuardRaisedOnStop() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AvailabilityGuardRaisedOnStop()
		 {
			  DatabaseAvailabilityGuard guard = NewAvailabilityGuard();
			  assertTrue( guard.Available );

			  LocalDatabase localDatabase = NewLocalDatabase( guard );
			  assertFalse( guard.Available );

			  localDatabase.Start();
			  assertTrue( guard.Available );

			  localDatabase.Stop();
			  AssertDatabaseIsStoppedAndUnavailable( guard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void availabilityGuardRaisedOnStopForStoreCopy() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AvailabilityGuardRaisedOnStopForStoreCopy()
		 {
			  DatabaseAvailabilityGuard guard = NewAvailabilityGuard();
			  assertTrue( guard.Available );

			  LocalDatabase localDatabase = NewLocalDatabase( guard );
			  assertFalse( guard.Available );

			  localDatabase.Start();
			  assertTrue( guard.Available );

			  localDatabase.StopForStoreCopy();
			  AssertDatabaseIsStoppedForStoreCopyAndUnavailable( guard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void availabilityGuardRaisedBeforeDataSourceManagerIsStopped() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AvailabilityGuardRaisedBeforeDataSourceManagerIsStopped()
		 {
			  AvailabilityGuard guard = mock( typeof( DatabaseAvailabilityGuard ) );
			  DataSourceManager dataSourceManager = mock( typeof( DataSourceManager ) );

			  LocalDatabase localDatabase = NewLocalDatabase( guard, dataSourceManager );
			  localDatabase.Stop();

			  InOrder inOrder = inOrder( guard, dataSourceManager );
			  // guard should be raised twice - once during construction and once during stop
			  inOrder.verify( guard, times( 2 ) ).require( any() );
			  inOrder.verify( dataSourceManager ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void availabilityGuardRaisedBeforeDataSourceManagerIsStoppedForStoreCopy() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AvailabilityGuardRaisedBeforeDataSourceManagerIsStoppedForStoreCopy()
		 {
			  AvailabilityGuard guard = mock( typeof( DatabaseAvailabilityGuard ) );
			  DataSourceManager dataSourceManager = mock( typeof( DataSourceManager ) );

			  LocalDatabase localDatabase = NewLocalDatabase( guard, dataSourceManager );
			  localDatabase.StopForStoreCopy();

			  InOrder inOrder = inOrder( guard, dataSourceManager );
			  // guard should be raised twice - once during construction and once during stop
			  inOrder.verify( guard, times( 2 ) ).require( any() );
			  inOrder.verify( dataSourceManager ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotRestartServicesIfAlreadyStarted()
		 public virtual void DoNotRestartServicesIfAlreadyStarted()
		 {
			  DataSourceManager dataSourceManager = mock( typeof( DataSourceManager ) );
			  LocalDatabase localDatabase = NewLocalDatabase( NewAvailabilityGuard(), dataSourceManager );

			  localDatabase.Start();

			  verify( dataSourceManager ).start();
			  reset( dataSourceManager );

			  localDatabase.Start();
			  localDatabase.Start();

			  verify( dataSourceManager, never() ).start();
		 }

		 private static LocalDatabase NewLocalDatabase( AvailabilityGuard databaseAvailabilityGuard )
		 {
			  return NewLocalDatabase( databaseAvailabilityGuard, mock( typeof( DataSourceManager ) ) );
		 }

		 private static LocalDatabase NewLocalDatabase( AvailabilityGuard databaseAvailabilityGuard, DataSourceManager dataSourceManager )
		 {
			  return new LocalDatabase( mock( typeof( DatabaseLayout ) ), mock( typeof( StoreFiles ) ), mock( typeof( LogFiles ) ), dataSourceManager, () => mock(typeof(DatabaseHealth)), databaseAvailabilityGuard, NullLogProvider.Instance );
		 }

		 private static DatabaseAvailabilityGuard NewAvailabilityGuard()
		 {
			  return new DatabaseAvailabilityGuard( DEFAULT_DATABASE_NAME, Clock.systemUTC(), NullLog.Instance );
		 }

		 private static void AssertDatabaseIsStoppedAndUnavailable( DatabaseAvailabilityGuard guard )
		 {
			  assertFalse( guard.Available );
			  assertThat( guard.DescribeWhoIsBlocking(), containsString("Database is stopped") );
		 }

		 private static void AssertDatabaseIsStoppedForStoreCopyAndUnavailable( DatabaseAvailabilityGuard guard )
		 {
			  assertFalse( guard.Available );
			  assertThat( guard.DescribeWhoIsBlocking(), containsString("Database is stopped to copy store") );
		 }
	}

}