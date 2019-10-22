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
namespace Neo4Net.metrics.source.db
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;

	using BoltConnectionMetricsMonitor = Neo4Net.Bolt.runtime.BoltConnectionMetricsMonitor;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	[Documented(".Bolt metrics")]
	public class BoltMetrics : LifecycleAdapter
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_boltMonitor = new BoltMetricsMonitor( this );
		}

		 private const string NAME_PREFIX = "Neo4Net.bolt";

		 [Documented("The total number of Bolt sessions started since this instance started. This includes both " + "succeeded and failed sessions (deprecated, use connections_opened instead).")]
		 public static readonly string SessionsStarted = name( NAME_PREFIX, "sessions_started" );

		 [Documented("The total number of Bolt connections opened since this instance started. This includes both " + "succeeded and failed connections.")]
		 public static readonly string ConnectionsOpened = name( NAME_PREFIX, "connections_opened" );

		 [Documented("The total number of Bolt connections closed since this instance started. This includes both " + "properly and abnormally ended connections.")]
		 public static readonly string ConnectionsClosed = name( NAME_PREFIX, "connections_closed" );

		 [Documented("The total number of Bolt connections currently being executed.")]
		 public static readonly string ConnectionsRunning = name( NAME_PREFIX, "connections_running" );

		 [Documented("The total number of Bolt connections sitting idle.")]
		 public static readonly string ConnectionsIdle = name( NAME_PREFIX, "connections_idle" );

		 [Documented("The total number of messages received via Bolt since this instance started.")]
		 public static readonly string MessagesReceived = name( NAME_PREFIX, "messages_received" );

		 [Documented("The total number of messages that began processing since this instance started. This is different " + "from messages received in that this counter tracks how many of the received messages have" + "been taken on by a worker thread.")]
		 public static readonly string MessagesStarted = name( NAME_PREFIX, "messages_started" );

		 [Documented("The total number of messages that completed processing since this instance started. This includes " + "successful, failed and ignored Bolt messages.")]
		 public static readonly string MessagesDone = name( NAME_PREFIX, "messages_done" );

		 [Documented("The total number of messages that failed processing since this instance started.")]
		 public static readonly string MessagesFailed = name( NAME_PREFIX, "messages_failed" );

		 [Documented("The accumulated time messages have spent waiting for a worker thread.")]
		 public static readonly string TotalQueueTime = name( NAME_PREFIX, "accumulated_queue_time" );

		 [Documented("The accumulated time worker threads have spent processing messages.")]
		 public static readonly string TotalProcessingTime = name( NAME_PREFIX, "accumulated_processing_time" );

		 private readonly MetricRegistry _registry;
		 private readonly Monitors _monitors;
		 private BoltMetricsMonitor _boltMonitor;

		 public BoltMetrics( MetricRegistry registry, Monitors monitors )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._registry = registry;
			  this._monitors = monitors;
		 }

		 public override void Start()
		 {
			  _monitors.addMonitorListener( _boltMonitor );
			  _registry.register( SessionsStarted, ( Gauge<long> ) _boltMonitor.connectionsOpened.get );
			  _registry.register( ConnectionsOpened, ( Gauge<long> ) _boltMonitor.connectionsOpened.get );
			  _registry.register( ConnectionsClosed, ( Gauge<long> ) _boltMonitor.connectionsClosed.get );
			  _registry.register( ConnectionsRunning, ( Gauge<long> ) _boltMonitor.connectionsActive.get );
			  _registry.register( ConnectionsIdle, ( Gauge<long> ) _boltMonitor.connectionsIdle.get );
			  _registry.register( MessagesReceived, ( Gauge<long> ) _boltMonitor.messagesReceived.get );
			  _registry.register( MessagesStarted, ( Gauge<long> ) _boltMonitor.messagesStarted.get );
			  _registry.register( MessagesDone, ( Gauge<long> ) _boltMonitor.messagesDone.get );
			  _registry.register( MessagesFailed, ( Gauge<long> ) _boltMonitor.messagesFailed.get );
			  _registry.register( TotalQueueTime, ( Gauge<long> ) _boltMonitor.queueTime.get );
			  _registry.register( TotalProcessingTime, ( Gauge<long> ) _boltMonitor.processingTime.get );
		 }

		 public override void Stop()
		 {
			  _registry.remove( SessionsStarted );
			  _registry.remove( ConnectionsOpened );
			  _registry.remove( ConnectionsClosed );
			  _registry.remove( ConnectionsIdle );
			  _registry.remove( ConnectionsRunning );
			  _registry.remove( MessagesReceived );
			  _registry.remove( MessagesStarted );
			  _registry.remove( MessagesDone );
			  _registry.remove( MessagesFailed );
			  _registry.remove( TotalQueueTime );
			  _registry.remove( TotalProcessingTime );
			  _monitors.removeMonitorListener( _boltMonitor );
		 }

		 private class BoltMetricsMonitor : BoltConnectionMetricsMonitor
		 {
			 private readonly BoltMetrics _outerInstance;

			 public BoltMetricsMonitor( BoltMetrics outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal readonly AtomicLong ConnectionsOpened = new AtomicLong();
			  internal readonly AtomicLong ConnectionsClosed = new AtomicLong();

			  internal readonly AtomicLong ConnectionsActive = new AtomicLong();
			  internal readonly AtomicLong ConnectionsIdle = new AtomicLong();

			  internal readonly AtomicLong MessagesReceived = new AtomicLong();
			  internal readonly AtomicLong MessagesStarted = new AtomicLong();
			  internal readonly AtomicLong MessagesDone = new AtomicLong();
			  internal readonly AtomicLong MessagesFailed = new AtomicLong();

			  // It will take about 300 million years of queue/processing time to overflow these
			  // Even if we run a million processors concurrently, the instance would need to
			  // run uninterrupted for three hundred years before the monitoring had a hiccup.
			  internal readonly AtomicLong QueueTime = new AtomicLong();
			  internal readonly AtomicLong ProcessingTime = new AtomicLong();

			  public override void ConnectionOpened()
			  {
					ConnectionsOpened.incrementAndGet();
					ConnectionsIdle.incrementAndGet();
			  }

			  public override void ConnectionActivated()
			  {
					ConnectionsActive.incrementAndGet();
					ConnectionsIdle.decrementAndGet();
			  }

			  public override void ConnectionWaiting()
			  {
					ConnectionsIdle.incrementAndGet();
					ConnectionsActive.decrementAndGet();
			  }

			  public override void MessageReceived()
			  {
					MessagesReceived.incrementAndGet();
			  }

			  public override void MessageProcessingStarted( long queueTime )
			  {
					this.QueueTime.addAndGet( queueTime );
					MessagesStarted.incrementAndGet();
			  }

			  public override void MessageProcessingCompleted( long processingTime )
			  {
					this.ProcessingTime.addAndGet( processingTime );
					MessagesDone.incrementAndGet();
			  }

			  public override void MessageProcessingFailed()
			  {
					MessagesFailed.incrementAndGet();
			  }

			  public override void ConnectionClosed()
			  {
					ConnectionsClosed.incrementAndGet();
					ConnectionsIdle.decrementAndGet();
			  }
		 }
	}

}