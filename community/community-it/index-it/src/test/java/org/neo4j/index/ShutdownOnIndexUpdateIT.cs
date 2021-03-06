﻿/*
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
namespace Org.Neo4j.Index
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Schema = Org.Neo4j.Graphdb.schema.Schema;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using RecordStorageEngine = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using LifecycleListener = Org.Neo4j.Kernel.Lifecycle.LifecycleListener;
	using LifecycleStatus = Org.Neo4j.Kernel.Lifecycle.LifecycleStatus;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ShutdownOnIndexUpdateIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule database = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public DatabaseRule Database = new ImpermanentDatabaseRule();

		 private const string UNIQUE_PROPERTY_NAME = "uniquePropertyName";
		 private static readonly AtomicLong _indexProvider = new AtomicLong();
		 private static Label _constraintIndexLabel = Label.label( "ConstraintIndexLabel" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shutdownWhileFinishingTransactionWithIndexUpdates()
		 public virtual void ShutdownWhileFinishingTransactionWithIndexUpdates()
		 {
			  CreateConstraint( Database );
			  WaitIndexesOnline( Database );

			  using ( Transaction transaction = Database.beginTx() )
			  {
					Node node = Database.createNode( _constraintIndexLabel );
					node.SetProperty( UNIQUE_PROPERTY_NAME, _indexProvider.AndIncrement );

					DependencyResolver dependencyResolver = Database.DependencyResolver;
					NeoStoreDataSource dataSource = dependencyResolver.ResolveDependency( typeof( NeoStoreDataSource ) );
					LifeSupport dataSourceLife = dataSource.Life;
					TransactionCloseListener closeListener = new TransactionCloseListener( transaction );
					dataSourceLife.AddLifecycleListener( closeListener );
					dataSource.Stop();

					assertTrue( "Transaction should be closed and no exception should be thrown.", closeListener.TransactionClosed );
			  }
		 }

		 private void WaitIndexesOnline( GraphDatabaseService database )
		 {
			  using ( Transaction ignored = database.BeginTx() )
			  {
					database.Schema().awaitIndexesOnline(5, TimeUnit.MINUTES);
			  }
		 }

		 private void CreateConstraint( GraphDatabaseService database )
		 {
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Schema schema = database.Schema();
					Schema.constraintFor( _constraintIndexLabel ).assertPropertyIsUnique( UNIQUE_PROPERTY_NAME ).create();
					transaction.Success();
			  }
		 }

		 private class TransactionCloseListener : LifecycleListener
		 {
			  internal readonly Transaction Transaction;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool TransactionClosedConflict;

			  internal TransactionCloseListener( Transaction transaction )
			  {
					this.Transaction = transaction;
			  }

			  public override void NotifyStatusChanged( object instance, LifecycleStatus from, LifecycleStatus to )
			  {
					if ( ( LifecycleStatus.STOPPED == to ) && ( instance is RecordStorageEngine ) )
					{
						 Transaction.success();
						 Transaction.close();
						 TransactionClosedConflict = true;
					}
			  }

			  internal virtual bool TransactionClosed
			  {
				  get
				  {
						return TransactionClosedConflict;
				  }
			  }
		 }
	}

}