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
namespace Neo4Net.backup.stresstests
{

	using Control = Neo4Net.causalclustering.stresstests.Control;
	using DatabaseShutdownException = Neo4Net.Graphdb.DatabaseShutdownException;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using TransientFailureException = Neo4Net.Graphdb.TransientFailureException;
	using Workload = Neo4Net.helper.Workload;

	internal class TransactionalWorkload : Workload
	{
		 private static readonly Label _label = Label.label( "Label" );
		 private readonly System.Func<GraphDatabaseService> _dbRef;

		 internal TransactionalWorkload( Control control, System.Func<GraphDatabaseService> dbRef ) : base( control )
		 {
			  this._dbRef = dbRef;
		 }

		 protected internal override void DoWork()
		 {
			  GraphDatabaseService db = _dbRef.get();
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						Node node = Db.createNode( _label );
						for ( int i = 1; i <= 8; i++ )
						{
							 node.SetProperty( Prop( i ), "let's add some data here so the transaction logs rotate more often..." );
						}
						tx.Success();
					  }
			  }
			  catch ( Exception e ) when ( e is DatabaseShutdownException || e is TransactionFailureException || e is TransientFailureException )
			  {
					// whatever let's go on with the workload
			  }
		 }

		 internal static void SetupIndexes( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 1; i <= 8; i++ )
					{
						 Db.schema().indexFor(_label).on(Prop(i)).create();
					}
					tx.Success();
			  }
		 }

		 private static string Prop( int i )
		 {
			  return "prop" + i;
		 }
	}

}