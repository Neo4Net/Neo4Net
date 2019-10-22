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
namespace Neo4Net.cluster.logging
{

	using NamedThreadFactory = Neo4Net.Helpers.NamedThreadFactory;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using AsyncLogEvent = Neo4Net.Logging.async.AsyncLogEvent;
	using AsyncLogProvider = Neo4Net.Logging.async.AsyncLogProvider;
	using Neo4Net.Utils.Concurrent;
	using Neo4Net.Utils.Concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.util.FeatureToggles.flag;

	public class AsyncLogging : LifecycleAdapter, System.Action<AsyncLogEvent>, AsyncEvents.Monitor
	{
		 private static readonly bool _enabled = flag( typeof( AsyncLogging ), "ENABLED", true );

		 public static LogProvider Provider( LifeSupport life, LogProvider provider )
		 {
			  if ( _enabled )
			  {
					if ( provider is NullLogProvider )
					{
						 return provider;
					}
					return new AsyncLogProvider( life.Add( new AsyncLogging( provider.GetLog( typeof( AsyncLogging ) ) ) ).eventSender(), provider );
			  }
			  else
			  {
					return provider;
			  }
		 }

		 private readonly Log _metaLog;
		 private readonly AsyncEvents<AsyncLogEvent> _events;
		 private long _highCount;
		 private ExecutorService _executor;

		 internal AsyncLogging( Log metaLog )
		 {
			  this._metaLog = metaLog;
			  this._events = new AsyncEvents<AsyncLogEvent>( this, this );
		 }

		 public override void Accept( AsyncLogEvent @event )
		 {
			  @event.Process();
		 }

		 public override void Start()
		 {
			  _highCount = 0;
			  _executor = Executors.newSingleThreadExecutor( new NamedThreadFactory( this.GetType().Name ) );
			  _executor.submit( _events );
			  _events.awaitStartup();
		 }

		 public override void Stop()
		 {
			  _events.shutdown();
			  _executor.shutdown();
			  _events.awaitTermination();
		 }

		 public override void EventCount( long count )
		 {
			  if ( _metaLog.DebugEnabled )
			  {
					if ( count > _highCount )
					{
						 _metaLog.debug( "High mark increasing from %d to %d events", _highCount, count );
						 _highCount = count;
					}
			  }
		 }

		 internal virtual AsyncEventSender<AsyncLogEvent> EventSender()
		 {
			  return _events;
		 }
	}

}