using System;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.stresstests.transaction.checkpoint.workload
{
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using RandomMutation = Neo4Net.Kernel.stresstests.transaction.checkpoint.mutation.RandomMutation;

	internal class Worker : ThreadStart
	{
		 internal interface Monitor
		 {
			  void TransactionCompleted();
			  bool Stop();
			  void Done();
		 }

		 private readonly IGraphDatabaseService _db;
		 private readonly RandomMutation _randomMutation;
		 private readonly Monitor _monitor;
		 private readonly int _numOpsPerTx;

		 internal Worker( IGraphDatabaseService db, RandomMutation randomMutation, Monitor monitor, int numOpsPerTx )
		 {
			  this._db = db;
			  this._randomMutation = randomMutation;
			  this._monitor = monitor;
			  this._numOpsPerTx = numOpsPerTx;
		 }

		 public override void Run()
		 {
			  do
			  {
					try
					{
							using ( Transaction tx = _db.beginTx() )
							{
							 for ( int i = 0; i < _numOpsPerTx; i++ )
							 {
								  _randomMutation.perform();
							 }
							 tx.Success();
							}
					}
					catch ( DeadlockDetectedException )
					{
						 // simply give up
					}
					catch ( Exception e )
					{
						 // ignore and go on
						 Console.WriteLine( e.ToString() );
						 Console.Write( e.StackTrace );
					}

					_monitor.transactionCompleted();
			  } while ( !_monitor.stop() );

			  _monitor.done();
		 }
	}

}