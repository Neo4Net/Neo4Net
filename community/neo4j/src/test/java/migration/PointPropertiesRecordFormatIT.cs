using System;

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
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Org.Neo4j.Graphdb;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using TransactionFailureException = Org.Neo4j.Graphdb.TransactionFailureException;
	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using StandardV3_2 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV3_2;
	using StandardV3_4 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV3_4;
	using StoreUpgrader = Org.Neo4j.Kernel.impl.storemigration.StoreUpgrader;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;

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
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) public class PointPropertiesRecordFormatIT
	public class PointPropertiesRecordFormatIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failToCreatePointOnOldDatabase()
		 internal virtual void FailToCreatePointOnOldDatabase()
		 {
			  File storeDir = _testDirectory.storeDir();
			  GraphDatabaseService nonUpgradedStore = startNonUpgradableDatabaseWithFormat( storeDir, StandardV3_2.NAME );
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

			  GraphDatabaseService restartedOldFormatDatabase = startNonUpgradableDatabaseWithFormat( storeDir, StandardV3_2.NAME );
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
			  GraphDatabaseService nonUpgradedStore = startNonUpgradableDatabaseWithFormat( storeDir, StandardV3_2.NAME );
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

			  GraphDatabaseService restartedOldFormatDatabase = startNonUpgradableDatabaseWithFormat( storeDir, StandardV3_2.NAME );
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

			  GraphDatabaseService database = startDatabaseWithFormat( storeDir, Standard.LATEST_NAME );
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode( pointNode );
					node.SetProperty( propertyKey, pointValue );
					transaction.Success();
			  }
			  database.Shutdown();

			  GraphDatabaseService restartedDatabase = startDatabaseWithFormat( storeDir, Standard.LATEST_NAME );
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

			  GraphDatabaseService database = startDatabaseWithFormat( storeDir, Standard.LATEST_NAME );
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode( pointNode );
					node.SetProperty( propertyKey, new PointValue[]{ pointValue, pointValue } );
					transaction.Success();
			  }
			  database.Shutdown();

			  GraphDatabaseService restartedDatabase = startDatabaseWithFormat( storeDir, Standard.LATEST_NAME );
			  using ( Transaction ignored = restartedDatabase.BeginTx() )
			  {
					using ( ResourceIterator<Node> nodes = restartedDatabase.FindNodes( pointNode ) )
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
			  GraphDatabaseService database = startDatabaseWithFormat( storeDir, StandardV3_4.NAME );
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