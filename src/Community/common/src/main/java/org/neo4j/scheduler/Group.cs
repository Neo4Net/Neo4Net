using System.Collections.Generic;

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
namespace Neo4Net.Scheduler
{

	/// <summary>
	/// Represents a common group of jobs, defining how they should be scheduled.
	/// </summary>
	public sealed class Group
	{
		 // GENERAL DATABASE GROUPS.
		 /// <summary>
		 /// Thread that schedules delayed or recurring tasks. </summary>
		 public static readonly Group TaskScheduler = new Group( "TaskScheduler", InnerEnum.TaskScheduler, "Scheduler", ExecutorServiceFactory.unschedulable() );
		 /* Background page cache worker. */
		 public static readonly Group PageCache = new Group( "PageCache", InnerEnum.PageCache, "PageCacheWorker" );
		 /// <summary>
		 /// Watch out for, and report, external manipulation of store files. </summary>
		 public static readonly Group FileWatcher = new Group( "FileWatcher", InnerEnum.FileWatcher, "FileWatcher" );
		 /// <summary>
		 /// Monitor and report system-wide pauses, in case they lead to service interruption. </summary>
		 public static readonly Group VmPauseMonitor = new Group( "VmPauseMonitor", InnerEnum.VmPauseMonitor, "VmPauseMonitor" );
		 /// <summary>
		 /// Rotates diagnostic text logs. </summary>
		 public static readonly Group LogRotation = new Group( "LogRotation", InnerEnum.LogRotation, "LogRotation" );
		 /// <summary>
		 /// Checkpoint and store flush. </summary>
		 public static readonly Group Checkpoint = new Group( "Checkpoint", InnerEnum.Checkpoint, "CheckPoint" );
		 /// <summary>
		 /// Various little periodic tasks that need to be done on a regular basis to keep the store in good shape. </summary>
		 public static readonly Group StorageMaintenance = new Group( "StorageMaintenance", InnerEnum.StorageMaintenance, "StorageMaintenance" );
		 /// <summary>
		 /// Terminates kernel transactions that have timed out. </summary>
		 public static readonly Group TransactionTimeoutMonitor = new Group( "TransactionTimeoutMonitor", InnerEnum.TransactionTimeoutMonitor, "TransactionTimeoutMonitor" );
		 /// <summary>
		 /// Background index population. </summary>
		 public static readonly Group IndexPopulation = new Group( "IndexPopulation", InnerEnum.IndexPopulation, "IndexPopulation" );
		 /// <summary>
		 /// Background index sampling </summary>
		 public static readonly Group IndexSampling = new Group( "IndexSampling", InnerEnum.IndexSampling, "IndexSampling" );
		 /// <summary>
		 /// Background index update applier, for eventually consistent indexes. </summary>
		 public static readonly Group IndexUpdating = new Group( "IndexUpdating", InnerEnum.IndexUpdating, "IndexUpdating", ExecutorServiceFactory.singleThread() ); // Single-threaded to serialise updates with opening/closing/flushing of indexes.
		 /// <summary>
		 /// Thread pool for anyone who want some help doing file IO in parallel. </summary>
		 public static readonly Group FileIoHelper = new Group( "FileIoHelper", InnerEnum.FileIoHelper, "FileIOHelper" );
		 public static readonly Group NativeSecurity = new Group( "NativeSecurity", InnerEnum.NativeSecurity, "NativeSecurity" );
		 public static readonly Group MetricsEvent = new Group( "MetricsEvent", InnerEnum.MetricsEvent, "MetricsEvent" );

		 // CYPHER.
		 /// <summary>
		 /// Thread pool for parallel Cypher query execution. </summary>
		 public static readonly Group CypherWorker = new Group( "CypherWorker", InnerEnum.CypherWorker, "CypherWorker", ExecutorServiceFactory.workStealing() );

		 // DATA COLLECTOR
		 public static readonly Group DataCollector = new Group( "DataCollector", InnerEnum.DataCollector, "DataCollector" );

		 // BOLT.
		 /// <summary>
		 /// Network IO threads for the Bolt protocol. </summary>
		 public static readonly Group BoltNetworkIo = new Group( "BoltNetworkIo", InnerEnum.BoltNetworkIo, "BoltNetworkIO" );
		 /// <summary>
		 /// Transaction processing threads for Bolt. </summary>
		 public static readonly Group BoltWorker = new Group( "BoltWorker", InnerEnum.BoltWorker, "BoltWorker" );

		 // CAUSAL CLUSTER, TOPOLOGY & BACKUP.
		 public static readonly Group RaftTimer = new Group( "RaftTimer", InnerEnum.RaftTimer, "RaftTimer" );
		 public static readonly Group RaftLogPruning = new Group( "RaftLogPruning", InnerEnum.RaftLogPruning, "RaftLogPruning" );
		 public static readonly Group RaftBatchHandler = new Group( "RaftBatchHandler", InnerEnum.RaftBatchHandler, "RaftBatchHandler" );
		 public static readonly Group RaftReaderPoolPruner = new Group( "RaftReaderPoolPruner", InnerEnum.RaftReaderPoolPruner, "RaftReaderPoolPruner" );
		 public static readonly Group HzTopologyHealth = new Group( "HzTopologyHealth", InnerEnum.HzTopologyHealth, "HazelcastHealth" );
		 public static readonly Group HzTopologyKeepAlive = new Group( "HzTopologyKeepAlive", InnerEnum.HzTopologyKeepAlive, "KeepAlive" );
		 public static readonly Group HzTopologyRefresh = new Group( "HzTopologyRefresh", InnerEnum.HzTopologyRefresh, "TopologyRefresh" );
		 public static readonly Group AkkaTopologyWorker = new Group( "AkkaTopologyWorker", InnerEnum.AkkaTopologyWorker, "AkkaTopologyWorkers", ExecutorServiceFactory.workStealing() );
		 public static readonly Group MembershipWaiter = new Group( "MembershipWaiter", InnerEnum.MembershipWaiter, "MembershipWaiter" );
		 public static readonly Group DownloadSnapshot = new Group( "DownloadSnapshot", InnerEnum.DownloadSnapshot, "DownloadSnapshot" );

		 // HA.
		 /// <summary>
		 /// Push transactions from master to slaves </summary>
		 public static readonly Group MasterTransactionPushing = new Group( "MasterTransactionPushing", InnerEnum.MasterTransactionPushing, "TransactionPushing" );
		 /// <summary>
		 /// Rolls back idle transactions on the server. </summary>
		 public static readonly Group ServerTransactionTimeout = new Group( "ServerTransactionTimeout", InnerEnum.ServerTransactionTimeout, "ServerTransactionTimeout" );
		 /// <summary>
		 /// Aborts idle slave lock sessions on the master. </summary>
		 public static readonly Group SlaveLocksTimeout = new Group( "SlaveLocksTimeout", InnerEnum.SlaveLocksTimeout, "SlaveLocksTimeout" );
		 /// <summary>
		 /// Pulls updates from the master. </summary>
		 public static readonly Group PullUpdates = new Group( "PullUpdates", InnerEnum.PullUpdates, "PullUpdates" );

		 // MISC.
		 /// <summary>
		 /// UDC timed events. </summary>
		 public static readonly Group Udc = new Group( "Udc", InnerEnum.Udc, "UsageDataCollector" );

		 private static readonly IList<Group> valueList = new List<Group>();

		 static Group()
		 {
			 valueList.Add( TaskScheduler );
			 valueList.Add( PageCache );
			 valueList.Add( FileWatcher );
			 valueList.Add( VmPauseMonitor );
			 valueList.Add( LogRotation );
			 valueList.Add( Checkpoint );
			 valueList.Add( StorageMaintenance );
			 valueList.Add( TransactionTimeoutMonitor );
			 valueList.Add( IndexPopulation );
			 valueList.Add( IndexSampling );
			 valueList.Add( IndexUpdating );
			 valueList.Add( FileIoHelper );
			 valueList.Add( NativeSecurity );
			 valueList.Add( MetricsEvent );
			 valueList.Add( CypherWorker );
			 valueList.Add( DataCollector );
			 valueList.Add( BoltNetworkIo );
			 valueList.Add( BoltWorker );
			 valueList.Add( RaftTimer );
			 valueList.Add( RaftLogPruning );
			 valueList.Add( RaftBatchHandler );
			 valueList.Add( RaftReaderPoolPruner );
			 valueList.Add( HzTopologyHealth );
			 valueList.Add( HzTopologyKeepAlive );
			 valueList.Add( HzTopologyRefresh );
			 valueList.Add( AkkaTopologyWorker );
			 valueList.Add( MembershipWaiter );
			 valueList.Add( DownloadSnapshot );
			 valueList.Add( MasterTransactionPushing );
			 valueList.Add( ServerTransactionTimeout );
			 valueList.Add( SlaveLocksTimeout );
			 valueList.Add( PullUpdates );
			 valueList.Add( Udc );
		 }

		 public enum InnerEnum
		 {
			 TaskScheduler,
			 PageCache,
			 FileWatcher,
			 VmPauseMonitor,
			 LogRotation,
			 Checkpoint,
			 StorageMaintenance,
			 TransactionTimeoutMonitor,
			 IndexPopulation,
			 IndexSampling,
			 IndexUpdating,
			 FileIoHelper,
			 NativeSecurity,
			 MetricsEvent,
			 CypherWorker,
			 DataCollector,
			 BoltNetworkIo,
			 BoltWorker,
			 RaftTimer,
			 RaftLogPruning,
			 RaftBatchHandler,
			 RaftReaderPoolPruner,
			 HzTopologyHealth,
			 HzTopologyKeepAlive,
			 HzTopologyRefresh,
			 AkkaTopologyWorker,
			 MembershipWaiter,
			 DownloadSnapshot,
			 MasterTransactionPushing,
			 ServerTransactionTimeout,
			 SlaveLocksTimeout,
			 PullUpdates,
			 Udc
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private readonly;
		 internal Private readonly;

		 internal Group( string name, InnerEnum innerEnum, string name, ExecutorServiceFactory executorServiceFactory )
		 {
			  this._name = name;
			  this._executorServiceFactory = executorServiceFactory;
			  _threadCounter = new AtomicInteger();

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal Group( string name, InnerEnum innerEnum, string name ) : this( name, ExecutorServiceFactory.cached() )
		 {

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 /// <summary>
		 /// The slightly more human-readable name of the group. Useful for naming <seealso cref="ThreadGroup thread groups"/>, and also used as a component in the
		 /// <seealso cref="threadName() thread names"/>.
		 /// </summary>
		 public string GroupName()
		 {
			  return _name;
		 }

		 /// <summary>
		 /// Name a new thread. This method may or may not be used, it is up to the scheduling strategy to decide
		 /// to honor this.
		 /// </summary>
		 public string ThreadName()
		 {
			  return "neo4j." + GroupName() + "-" + _threadCounter.incrementAndGet();
		 }

		 public java.util.concurrent.ExecutorService BuildExecutorService( SchedulerThreadFactory factory )
		 {
			  return _executorServiceFactory.build( this, factory );
		 }

		public static IList<Group> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static Group valueOf( string name )
		{
			foreach ( Group enumInstance in Group.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}