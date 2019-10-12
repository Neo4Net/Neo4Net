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


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using StandardV3_2 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_2;
	using StandardV3_4 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_4;
	using StoreUpgrader = Neo4Net.Kernel.impl.storemigration.StoreUpgrader;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DateValue = Neo4Net.Values.Storable.DateValue;

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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class TemporalPropertiesRecordFormatIT
	internal class TemporalPropertiesRecordFormatIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failToCreateDateOnOldDatabase()
		 internal virtual void FailToCreateDateOnOldDatabase()
		 {
			  File storeDir = _testDirectory.storeDir();
			  GraphDatabaseService nonUpgradedStore = startNonUpgradableDatabaseWithFormat( storeDir, StandardV3_2.NAME );
			  TransactionFailureException failureException = assertThrows(typeof(TransactionFailureException), () =>
			  {
				using ( Transaction transaction = nonUpgradedStore.BeginTx() )
				{
					 Node node = nonUpgradedStore.CreateNode();
					 node.setProperty( "a", DateValue.date( 1991, 5, 3 ).asObjectCopy() );
					 transaction.success();
				}
			  });
			  assertEquals( "Current record format does not support TEMPORAL_PROPERTIES. Please upgrade your store to the format that support requested capability.", Exceptions.rootCause( failureException ).Message );
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
//ORIGINAL LINE: @Test void failToCreateDateArrayOnOldDatabase()
		 internal virtual void FailToCreateDateArrayOnOldDatabase()
		 {
			  File storeDir = _testDirectory.storeDir();
			  GraphDatabaseService nonUpgradedStore = startNonUpgradableDatabaseWithFormat( storeDir, StandardV3_2.NAME );
			  LocalDate date = DateValue.date( 1991, 5, 3 ).asObjectCopy();
			  TransactionFailureException failureException = assertThrows(typeof(TransactionFailureException), () =>
			  {
				using ( Transaction transaction = nonUpgradedStore.BeginTx() )
				{
					 Node node = nonUpgradedStore.CreateNode();
					 node.setProperty( "a", new LocalDate[]{ date, date } );
					 transaction.success();
				}
			  });
			  assertEquals( "Current record format does not support TEMPORAL_PROPERTIES. Please upgrade your store " + "to the format that support requested capability.", Exceptions.rootCause( failureException ).Message );
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
//ORIGINAL LINE: @Test void createDatePropertyOnLatestDatabase()
		 internal virtual void CreateDatePropertyOnLatestDatabase()
		 {
			  File storeDir = _testDirectory.storeDir();
			  Label label = Label.label( "DateNode" );
			  string propertyKey = "a";
			  LocalDate date = DateValue.date( 1991, 5, 3 ).asObjectCopy();

			  GraphDatabaseService database = startDatabaseWithFormat( storeDir, Standard.LATEST_NAME );
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode( label );
					node.SetProperty( propertyKey, date );
					transaction.Success();
			  }
			  database.Shutdown();

			  GraphDatabaseService restartedDatabase = startDatabaseWithFormat( storeDir, Standard.LATEST_NAME );
			  using ( Transaction ignored = restartedDatabase.BeginTx() )
			  {
					assertNotNull( restartedDatabase.FindNode( label, propertyKey, date ) );
			  }
			  restartedDatabase.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createDateArrayOnLatestDatabase()
		 internal virtual void CreateDateArrayOnLatestDatabase()
		 {
			  File storeDir = _testDirectory.storeDir();
			  Label label = Label.label( "DateNode" );
			  string propertyKey = "a";
			  LocalDate date = DateValue.date( 1991, 5, 3 ).asObjectCopy();

			  GraphDatabaseService database = startDatabaseWithFormat( storeDir, Standard.LATEST_NAME );
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode( label );
					node.SetProperty( propertyKey, new LocalDate[]{ date, date } );
					transaction.Success();
			  }
			  database.Shutdown();

			  GraphDatabaseService restartedDatabase = startDatabaseWithFormat( storeDir, Standard.LATEST_NAME );
			  using ( Transaction ignored = restartedDatabase.BeginTx() )
			  {
					using ( ResourceIterator<Node> nodes = restartedDatabase.FindNodes( label ) )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 Node node = nodes.next();
						 LocalDate[] points = ( LocalDate[] ) node.GetProperty( propertyKey );
						 assertThat( points, arrayWithSize( 2 ) );
					}
			  }
			  restartedDatabase.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failToOpenStoreWithDatePropertyUsingOldFormat()
		 internal virtual void FailToOpenStoreWithDatePropertyUsingOldFormat()
		 {
			  File storeDir = _testDirectory.storeDir();
			  GraphDatabaseService database = startDatabaseWithFormat( storeDir, StandardV3_4.NAME );
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode();
					node.SetProperty( "a", DateValue.date( 1991, 5, 3 ) );
					transaction.Success();
			  }
			  database.Shutdown();

			  Exception throwable = assertThrows( typeof( Exception ), () => startDatabaseWithFormat(storeDir, StandardV3_2.NAME) );
			  assertSame( typeof( StoreUpgrader.AttemptedDowngradeException ), Exceptions.rootCause( throwable ).GetType() );
		 }
	}

}