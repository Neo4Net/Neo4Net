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
namespace Neo4Net.causalclustering.stresstests
{

	using Neo4Net.causalclustering.discovery;
	using DatabaseShutdownException = Neo4Net.GraphDb.DatabaseShutdownException;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using TransactionFailureException = Neo4Net.GraphDb.TransactionFailureException;
	using Workload = Neo4Net.helper.Workload;
	using Log = Neo4Net.Logging.Log;
	using CappedLogger = Neo4Net.Logging.Internal.CappedLogger;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;

	internal class CreateNodesWithProperties : Workload
	{
		 private static readonly Label _label = Label.label( "Label" );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Neo4Net.causalclustering.discovery.Cluster<?> cluster;
		 private readonly Cluster<object> _cluster;
		 private readonly CappedLogger _txLogger;
		 private readonly bool _enableIndexes;

		 private long _txSuccessCount;
		 private long _txFailCount;

		 internal CreateNodesWithProperties( Control control, Resources resources, Config config ) : base( control )
		 {
			  this._enableIndexes = config.EnableIndexes();
			  this._cluster = resources.Cluster();
			  Log log = resources.LogProvider().getLog(this.GetType());
			  this._txLogger = ( new CappedLogger( log ) ).setTimeLimit( 5, TimeUnit.SECONDS, resources.Clock() );
		 }

		 public override void Prepare()
		 {
			  if ( _enableIndexes )
			  {
					SetupIndexes( _cluster );
			  }
		 }

		 protected internal override void DoWork()
		 {
			  _txLogger.info( "SuccessCount: " + _txSuccessCount + " FailCount: " + _txFailCount );
			  RandomValues randomValues = RandomValues.create();

			  try
			  {
					_cluster.coreTx((db, tx) =>
					{
					 Node node = Db.createNode( _label );
					 for ( int i = 1; i <= 8; i++ )
					 {
						  node.setProperty( Prop( i ), randomValues.NextValue().asObject() );
					 }
					 tx.success();
					});
			  }
			  catch ( Exception e )
			  {
					_txFailCount++;

					if ( IsInterrupted( e ) || IsTransient( e ) )
					{
						 // whatever let's go on with the workload
						 return;
					}

					throw new Exception( e );
			  }

			  _txSuccessCount++;
		 }

		 private static void SetupIndexes<T1>( Cluster<T1> cluster )
		 {
			  try
			  {
					cluster.CoreTx((db, tx) =>
					{
					 for ( int i = 1; i <= 8; i++ )
					 {
						  Db.schema().indexFor(_label).on(Prop(i)).create();
					 }
					 tx.success();
					});
			  }
			  catch ( Exception t )
			  {
					throw new Exception( t );
			  }
		 }

		 private static string Prop( int i )
		 {
			  return "prop" + i;
		 }

		 private bool IsTransient( Exception e )
		 {
			  return e != null && ( e is TimeoutException || e is DatabaseShutdownException || e is TransactionFailureException || IsInterrupted( e.InnerException ) );
		 }

		 private bool IsInterrupted( Exception e )
		 {
			  if ( e == null )
			  {
					return false;
			  }

			  if ( e is InterruptedException )
			  {
					Thread.interrupted();
					return true;
			  }

			  return IsInterrupted( e.InnerException );
		 }
	}

}