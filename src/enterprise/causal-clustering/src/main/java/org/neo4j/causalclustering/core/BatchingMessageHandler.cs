using System;
using System.Collections.Generic;
using System.Diagnostics;
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
namespace Neo4Net.causalclustering.core
{

	using Neo4Net.causalclustering.core.BoundedPriorityQueue;
	using ContinuousJob = Neo4Net.causalclustering.core.consensus.ContinuousJob;
	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using RaftMessages_AppendEntries = Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries;
	using RaftMessages_NewEntry = Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry;
	using Neo4Net.causalclustering.core.consensus;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using ComposableMessageHandler = Neo4Net.causalclustering.messaging.ComposableMessageHandler;
	using Neo4Net.causalclustering.messaging;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Streams = Neo4Net.Stream.Streams;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.BoundedPriorityQueue.Result.OK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.ArrayUtil.lastOf;

	/// <summary>
	/// This class gets Raft messages as input and queues them up for processing. Some messages are
	/// batched together before they are forwarded to the Raft machine, for reasons of efficiency.
	/// </summary>
	internal class BatchingMessageHandler : ThreadStart, LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<JavaToDotNetGenericWildcard>>
	{
		 public class Config
		 {
			  internal readonly int MaxBatchCount;
			  internal readonly long MaxBatchBytes;

			  internal Config( int maxBatchCount, long maxBatchBytes )
			  {
					this.MaxBatchCount = maxBatchCount;
					this.MaxBatchBytes = maxBatchBytes;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Neo4Net.causalclustering.messaging.LifecycleMessageHandler<Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> handler;
		 private readonly LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> _handler;
		 private readonly Log _log;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final BoundedPriorityQueue<Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> inQueue;
		 private readonly BoundedPriorityQueue<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> _inQueue;
		 private readonly ContinuousJob _job;
		 private readonly IList<ReplicatedContent> _contentBatch; // reused for efficiency
		 private readonly IList<RaftLogEntry> _entryBatch; // reused for efficiency
		 private readonly Config _batchConfig;

		 private volatile bool _stopped;
		 private volatile BoundedPriorityQueue.Result _lastResult = OK;
		 private AtomicLong _droppedCount = new AtomicLong();

		 internal BatchingMessageHandler<T1>( LifecycleMessageHandler<T1> handler, BoundedPriorityQueue.Config inQueueConfig, Config batchConfig, System.Func<ThreadStart, ContinuousJob> jobFactory, LogProvider logProvider )
		 {
			  this._handler = handler;
			  this._log = logProvider.getLog( this.GetType() );
			  this._batchConfig = batchConfig;
			  this._contentBatch = new List<ReplicatedContent>( batchConfig.MaxBatchCount );
			  this._entryBatch = new List<RaftLogEntry>( batchConfig.MaxBatchCount );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: this.inQueue = new BoundedPriorityQueue<>(inQueueConfig, ContentSize::of, new MessagePriority());
			  this._inQueue = new BoundedPriorityQueue<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>>( inQueueConfig, ContentSize.of, new MessagePriority( this ) );
			  this._job = jobFactory( this );
		 }

		 internal static ComposableMessageHandler Composable( BoundedPriorityQueue.Config inQueueConfig, Config batchConfig, System.Func<ThreadStart, ContinuousJob> jobSchedulerFactory, LogProvider logProvider )
		 {
			  return @delegate => new BatchingMessageHandler( @delegate, inQueueConfig, batchConfig, jobSchedulerFactory, logProvider );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start(Neo4Net.causalclustering.identity.ClusterId clusterId) throws Throwable
		 public override void Start( ClusterId clusterId )
		 {
			  _handler.start( clusterId );
			  _job.start();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  _stopped = true;
			  _handler.stop();
			  _job.stop();
		 }

		 public override void Handle<T1>( RaftMessages_ReceivedInstantClusterIdAwareMessage<T1> message )
		 {
			  if ( _stopped )
			  {
					_log.debug( "This handler has been stopped, dropping the message: %s", message );
					return;
			  }

			  BoundedPriorityQueue.Result result = _inQueue.offer( message );
			  LogQueueState( result );
		 }

		 private void LogQueueState( BoundedPriorityQueue.Result result )
		 {
			  if ( result != OK )
			  {
					_droppedCount.incrementAndGet();
			  }

			  if ( result != _lastResult )
			  {
					if ( result == OK )
					{
						 _log.info( "Raft in-queue not dropping messages anymore. Dropped %d messages.", _droppedCount.getAndSet( 0 ) );
					}
					else
					{
						 _log.warn( "Raft in-queue dropping messages after: " + result );
					}
					_lastResult = result;
			  }
		 }

		 public override void Run()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Optional<Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> baseMessage;
			  Optional<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> baseMessage;
			  try
			  {
					baseMessage = _inQueue.poll( 1, SECONDS );
			  }
			  catch ( InterruptedException e )
			  {
					_log.warn( "Not expecting to be interrupted.", e );
					return;
			  }

			  if ( !baseMessage.Present )
			  {
					return;
			  }

			  RaftMessages_ReceivedInstantClusterIdAwareMessage batchedMessage = baseMessage.get().message().dispatch(new BatchingHandler(this, baseMessage.get()));

			  _handler.handle( batchedMessage == null ? baseMessage.get() : batchedMessage );
		 }

		 /// <summary>
		 /// Batches together the content of NewEntry.Requests for efficient handling.
		 /// </summary>
		 private Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_BatchRequest BatchNewEntries( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request first )
		 {
			  _contentBatch.Clear();

			  _contentBatch.Add( first.Content() );
			  long totalBytes = first.Content().size().GetValueOrDefault(0L);

			  while ( _contentBatch.Count < _batchConfig.maxBatchCount )
			  {
					Optional<Removable<Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request>> peeked = PeekNext( typeof( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request ) );

					if ( !peeked.Present )
					{
						 break;
					}

					ReplicatedContent content = peeked.get().get().content();

					if ( content.Size().HasValue && (totalBytes + content.Size().Value) > _batchConfig.maxBatchBytes )
					{
						 break;
					}

					_contentBatch.Add( content );

					bool removed = peeked.get().remove();
					Debug.Assert( removed ); // single consumer assumed
			  }

			  /*
			   * Individual NewEntry.Requests are batched together into a BatchRequest to take advantage
			   * of group commit into the Raft log and any other batching benefits.
			   */
			  return new Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_BatchRequest( _contentBatch );
		 }

		 private Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request BatchAppendEntries( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request first )
		 {
			  _entryBatch.Clear();

			  long totalBytes = 0;

			  foreach ( RaftLogEntry entry in first.Entries() )
			  {
					totalBytes += entry.Content().size().GetValueOrDefault(0L);
					_entryBatch.Add( entry );
			  }

			  long leaderCommit = first.LeaderCommit();
			  long lastTerm = lastOf( first.Entries() ).term();

			  while ( _entryBatch.Count < _batchConfig.maxBatchCount )
			  {
					Optional<Removable<Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request>> peeked = PeekNext( typeof( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request ) );

					if ( !peeked.Present )
					{
						 break;
					}

					Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request = peeked.get().get();

					if ( request.Entries().Length == 0 || !ConsecutiveOrigin(first, request, _entryBatch.Count) )
					{
						 // probe (RaftLogShipper#sendEmpty) or leader switch
						 break;
					}

					Debug.Assert( lastTerm == request.PrevLogTerm() );

					// note that this code is backwards compatible, but AppendEntries.Request generation by the leader
					// will be changed to only generate single entry AppendEntries.Requests and the code here
					// will be responsible for the batching of the individual and consecutive entries

					RaftLogEntry[] entries = request.Entries();
					lastTerm = lastOf( entries ).term();

					if ( entries.Length + _entryBatch.Count > _batchConfig.maxBatchCount )
					{
						 break;
					}

					long requestBytes = java.util.entries.Select( entry => entry.content().size().orElse(0L) ).Sum();

					if ( requestBytes > 0 && ( totalBytes + requestBytes ) > _batchConfig.maxBatchBytes )
					{
						 break;
					}

					( ( IList<RaftLogEntry> )_entryBatch ).AddRange( Arrays.asList( entries ) );
					totalBytes += requestBytes;
					leaderCommit = max( leaderCommit, request.LeaderCommit() );

					bool removed = peeked.get().remove();
					Debug.Assert( removed ); // single consumer assumed
			  }

			  return new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( first.From(), first.LeaderTerm(), first.PrevLogIndex(), first.PrevLogTerm(), _entryBatch.toArray(RaftLogEntry.empty), leaderCommit );
		 }

		 private bool ConsecutiveOrigin( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request first, Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request, int currentSize )
		 {
			  if ( request.LeaderTerm() != first.LeaderTerm() )
			  {
					return false;
			  }
			  else
			  {
					return request.PrevLogIndex() == first.PrevLogIndex() + currentSize;
			  }
		 }

		 private Optional<Removable<M>> PeekNext<M>( Type acceptedType )
		 {
				 acceptedType = typeof( M );
			  return _inQueue.peek().filter(r => acceptedType.IsInstanceOfType(r.get().message())).map(r => r.map(m => acceptedType.cast(m.message())));
		 }

		 private class ContentSize : Neo4Net.causalclustering.core.consensus.RaftMessages_HandlerAdaptor<long, Exception>
		 {
			  internal static readonly ContentSize Instance = new ContentSize();

			  internal ContentSize()
			  {
			  }

			  internal static long Of<T1>( RaftMessages_ReceivedInstantClusterIdAwareMessage<T1> message )
			  {
					long? dispatch = message.dispatch( Instance );
					return dispatch == null ? 0L : dispatch.Value;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<long> handle(Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request request) throws RuntimeException
			  public override long? Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request request )
			  {
					return request.Content().size().GetValueOrDefault(0L);
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<long> handle(Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request) throws RuntimeException
			  public override long? Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request )
			  {
					long totalSize = 0L;
					foreach ( RaftLogEntry entry in request.Entries() )
					{
						 if ( entry.Content().size().HasValue )
						 {
							  totalSize += entry.Content().size().Value;
						 }
					}
					return totalSize;
			  }
		 }

		 private class MessagePriority : Neo4Net.causalclustering.core.consensus.RaftMessages_HandlerAdaptor<int, Exception>, IComparer<RaftMessages_ReceivedInstantClusterIdAwareMessage<JavaToDotNetGenericWildcard>>
		 {
			 private readonly BatchingMessageHandler _outerInstance;

			 public MessagePriority( BatchingMessageHandler outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal readonly int? BasePriority = 10; // lower number means higher priority

			  public override int? Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request )
			  {

					// 0 length means this is a heartbeat, so let it be handled with higher priority
					return request.Entries().Length == 0 ? BasePriority : 20;
			  }

			  public override int? Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request request )
			  {
					return 30;
			  }

			  public override int Compare<T1, T2>( RaftMessages_ReceivedInstantClusterIdAwareMessage<T1> messageA, RaftMessages_ReceivedInstantClusterIdAwareMessage<T2> messageB )
			  {
					int priorityA = GetPriority( messageA );
					int priorityB = GetPriority( messageB );

					return Integer.compare( priorityA, priorityB );
			  }

			  internal virtual int GetPriority<T1>( RaftMessages_ReceivedInstantClusterIdAwareMessage<T1> message )
			  {
					int? priority = message.dispatch( this );
					return priority == null ? BasePriority.Value : priority.Value;
			  }
		 }

		 private class BatchingHandler : Neo4Net.causalclustering.core.consensus.RaftMessages_HandlerAdaptor<RaftMessages_ReceivedInstantClusterIdAwareMessage, Exception>
		 {
			 private readonly BatchingMessageHandler _outerInstance;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?> baseMessage;
			  internal readonly RaftMessages_ReceivedInstantClusterIdAwareMessage<object> BaseMessage;

			  internal BatchingHandler<T1>( BatchingMessageHandler outerInstance, RaftMessages_ReceivedInstantClusterIdAwareMessage<T1> baseMessage )
			  {
				  this._outerInstance = outerInstance;
					this.BaseMessage = baseMessage;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage handle(Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request request) throws RuntimeException
			  public override RaftMessages_ReceivedInstantClusterIdAwareMessage Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request request )
			  {
					Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_BatchRequest newEntryBatch = outerInstance.batchNewEntries( request );
					return RaftMessages_ReceivedInstantClusterIdAwareMessage.of( BaseMessage.receivedAt(), BaseMessage.clusterId(), newEntryBatch );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage handle(Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request) throws RuntimeException
			  public override RaftMessages_ReceivedInstantClusterIdAwareMessage Handle( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request )
			  {
					if ( request.Entries().Length == 0 )
					{
						 // this is a heartbeat, so let it be solo handled
						 return null;
					}

					Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request appendEntriesBatch = outerInstance.batchAppendEntries( request );
					return RaftMessages_ReceivedInstantClusterIdAwareMessage.of( BaseMessage.receivedAt(), BaseMessage.clusterId(), appendEntriesBatch );
			  }
		 }
	}

}