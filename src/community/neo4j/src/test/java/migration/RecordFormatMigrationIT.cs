using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Migration
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using StandardV3_2 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_2;
	using StandardV3_4 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_4;
	using StoreUpgrader = Neo4Net.Kernel.impl.storemigration.StoreUpgrader;
	using UpgradeNotAllowedByConfigurationException = Neo4Net.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.allow_upgrade;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.record_format;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class RecordFormatMigrationIT
	internal class RecordFormatMigrationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;
		 private File _storeDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _storeDir = _testDirectory.storeDir();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failToDowngradeFormatWhenUpgradeNotAllowed()
		 internal virtual void FailToDowngradeFormatWhenUpgradeNotAllowed()
		 {
			  GraphDatabaseService database = StartDatabaseWithFormatUnspecifiedUpgrade( _storeDir, StandardV3_4.NAME );
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode();
					node.SetProperty( "a", "b" );
					transaction.Success();
			  }
			  database.Shutdown();
			  Exception throwable = assertThrows( typeof( Exception ), () => StartDatabaseWithFormatUnspecifiedUpgrade(_storeDir, StandardV3_2.NAME) );
			  assertSame( typeof( UpgradeNotAllowedByConfigurationException ), Exceptions.rootCause( throwable ).GetType() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failToDowngradeFormatWheUpgradeAllowed()
		 internal virtual void FailToDowngradeFormatWheUpgradeAllowed()
		 {
			  GraphDatabaseService database = StartDatabaseWithFormatUnspecifiedUpgrade( _storeDir, StandardV3_4.NAME );
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode();
					node.SetProperty( "a", "b" );
					transaction.Success();
			  }
			  database.Shutdown();
			  Exception throwable = assertThrows( typeof( Exception ), () => (new GraphDatabaseFactory()).newEmbeddedDatabaseBuilder(_storeDir).setConfig(record_format, StandardV3_2.NAME).setConfig(allow_upgrade, Settings.TRUE).newGraphDatabase() );
			  assertSame( typeof( StoreUpgrader.AttemptedDowngradeException ), Exceptions.rootCause( throwable ).GetType() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void skipMigrationIfFormatSpecifiedInConfig()
		 internal virtual void SkipMigrationIfFormatSpecifiedInConfig()
		 {
			  GraphDatabaseService database = StartDatabaseWithFormatUnspecifiedUpgrade( _storeDir, StandardV3_2.NAME );
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode();
					node.SetProperty( "a", "b" );
					transaction.Success();
			  }
			  database.Shutdown();

			  GraphDatabaseAPI nonUpgradedStore = ( GraphDatabaseAPI ) StartDatabaseWithFormatUnspecifiedUpgrade( _storeDir, StandardV3_2.NAME );
			  RecordStorageEngine storageEngine = nonUpgradedStore.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) );
			  assertEquals( StandardV3_2.NAME, storageEngine.TestAccessNeoStores().RecordFormats.name() );
			  nonUpgradedStore.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void skipMigrationIfStoreFormatNotSpecifiedButIsAvailableInRuntime()
		 internal virtual void SkipMigrationIfStoreFormatNotSpecifiedButIsAvailableInRuntime()
		 {
			  GraphDatabaseService database = StartDatabaseWithFormatUnspecifiedUpgrade( _storeDir, StandardV3_2.NAME );
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode();
					node.SetProperty( "a", "b" );
					transaction.Success();
			  }
			  database.Shutdown();

			  GraphDatabaseAPI nonUpgradedStore = ( GraphDatabaseAPI ) ( new GraphDatabaseFactory() ).newEmbeddedDatabase(_storeDir);
			  RecordStorageEngine storageEngine = nonUpgradedStore.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) );
			  assertEquals( StandardV3_2.NAME, storageEngine.TestAccessNeoStores().RecordFormats.name() );
			  nonUpgradedStore.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void latestRecordNotMigratedWhenFormatBumped()
		 internal virtual void LatestRecordNotMigratedWhenFormatBumped()
		 {
			  GraphDatabaseService database = StartDatabaseWithFormatUnspecifiedUpgrade( _storeDir, StandardV3_2.NAME );
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode();
					node.SetProperty( "a", "b" );
					transaction.Success();
			  }
			  database.Shutdown();

			  Exception exception = assertThrows( typeof( Exception ), () => StartDatabaseWithFormatUnspecifiedUpgrade(_storeDir, Standard.LATEST_NAME) );
			  assertSame( typeof( UpgradeNotAllowedByConfigurationException ), Exceptions.rootCause( exception ).GetType() );
		 }

		 private static GraphDatabaseService StartDatabaseWithFormatUnspecifiedUpgrade( File storeDir, string formatName )
		 {
			  return ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(record_format, formatName).newGraphDatabase();
		 }

		 internal static GraphDatabaseService StartNonUpgradableDatabaseWithFormat( File storeDir, string formatName )
		 {
			  return ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(record_format, formatName).setConfig(allow_upgrade, FALSE).newGraphDatabase();
		 }

		 internal static GraphDatabaseService StartDatabaseWithFormat( File storeDir, string formatName )
		 {
			  return ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(record_format, formatName).setConfig(allow_upgrade, TRUE).newGraphDatabase();
		 }
	}

}