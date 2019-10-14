using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.ha.transaction
{

	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using ComException = Neo4Net.com.ComException;
	using NamedThreadFactory = Neo4Net.Helpers.NamedThreadFactory;
	using Neo4Net.Helpers.Collections;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Slave = Neo4Net.Kernel.ha.com.master.Slave;
	using SlavePriorities = Neo4Net.Kernel.ha.com.master.SlavePriorities;
	using SlavePriority = Neo4Net.Kernel.ha.com.master.SlavePriority;
	using Slaves = Neo4Net.Kernel.ha.com.master.Slaves;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Log = Neo4Net.Logging.Log;
	using CappedLogger = Neo4Net.Logging.@internal.CappedLogger;
	using Clocks = Neo4Net.Time.Clocks;

	/// <summary>
	/// Pushes transactions committed on master to one or more slaves. Number of slaves receiving each transactions
	/// is controlled by <seealso cref="HaSettings.tx_push_factor"/>. Which slaves receives transactions is controlled by
	/// <seealso cref="HaSettings.tx_push_strategy"/>.
	/// 
	/// An attempt is made to push each transaction to the wanted number of slaves, but if it isn't possible
	/// and a timeout is hit, propagation will still be considered as successful and occurrence will be logged.
	/// </summary>
	public class TransactionPropagator : Lifecycle
	{
		 public interface Configuration
		 {
			  int TxPushFactor { get; }

			  InstanceId ServerId { get; }

			  SlavePriority ReplicationStrategy { get; }
		 }

		 private class ReplicationContext
		 {
			  internal readonly Future<Void> Future;
			  internal readonly Slave Slave;

			  internal Exception Throwable;

			  internal ReplicationContext( Future<Void> future, Slave slave )
			  {
					this.Future = future;
					this.Slave = slave;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Configuration from(final org.neo4j.kernel.configuration.Config config)
		 public static Configuration From( Config config )
		 {
			  return new ConfigurationAnonymousInnerClass( config );
		 }

		 private class ConfigurationAnonymousInnerClass : Configuration
		 {
			 private Config _config;

			 public ConfigurationAnonymousInnerClass( Config config )
			 {
				 this._config = config;
			 }

			 public int TxPushFactor
			 {
				 get
				 {
					  return _config.get( HaSettings.tx_push_factor );
				 }
			 }

			 public InstanceId ServerId
			 {
				 get
				 {
					  return _config.get( ClusterSettings.server_id );
				 }
			 }

			 public SlavePriority ReplicationStrategy
			 {
				 get
				 {
					  switch ( _config.get( HaSettings.tx_push_strategy ) )
					  {
							case fixed_descending:
								 return SlavePriorities.fixedDescending();
   
							case fixed_ascending:
								 return SlavePriorities.fixedAscending();
   
							case round_robin:
								 return SlavePriorities.roundRobin();
   
							default:
								 throw new Exception( "Unknown replication strategy " );
					  }
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Configuration from(final org.neo4j.kernel.configuration.Config config, final org.neo4j.kernel.ha.com.master.SlavePriority slavePriority)
		 public static Configuration From( Config config, SlavePriority slavePriority )
		 {
			  return new ConfigurationAnonymousInnerClass2( config, slavePriority );
		 }

		 private class ConfigurationAnonymousInnerClass2 : Configuration
		 {
			 private Config _config;
			 private SlavePriority _slavePriority;

			 public ConfigurationAnonymousInnerClass2( Config config, SlavePriority slavePriority )
			 {
				 this._config = config;
				 this._slavePriority = slavePriority;
			 }

			 public int TxPushFactor
			 {
				 get
				 {
					  return _config.get( HaSettings.tx_push_factor );
				 }
			 }

			 public InstanceId ServerId
			 {
				 get
				 {
					  return _config.get( ClusterSettings.server_id );
				 }
			 }

			 public SlavePriority ReplicationStrategy
			 {
				 get
				 {
					  return _slavePriority;
				 }
			 }
		 }

		 private int _desiredReplicationFactor;
		 private SlavePriority _replicationStrategy;
		 private ExecutorService _slaveCommitters;
		 private readonly Log _log;
		 private readonly Configuration _config;
		 private readonly Slaves _slaves;
		 private readonly CommitPusher _pusher;
		 private readonly CappedLogger _slaveCommitFailureLogger;
		 private readonly CappedLogger _pushedToTooFewSlaveLogger;

		 public TransactionPropagator( Configuration config, Log log, Slaves slaves, CommitPusher pusher )
		 {
			  this._config = config;
			  this._log = log;
			  this._slaves = slaves;
			  this._pusher = pusher;
			  _slaveCommitFailureLogger = ( new CappedLogger( log ) ).setTimeLimit( 5, SECONDS, Clocks.systemClock() );
			  _pushedToTooFewSlaveLogger = ( new CappedLogger( log ) ).setTimeLimit( 5, SECONDS, Clocks.systemClock() );
		 }

		 public override void Init()
		 {
		 }

		 public override void Start()
		 {
			  this._slaveCommitters = Executors.newCachedThreadPool( new NamedThreadFactory( "slave-committer" ) );
			  _desiredReplicationFactor = _config.TxPushFactor;
			  _replicationStrategy = _config.ReplicationStrategy;
		 }

		 public override void Stop()
		 {
			  this._slaveCommitters.shutdown();
		 }

		 public override void Shutdown()
		 {
		 }

		 /// 
		 /// <param name="txId"> transaction id to replicate </param>
		 /// <param name="authorId"> author id for such transaction id </param>
		 /// <returns> the number of missed replicas (e.g., desired replication factor - number of successful replications) </returns>
		 public virtual int Committed( long txId, int authorId )
		 {
			  int replicationFactor = _desiredReplicationFactor;
			  // If the author is not this instance, then we need to push to one less - the committer already has it
			  bool isAuthoredBySlave = _config.ServerId.toIntegerIndex() != authorId;
			  if ( isAuthoredBySlave )
			  {
					replicationFactor--;
			  }

			  if ( replicationFactor == 0 )
			  {
					return replicationFactor;
			  }
			  ICollection<ReplicationContext> committers = new HashSet<ReplicationContext>();

			  try
			  {
					// TODO: Move this logic into {@link CommitPusher}
					// Commit at the configured amount of slaves in parallel.
					int successfulReplications = 0;
					IEnumerator<Slave> slaveList = Filter( _replicationStrategy.prioritize( _slaves.getSlaves() ).GetEnumerator(), authorId );
					CompletionNotifier notifier = new CompletionNotifier();

					// Start as many initial committers as needed
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					for ( int i = 0; i < replicationFactor && slaveList.hasNext(); i++ )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 Slave slave = slaveList.next();
						 Callable<Void> slaveCommitter = slaveCommitter( slave, txId, notifier );
						 committers.Add( new ReplicationContext( _slaveCommitters.submit( slaveCommitter ), slave ) );
					}

					// Wait for them and perhaps spawn new ones for failing committers until we're done
					// or until we have no more slaves to try out.
					ICollection<ReplicationContext> toAdd = new List<ReplicationContext>();
					ICollection<ReplicationContext> toRemove = new List<ReplicationContext>();
					while ( committers.Count > 0 && successfulReplications < replicationFactor )
					{
						 toAdd.Clear();
						 toRemove.Clear();
						 foreach ( ReplicationContext context in committers )
						 {
							  if ( !context.Future.Done )
							  {
									continue;
							  }

							  if ( IsSuccessful( context ) )
							  {
							  // This committer was successful, increment counter
									successfulReplications++;
							  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  else if ( slaveList.hasNext() )
							  {
							  // This committer failed, spawn another one
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
									Slave newSlave = slaveList.next();
									Callable<Void> slaveCommitter;
									try
									{
										 slaveCommitter = slaveCommitter( newSlave, txId, notifier );
									}
									catch ( Exception t )
									{
										 _log.error( "Unknown error commit master transaction at slave", t );
										 return _desiredReplicationFactor;
									}

									toAdd.Add( new ReplicationContext( _slaveCommitters.submit( slaveCommitter ), newSlave ) );
							  }
							  toRemove.Add( context );
						 }

						 // Incorporate the results into committers collection
						 if ( toAdd.Count > 0 )
						 {
							  committers.addAll( toAdd );
						 }
						 if ( toRemove.Count > 0 )
						 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
							  committers.removeAll( toRemove );
						 }

						 if ( committers.Count > 0 )
						 // There are committers doing work right now, so go and wait for
						 // any of the committers to be done so that we can reevaluate
						 // the situation again.
						 {
							  notifier.WaitForAnyCompletion();
						 }
					}

					// We did the best we could, have we committed successfully on enough slaves?
					if ( successfulReplications < replicationFactor )
					{
						 _pushedToTooFewSlaveLogger.info( "Transaction " + txId + " couldn't commit on enough slaves, desired " + replicationFactor + ", but could only commit at " + successfulReplications );
					}

					return replicationFactor - successfulReplications;
			  }
			  finally
			  {
					// Cancel all ongoing committers in the executor
					foreach ( ReplicationContext committer in committers )
					{
						 committer.Future.cancel( false );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private java.util.Iterator<org.neo4j.kernel.ha.com.master.Slave> filter(java.util.Iterator<org.neo4j.kernel.ha.com.master.Slave> slaves, final System.Nullable<int> externalAuthorServerId)
		 private IEnumerator<Slave> Filter( IEnumerator<Slave> slaves, int? externalAuthorServerId )
		 {
			  return externalAuthorServerId == null ? slaves : new FilteringIterator<Slave>( slaves, item => item.ServerId != externalAuthorServerId );
		 }

		 private bool IsSuccessful( ReplicationContext context )
		 {
			  try
			  {
					context.Future.get();
					return true;
			  }
			  catch ( Exception e ) when ( e is InterruptedException || e is CancellationException )
			  {
					return false;
			  }
			  catch ( ExecutionException e )
			  {
					context.Throwable = e.InnerException;
					_slaveCommitFailureLogger.error( "Slave " + context.Slave.ServerId + ": Replication commit threw" + ( context.Throwable is ComException ? " communication" : "" ) + " exception:", context.Throwable );
					return false;
			  }
		 }

		 /// <summary>
		 /// A version of wait/notify which can handle that a notify comes before the
		 /// call to wait, in which case the call to wait will return immediately.
		 /// 
		 /// @author Mattias Persson
		 /// </summary>
		 private class CompletionNotifier
		 {
			  internal bool Notified;

			  internal virtual void Completed()
			  {
				  lock ( this )
				  {
						Notified = true;
						Monitor.PulseAll( this );
				  }
			  }

			  internal virtual void WaitForAnyCompletion()
			  {
				  lock ( this )
				  {
						if ( !Notified )
						{
							 Notified = false;
							 try
							 {
								  Monitor.Wait( this, TimeSpan.FromMilliseconds( 2000 ) );
							 }
							 catch ( InterruptedException )
							 {
								  Thread.interrupted();
								  // Hmm, ok we got interrupted. No biggy I'd guess
							 }
						}
						else
						{
							 Notified = false;
						}
				  }
			  }

			  public override string ToString()
			  {
					return "CompletionNotifier{id=" + System.identityHashCode( this ) + ",notified=" + Notified + "}";
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private java.util.concurrent.Callable<Void> slaveCommitter(final org.neo4j.kernel.ha.com.master.Slave slave, final long txId, final CompletionNotifier notifier)
		 private Callable<Void> SlaveCommitter( Slave slave, long txId, CompletionNotifier notifier )
		 {
			  return () =>
			  {
				try
				{
					 // TODO Bypass the CommitPusher, now that we have a single thread pulling updates on each slave
					 // The CommitPusher is all about batching transaction pushing to slaves, to reduce the overhead
					 // of multiple threads pulling the same transactions on each slave. That should be fine now.
//                    slave.pullUpdates( txId );
					 _pusher.queuePush( slave, txId );

					 return null;
				}
				finally
				{
					 notifier.Completed();
				}
			  };
		 }
	}

}