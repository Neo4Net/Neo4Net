﻿using System;

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
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionFailureException = Neo4Net.GraphDb.TransactionFailureException;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using StandardV3_2 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_2;
	using StandardV3_4 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_4;
	using StoreUpgrader = Neo4Net.Kernel.impl.storemigration.StoreUpgrader;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using PointValue = Neo4Net.Values.Storable.PointValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static migration.RecordFormatMigrationIT.startDatabaseWithFormat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static migration.RecordFormatMigrationIT.startNonUpgradableDatabaseWithFormat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.arrayWithSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.pointValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) public class PointPropertiesRecordFormatIT
	public class PointPropertiesRecordFormatIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private Neo4Net.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failToCreatePointOnOldDatabase()
		 internal virtual void FailToCreatePointOnOldDatabase()
		 {
			  File storeDir = _testDirectory.storeDir();
			  IGraphDatabaseService nonUpgradedStore = startNonUpgradableDatabaseWithFormat( storeDir, StandardV3_2.NAME );
			  TransactionFailureException exception = assertThrows(typeof(TransactionFailureException), () =>
			  {
				using ( Transaction transaction = nonUpgradedStore.BeginTx() )
				{
					 Node node = nonUpgradedStore.CreateNode();
					 node.setProperty( "a", pointValue( Cartesian, 1.0, 2.0 ) );
					 transaction.success();
				}
			  });
			  assertEquals( "Current record format does not support POINT_PROPERTIES. Please upgrade your store to the format that support requested capability.", Exceptions.rootCause( exception ).Message );
			  nonUpgradedStore.Shutdown();

			  IGraphDatabaseService restartedOldFormatDatabase = startNonUpgradableDatabaseWithFormat( storeDir, StandardV3_2.NAME );
			  using ( Transaction transaction = restartedOldFormatDatabase.BeginTx() )
			  {
					Node node = restartedOldFormatDatabase.CreateNode();
					node.SetProperty( "c", "d" );
					transaction.success();
			  }
			  restartedOldFormatDatabase.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failToCreatePointArrayOnOldDatabase()
		 internal virtual void FailToCreatePointArrayOnOldDatabase()
		 {
			  File storeDir = _testDirectory.storeDir();
			  IGraphDatabaseService nonUpgradedStore = startNonUpgradableDatabaseWithFormat( storeDir, StandardV3_2.NAME );
			  PointValue point = pointValue( Cartesian, 1.0, 2.0 );
			  TransactionFailureException exception = assertThrows(typeof(TransactionFailureException), () =>
			  {
				using ( Transaction transaction = nonUpgradedStore.BeginTx() )
				{
					 Node node = nonUpgradedStore.CreateNode();
					 node.setProperty( "a", new PointValue[]{ point, point } );
					 transaction.success();
				}
			  });
			  assertEquals( "Current record format does not support POINT_PROPERTIES. Please upgrade your store to the format that support requested capability.", Exceptions.rootCause( exception ).Message );
			  nonUpgradedStore.Shutdown();

			  IGraphDatabaseService restartedOldFormatDatabase = startNonUpgradableDatabaseWithFormat( storeDir, StandardV3_2.NAME );
			  using ( Transaction transaction = restartedOldFormatDatabase.BeginTx() )
			  {
					Node node = restartedOldFormatDatabase.CreateNode();
					node.SetProperty( "c", "d" );
					transaction.success();
			  }
			  restartedOldFormatDatabase.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createPointPropertyOnLatestDatabase()
		 internal virtual void CreatePointPropertyOnLatestDatabase()
		 {
			  File storeDir = _testDirectory.storeDir();
			  Label pointNode = Label.label( "PointNode" );
			  string propertyKey = "a";
			  PointValue pointValue = pointValue( Cartesian, 1.0, 2.0 );

			  IGraphDatabaseService database = startDatabaseWithFormat( storeDir, Standard.LATEST_NAME );
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode( pointNode );
					node.SetProperty( propertyKey, pointValue );
					transaction.Success();
			  }
			  database.Shutdown();

			  IGraphDatabaseService restartedDatabase = startDatabaseWithFormat( storeDir, Standard.LATEST_NAME );
			  using ( Transaction ignored = restartedDatabase.BeginTx() )
			  {
					assertNotNull( restartedDatabase.FindNode( pointNode, propertyKey, pointValue ) );
			  }
			  restartedDatabase.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createPointArrayPropertyOnLatestDatabase()
		 internal virtual void CreatePointArrayPropertyOnLatestDatabase()
		 {
			  File storeDir = _testDirectory.storeDir();
			  Label pointNode = Label.label( "PointNode" );
			  string propertyKey = "a";
			  PointValue pointValue = pointValue( Cartesian, 1.0, 2.0 );

			  IGraphDatabaseService database = startDatabaseWithFormat( storeDir, Standard.LATEST_NAME );
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode( pointNode );
					node.SetProperty( propertyKey, new PointValue[]{ pointValue, pointValue } );
					transaction.Success();
			  }
			  database.Shutdown();

			  IGraphDatabaseService restartedDatabase = startDatabaseWithFormat( storeDir, Standard.LATEST_NAME );
			  using ( Transaction ignored = restartedDatabase.BeginTx() )
			  {
					using ( IResourceIterator<Node> nodes = restartedDatabase.FindNodes( pointNode ) )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 Node node = nodes.next();
						 PointValue[] points = ( PointValue[] ) node.GetProperty( propertyKey );
						 assertThat( points, arrayWithSize( 2 ) );
					}
			  }
			  restartedDatabase.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failToOpenStoreWithPointPropertyUsingOldFormat()
		 internal virtual void FailToOpenStoreWithPointPropertyUsingOldFormat()
		 {
			  File storeDir = _testDirectory.storeDir();
			  IGraphDatabaseService database = startDatabaseWithFormat( storeDir, StandardV3_4.NAME );
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode();
					node.SetProperty( "a", pointValue( Cartesian, 1.0, 2.0 ) );
					transaction.Success();
			  }
			  database.Shutdown();

			  Exception throwable = assertThrows( typeof( Exception ), () => startDatabaseWithFormat(storeDir, StandardV3_2.NAME) );
			  assertSame( typeof( StoreUpgrader.AttemptedDowngradeException ), Exceptions.rootCause( throwable ).GetType() );
		 }
	}

}