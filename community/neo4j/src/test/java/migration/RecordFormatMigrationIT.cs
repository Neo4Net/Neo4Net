﻿using System;

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
namespace Migration
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseFactory = Org.Neo4j.Graphdb.factory.GraphDatabaseFactory;
	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using RecordStorageEngine = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using StandardV3_2 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV3_2;
	using StandardV3_4 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV3_4;
	using StoreUpgrader = Org.Neo4j.Kernel.impl.storemigration.StoreUpgrader;
	using UpgradeNotAllowedByConfigurationException = Org.Neo4j.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

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