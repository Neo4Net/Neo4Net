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
namespace Neo4Net.causalclustering.stresstests
{

	using Neo4Net.causalclustering.discovery;
	using Neo4Net.causalclustering.discovery;
	using ReadReplica = Neo4Net.causalclustering.discovery.ReadReplica;
	using Lag = Neo4Net.causalclustering.stresstests.LagEvaluator.Lag;
	using Workload = Neo4Net.helper.Workload;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.awaitForever;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.DependencyResolver_SelectionStrategy.ONLY;

	internal class CatchupNewReadReplica : Workload
	{
		 private const long SAMPLE_INTERVAL_MS = 2000;
		 private const long MAX_LAG_MS = 500;

		 private readonly FileSystemAbstraction _fs;
		 private readonly Log _log;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private readonly Cluster<object> _cluster;
		 private bool _deleteStore;

		 internal CatchupNewReadReplica( Control control, Resources resources ) : base( control )
		 {
			  this._fs = resources.FileSystem();
			  this._cluster = resources.Cluster();
			  this._log = resources.LogProvider().getLog(this.GetType());
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void doWork() throws java.io.IOException
		 protected internal override void DoWork()
		 {
			  int newMemberId = _cluster.readReplicas().Count;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.ReadReplica readReplica = cluster.addReadReplicaWithId(newMemberId);
			  ReadReplica readReplica = _cluster.addReadReplicaWithId( newMemberId );

			  _log.info( "Adding " + readReplica );
			  readReplica.Start();

			  LagEvaluator lagEvaluator = new LagEvaluator( this.leaderTxId, () => TxId(readReplica), Clock.systemUTC() );

			  awaitForever(() =>
			  {
				if ( !Control.keepGoing() )
				{
					 return true;
				}

				Optional<Lag> lagEstimate = lagEvaluator.Evaluate();

				if ( lagEstimate.Present )
				{
					 _log.info( lagEstimate.get().ToString() );
					 return lagEstimate.get().timeLagMillis() < MAX_LAG_MS;
				}
				else
				{
					 _log.info( "Lag estimate not available" );
					 return false;
				}
			  }, SAMPLE_INTERVAL_MS, MILLISECONDS);

			  if ( !Control.keepGoing() )
			  {
					return;
			  }

			  _log.info( "Caught up" );
			  _cluster.removeReadReplicaWithMemberId( newMemberId );

			  if ( _deleteStore )
			  {
					_log.info( "Deleting store of " + readReplica );
					_fs.deleteRecursively( readReplica.DatabaseDirectory() );
			  }
			  _deleteStore = !_deleteStore;
		 }

		 private long? LeaderTxId()
		 {
			  try
			  {
					return TxId( _cluster.awaitLeader() );
			  }
			  catch ( TimeoutException )
			  {
					return long?.empty();
			  }
		 }

		 private long? TxId( ClusterMember member )
		 {
			  try
			  {
					GraphDatabaseAPI database = member.database();
					TransactionIdStore txIdStore = database.DependencyResolver.resolveDependency( typeof( TransactionIdStore ), ONLY );
					return long?.of( txIdStore.LastClosedTransactionId );
			  }
			  catch ( Exception )
			  {
					return long?.empty();
			  }
		 }
	}

}