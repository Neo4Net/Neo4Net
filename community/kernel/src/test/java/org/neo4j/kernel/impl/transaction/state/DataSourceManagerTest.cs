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
namespace Org.Neo4j.Kernel.impl.transaction.state
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class DataSourceManagerTest
	internal class DataSourceManagerTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCallListenersOnStart()
		 internal virtual void ShouldCallListenersOnStart()
		 {
			  // given
			  DataSourceManager manager = CreateDataSourceManager();
			  DataSourceManager.Listener listener = mock( typeof( DataSourceManager.Listener ) );
			  manager.Register( mock( typeof( NeoStoreDataSource ) ) );
			  manager.AddListener( listener );

			  // when
			  manager.Start();

			  // then
			  verify( listener ).registered( any( typeof( NeoStoreDataSource ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCallListenersWhenAddedIfManagerAlreadyStarted()
		 internal virtual void ShouldCallListenersWhenAddedIfManagerAlreadyStarted()
		 {
			  // given
			  DataSourceManager manager = CreateDataSourceManager();
			  DataSourceManager.Listener listener = mock( typeof( DataSourceManager.Listener ) );
			  manager.Register( mock( typeof( NeoStoreDataSource ) ) );
			  manager.Start();

			  // when
			  manager.AddListener( listener );

			  // then
			  verify( listener ).registered( any( typeof( NeoStoreDataSource ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCallListenersOnDataSourceRegistrationIfManagerAlreadyStarted()
		 internal virtual void ShouldCallListenersOnDataSourceRegistrationIfManagerAlreadyStarted()
		 {
			  // given
			  DataSourceManager manager = CreateDataSourceManager();
			  DataSourceManager.Listener listener = mock( typeof( DataSourceManager.Listener ) );
			  manager.AddListener( listener );
			  manager.Start();

			  // when
			  manager.Register( mock( typeof( NeoStoreDataSource ) ) );

			  // then
			  verify( listener ).registered( any( typeof( NeoStoreDataSource ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportMultipleStartStopCycles() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldSupportMultipleStartStopCycles()
		 {
			  // given
			  DataSourceManager manager = CreateDataSourceManager();
			  NeoStoreDataSource dataSource = mock( typeof( NeoStoreDataSource ) );
			  manager.Register( dataSource );
			  manager.Init();

			  // when
			  manager.Start();
			  manager.Stop();
			  manager.Start();

			  // then
			  verify( dataSource, times( 2 ) ).start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void provideAccessOnlyToActiveDatabase()
		 internal virtual void ProvideAccessOnlyToActiveDatabase()
		 {
			  DataSourceManager manager = CreateDataSourceManager();
			  NeoStoreDataSource dataSource1 = mock( typeof( NeoStoreDataSource ) );
			  NeoStoreDataSource dataSource2 = mock( typeof( NeoStoreDataSource ) );
			  when( dataSource1.DatabaseLayout ).thenReturn( _testDirectory.databaseLayout() );
			  when( dataSource2.DatabaseLayout ).thenReturn( _testDirectory.databaseLayout( "somethingElse" ) );
			  manager.Register( dataSource1 );
			  manager.Register( dataSource2 );

			  assertEquals( dataSource1, manager.DataSource );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void illegalStateWhenActiveDatabaseNotFound()
		 internal virtual void IllegalStateWhenActiveDatabaseNotFound()
		 {
			  DataSourceManager manager = CreateDataSourceManager();
			  assertThrows( typeof( System.InvalidOperationException ), manager.getDataSource );
		 }

		 private static DataSourceManager CreateDataSourceManager()
		 {
			  return new DataSourceManager( Config.defaults() );
		 }
	}

}