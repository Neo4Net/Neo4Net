using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.transaction.command
{
	using Test = org.junit.Test;


	using Label = Neo4Net.Graphdb.Label;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionCursor = Neo4Net.Kernel.impl.transaction.log.TransactionCursor;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.singleOrNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.tracing.CommitEvent.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.EXTERNAL;

	/// <summary>
	/// This test is for an issue with transaction batching where there would be a batch of transactions
	/// to be applied in the same batch; the batch containing a creation of node N with label L and property P.
	/// Later in that batch there would be a uniqueness constraint created for label L and property P.
	/// The number of nodes matching this constraint would be few and so the label scan store would be selected
	/// to drive the population of the index. Problem is that the label update for N would still sit in
	/// the batch state, to be applied at the end of the batch. Hence the node would be forgotten when the
	/// index was being built.
	/// </summary>
	public class LabelAndIndexUpdateBatchingIT
	{
		 private const string PROPERTY_KEY = "key";
		 private static readonly Label _label = Label.label( "label" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexShouldIncludeNodesCreatedPreviouslyInBatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexShouldIncludeNodesCreatedPreviouslyInBatch()
		 {
			  // GIVEN a transaction stream leading up to this issue
			  // perform the transactions from db-level and extract the transactions as commands
			  // so that they can be applied batch-wise they way we'd like to later.

			  // a bunch of nodes (to have the index population later on to decide to use label scan for population)
			  IList<TransactionRepresentation> transactions;
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  string nodeN = "our guy";
			  string otherNode = "just to create the tokens";
			  try
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.createNode( _label ).setProperty( PROPERTY_KEY, otherNode );
						 for ( int i = 0; i < 10_000; i++ )
						 {
							  Db.createNode();
						 }
						 tx.Success();
					}
					// node N
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.createNode( _label ).setProperty( PROPERTY_KEY, nodeN );
						 tx.Success();
					}
					// uniqueness constraint affecting N
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().constraintFor(_label).assertPropertyIsUnique(PROPERTY_KEY).create();
						 tx.Success();
					}
					transactions = ExtractTransactions( db );
			  }
			  finally
			  {
					Db.shutdown();
			  }

			  db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  TransactionCommitProcess commitProcess = Db.DependencyResolver.resolveDependency( typeof( TransactionCommitProcess ) );
			  try
			  {
					int cutoffIndex = FindCutoffIndex( transactions );
					commitProcess.Commit( ToApply( transactions.subList( 0, cutoffIndex ) ), NULL, EXTERNAL );

					// WHEN applying the two transactions (node N and the constraint) in the same batch
					commitProcess.Commit( ToApply( transactions.subList( cutoffIndex, transactions.Count ) ), NULL, EXTERNAL );

					// THEN node N should've ended up in the index too
					using ( Transaction tx = Db.beginTx() )
					{
						 assertNotNull( "Verification node not found", singleOrNull( Db.findNodes( _label, PROPERTY_KEY, otherNode ) ) ); // just to verify
						 assertNotNull( "Node N not found", singleOrNull( Db.findNodes( _label, PROPERTY_KEY, nodeN ) ) );
						 tx.Success();
					}
			  }
			  finally
			  {
					Db.shutdown();
			  }

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static int findCutoffIndex(java.util.Collection<org.neo4j.kernel.impl.transaction.TransactionRepresentation> transactions) throws java.io.IOException
		 private static int FindCutoffIndex( ICollection<TransactionRepresentation> transactions )
		 {
			  IEnumerator<TransactionRepresentation> iterator = transactions.GetEnumerator();
			  for ( int i = 0; iterator.MoveNext(); i++ )
			  {
					TransactionRepresentation tx = iterator.Current;
					CommandExtractor extractor = new CommandExtractor();
					tx.Accept( extractor );
					IList<StorageCommand> commands = extractor.Commands;
					IList<StorageCommand> nodeCommands = commands.Where( command => command is NodeCommand ).ToList();
					if ( nodeCommands.Count == 1 )
					{
						 return i;
					}
			  }
			  throw new AssertionError( "Couldn't find the transaction which would be the cut-off point" );
		 }

		 private static TransactionToApply ToApply( ICollection<TransactionRepresentation> transactions )
		 {
			  TransactionToApply first = null;
			  TransactionToApply last = null;
			  foreach ( TransactionRepresentation transactionRepresentation in transactions )
			  {
					TransactionToApply transaction = new TransactionToApply( transactionRepresentation );
					if ( first == null )
					{
						 first = last = transaction;
					}
					else
					{
						 last.Next( transaction );
						 last = transaction;
					}
			  }
			  return first;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.List<org.neo4j.kernel.impl.transaction.TransactionRepresentation> extractTransactions(org.neo4j.kernel.internal.GraphDatabaseAPI db) throws java.io.IOException
		 private static IList<TransactionRepresentation> ExtractTransactions( GraphDatabaseAPI db )
		 {
			  LogicalTransactionStore txStore = Db.DependencyResolver.resolveDependency( typeof( LogicalTransactionStore ) );
			  IList<TransactionRepresentation> transactions = new List<TransactionRepresentation>();
			  using ( TransactionCursor cursor = txStore.GetTransactions( Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID + 1 ) )
			  {
					cursor.forAll( tx => transactions.Add( tx.TransactionRepresentation ) );
			  }
			  return transactions;
		 }
	}

}