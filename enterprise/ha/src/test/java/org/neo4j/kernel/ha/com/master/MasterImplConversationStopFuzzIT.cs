﻿using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Kernel.ha.com.master
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using RequestContext = Org.Neo4j.com.RequestContext;
	using ResourceReleaser = Org.Neo4j.com.ResourceReleaser;
	using Org.Neo4j.com;
	using TransactionNotPresentOnMasterException = Org.Neo4j.com.TransactionNotPresentOnMasterException;
	using Org.Neo4j.com;
	using StoreWriter = Org.Neo4j.com.storecopy.StoreWriter;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConversationSPI = Org.Neo4j.Kernel.ha.cluster.ConversationSPI;
	using DefaultConversationSPI = Org.Neo4j.Kernel.ha.cluster.DefaultConversationSPI;
	using IdAllocation = Org.Neo4j.Kernel.ha.id.IdAllocation;
	using ForsetiLockManager = Org.Neo4j.Kernel.impl.enterprise.@lock.forseti.ForsetiLockManager;
	using DumpLocksVisitor = Org.Neo4j.Kernel.impl.locking.DumpLocksVisitor;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using ResourceTypes = Org.Neo4j.Kernel.impl.locking.ResourceTypes;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using ConcurrentAccessException = Org.Neo4j.Kernel.impl.util.collection.ConcurrentAccessException;
	using Org.Neo4j.Kernel.impl.util.collection;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using FormattedLog = Org.Neo4j.Logging.FormattedLog;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;
	using VerboseTimeout = Org.Neo4j.Test.rule.VerboseTimeout;
	using Clocks = Org.Neo4j.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.ClusterSettings.server_id;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.StoreIdTestFactory.newStoreIdForCurrentVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.HaSettings.lock_read_timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;

	/// <summary>
	///  Current test will try to emulate client master conversation lifecycle
	///  starting from handshake till end of the session,
	///  including simulation of idle conversations cleanup and inactive session removal.
	/// 
	///  Workers will try to follow common patterns of client-server communication using defined state machine.
	///  Except common cases state machine will try to simulate abnormal behaviour and will
	///  fall into sleep from time to time emulating inactivates.
	/// 
	/// </summary>
	public class MasterImplConversationStopFuzzIT
	{
		private bool InstanceFieldsInitialized = false;

		public MasterImplConversationStopFuzzIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_scheduler = _life.add( createInitialisedScheduler() );
			Timeout = VerboseTimeout.builder().withTimeout(50, TimeUnit.SECONDS).describeOnFailure(_locks, MasterImplConversationStopFuzzIT.getLocksDescriptionFunction).build();
		}

		 private const int NUMBER_OF_WORKERS = 10;
		 private const int NUMBER_OF_OPERATIONS = 1_000;
		 private const int NUMBER_OF_RESOURCES = 100;

		 public static readonly StoreId StoreId = newStoreIdForCurrentVersion();

		 private readonly LifeSupport _life = new LifeSupport();
		 private readonly ExecutorService _executor = Executors.newFixedThreadPool( NUMBER_OF_WORKERS + 1 );
		 private JobScheduler _scheduler;
		 private readonly Config _config = Config.defaults( stringMap( server_id.name(), "0", lock_read_timeout.name(), "1" ) );
		 private readonly Locks _locks = new ForsetiLockManager( Config.defaults(), Clocks.systemClock(), ResourceTypes.NODE, ResourceTypes.LABEL );

		 private static MasterExecutionStatistic _executionStatistic = new MasterExecutionStatistic();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.VerboseTimeout timeout = org.neo4j.test.rule.VerboseTimeout.builder().withTimeout(50, java.util.concurrent.TimeUnit.SECONDS).describeOnFailure(locks, MasterImplConversationStopFuzzIT::getLocksDescriptionFunction).build();
		 public VerboseTimeout Timeout;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  _life.shutdown();
			  _executor.shutdownNow();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleRandomizedLoad() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleRandomizedLoad()
		 {
			  // Given
			  DefaultConversationSPI conversationSPI = new DefaultConversationSPI( _locks, _scheduler );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ExposedConversationManager conversationManager = new ExposedConversationManager(conversationSPI, config, 100, 0);
			  ExposedConversationManager conversationManager = new ExposedConversationManager( this, conversationSPI, _config, 100, 0 );

			  ConversationTestMasterSPI conversationTestMasterSPI = new ConversationTestMasterSPI();
			  MasterImpl master = new MasterImpl( conversationTestMasterSPI, conversationManager, ( new Monitors() ).newMonitor(typeof(MasterImpl.Monitor)), _config );
			  _life.add( conversationManager );
			  _life.start();

			  ConversationKiller conversationKiller = new ConversationKiller( conversationManager );
			  _executor.submit( conversationKiller );
			  IList<Callable<Void>> slaveWorkers = Workers( master, NUMBER_OF_WORKERS );
			  IList<Future<Void>> workers = _executor.invokeAll( slaveWorkers );

			  // Wait for all workers to complete
			  foreach ( Future<Void> future in workers )
			  {
					future.get();
			  }
			  conversationKiller.Stop();

			  assertTrue( _executionStatistic.SuccessfulExecution );
		 }

		 private static string GetLocksDescriptionFunction( Locks locks )
		 {
			  StringWriter stringWriter = new StringWriter();
			  locks.Accept( new DumpLocksVisitor( FormattedLog.withUTCTimeZone().toWriter(stringWriter) ) );
			  return stringWriter.ToString();
		 }

		 private IList<Callable<Void>> Workers( MasterImpl master, int numWorkers )
		 {
			  LinkedList<Callable<Void>> workers = new LinkedList<Callable<Void>>();
			  for ( int i = 0; i < numWorkers; i++ )
			  {
					workers.AddLast( new SlaveEmulatorWorker( master, i ) );
			  }
			  return workers;
		 }

		 internal class SlaveEmulatorWorker : Callable<Void>
		 {
			  internal readonly Random Random;
			  internal readonly MasterImpl Master;
			  internal readonly int MachineId;

			  internal State State = State.Uninitialized;
			  internal readonly long LastTx = 0;
			  internal long Epoch;
			  internal RequestContext RequestContext;

			  internal abstract class State
			  {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//               UNINITIALIZED { State next(SlaveEmulatorWorker worker) { HandshakeResult handshake = worker.master.handshake(worker.lastTx, org.neo4j.storageengine.api.StoreId).response(); worker.epoch = handshake.epoch(); return IDLE; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//               IDLE { State next(SlaveEmulatorWorker worker) throws Exception { if(lowProbabilityEvent(worker)) { return UNINITIALIZED; } else if(lowProbabilityEvent(worker)) { return commit(worker, new org.neo4j.com.RequestContext(worker.epoch, worker.machineId, -1, worker.lastTx, 0)); } else { try { worker.master.newLockSession(worker.newRequestContext()); return IN_SESSION; } catch(org.neo4j.internal.kernel.api.exceptions.TransactionFailureException e) { if(e.getCause() instanceof org.neo4j.kernel.impl.util.collection.ConcurrentAccessException) { executionStatistic.reportAlreadyInUseError(); return IDLE; } else { throw e; } } } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//               IN_SESSION { State next(SlaveEmulatorWorker worker) throws Exception { if(lowProbabilityEvent(worker)) { return UNINITIALIZED; } else { int i = worker.random.nextInt(10); if(i >= 5) { return commit(worker, worker.requestContext); } else if(i >= 4) { worker.master.acquireExclusiveLock(worker.requestContext, org.neo4j.kernel.impl.locking.ResourceTypes.NODE, randomResource(worker)); return IN_SESSION; } else if(i >= 1) { worker.master.acquireSharedLock(worker.requestContext, org.neo4j.kernel.impl.locking.ResourceTypes.NODE, randomResource(worker)); return IN_SESSION; } else { endLockSession(worker); return IDLE; } } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//               CLOSING_SESSION { State next(SlaveEmulatorWorker worker) { if(lowProbabilityEvent(worker)) { return UNINITIALIZED; } else { endLockSession(worker); return IDLE; } } };

					private static readonly IList<State> valueList = new List<State>();

					static State()
					{
						valueList.Add( UNINITIALIZED );
						valueList.Add( IDLE );
						valueList.Add( IN_SESSION );
						valueList.Add( CLOSING_SESSION );
					}

					public enum InnerEnum
					{
						UNINITIALIZED,
						IDLE,
						IN_SESSION,
						CLOSING_SESSION
					}

					public readonly InnerEnum innerEnumValue;
					private readonly string nameValue;
					private readonly int ordinalValue;
					private static int nextOrdinal = 0;

					private State( string name, InnerEnum innerEnum )
					{
						nameValue = name;
						ordinalValue = nextOrdinal++;
						innerEnumValue = innerEnum;
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract State next(SlaveEmulatorWorker worker) throws Exception;
					internal abstract State next( SlaveEmulatorWorker worker );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected State commit(SlaveEmulatorWorker worker, org.neo4j.com.RequestContext requestContext) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
					protected internal State Commit( SlaveEmulatorWorker worker, Org.Neo4j.com.RequestContext requestContext )
					{
						 try
						 {
							  worker.Master.commit( requestContext, mock( typeof( TransactionRepresentation ) ) );
							  _executionStatistic.reportCommittedOperation();
							  return CLOSING_SESSION;
						 }
						 catch ( TransactionNotPresentOnMasterException )
						 {
							  _executionStatistic.reportTransactionNotPresentError();
							  return IDLE;
						 }
					}

				  public static IList<State> values()
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

				  public static State valueOf( string name )
				  {
					  foreach ( State enumInstance in State.valueList )
					  {
						  if ( enumInstance.nameValue == name )
						  {
							  return enumInstance;
						  }
					  }
					  throw new System.ArgumentException( name );
				  }
			  }

			  internal static bool LowProbabilityEvent( SlaveEmulatorWorker worker )
			  {
					return worker.Random.Next( 100 ) <= 1;
			  }

			  internal static long RandomResource( SlaveEmulatorWorker worker )
			  {
					return worker.Random.Next( NUMBER_OF_RESOURCES );
			  }

			  internal SlaveEmulatorWorker( MasterImpl master, int clientNumber )
			  {
					this.MachineId = clientNumber;
					this.Random = new Random( MachineId );
					this.Master = master;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void call() throws Exception
			  public override Void Call()
			  {
					for ( int i = 0; i < NUMBER_OF_OPERATIONS; i++ )
					{
						 State = State.next( this );
					}
					return null;
			  }

			  internal virtual RequestContext NewRequestContext()
			  {
					return RequestContext = new RequestContext( Epoch, MachineId, NewLockSessionId(), LastTx, Random.Next() );
			  }

			  internal virtual int NewLockSessionId()
			  {
					return Random.Next();
			  }

			  internal static void EndLockSession( SlaveEmulatorWorker worker )
			  {
					bool successfulSession = worker.Random.nextBoolean();
					worker.Master.endLockSession( worker.RequestContext, successfulSession );
			  }
		 }

		 internal class ConversationTestMasterSPI : MasterImpl.SPI
		 {
			  public virtual bool Accessible
			  {
				  get
				  {
						return true;
				  }
			  }

			  public override StoreId StoreId()
			  {
					return StoreId;
			  }

			  public override long ApplyPreparedTransaction( TransactionRepresentation preparedTransaction )
			  {
					// sleeping here and hope to be noticed by conversation killer.
					Sleep();
					return 0;
			  }

			  internal virtual void Sleep()
			  {
					try
					{
						 Thread.Sleep( 20 );
					}
					catch ( InterruptedException e )
					{
						 throw new Exception( e );
					}
			  }

			  public override long GetTransactionChecksum( long txId )
			  {
					return 0;
			  }

			  public override Response<T> PackEmptyResponse<T>( T response )
			  {
					return new TransactionObligationResponse<T>( response, StoreId, Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID, Org.Neo4j.com.ResourceReleaser_Fields.NoOp );
			  }

			  public override Response<T> PackTransactionObligationResponse<T>( RequestContext context, T response )
			  {
					return PackEmptyResponse( response );
			  }

			  public override IdAllocation AllocateIds( IdType idType )
			  {
					throw new System.NotSupportedException();
			  }

			  public override int? CreateRelationshipType( string name )
			  {
					throw new System.NotSupportedException();
			  }

			  public override RequestContext FlushStoresAndStreamStoreFiles( StoreWriter writer )
			  {
					throw new System.NotSupportedException();
			  }

			  public override Response<T> PackTransactionStreamResponse<T>( RequestContext context, T response )
			  {
					throw new System.NotSupportedException();
			  }

			  public override int GetOrCreateLabel( string name )
			  {
					throw new System.NotSupportedException();
			  }

			  public override int GetOrCreateProperty( string name )
			  {
					throw new System.NotSupportedException();
			  }
		 }

		 /// <summary>
		 /// This emulates the MasterServer behavior of killing conversations after they have not had traffic sent on them for
		 /// a certain time
		 /// </summary>
		 private class ConversationKiller : ThreadStart
		 {
			  internal volatile bool Running = true;
			  internal readonly ConversationManager ConversationManager;

			  internal ConversationKiller( ConversationManager conversationManager )
			  {
					this.ConversationManager = conversationManager;
			  }

			  public override void Run()
			  {
					try
					{
						 while ( Running )
						 {
							  IEnumerator<RequestContext> conversationIterator = ConversationManager.ActiveContexts.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  if ( conversationIterator.hasNext() )
							  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
									RequestContext next = conversationIterator.next();
									ConversationManager.end( next );
							  }
							  try
							  {
									Thread.Sleep( 10 );
							  }
							  catch ( InterruptedException e )
							  {
									throw new Exception( e );
							  }
						 }
					}
					catch ( Exception e )
					{
						 throw new Exception( "Conversation killer failed.", e );
					}
			  }

			  public virtual void Stop()
			  {
					Running = false;
			  }
		 }

		 private class ExposedConversationManager : ConversationManager
		 {
			 private readonly MasterImplConversationStopFuzzIT _outerInstance;


			  internal TimedRepository<RequestContext, Conversation> ConversationStore;

			  internal ExposedConversationManager( MasterImplConversationStopFuzzIT outerInstance, ConversationSPI spi, Config config, int activityCheckInterval, int lockTimeoutAddition ) : base( spi, config, activityCheckInterval, lockTimeoutAddition )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override TimedRepository<RequestContext, Conversation> CreateConversationStore()
			  {
					ConversationStore = new TimedRepository<RequestContext, Conversation>( ConversationFactory, ConversationReaper, 1, Clocks.systemClock() );
					return ConversationStore;
			  }
		 }

		 private class MasterExecutionStatistic
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AtomicLong AlreadyInUseErrorsConflict = new AtomicLong();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AtomicLong TransactionNotPresentErrorsConflict = new AtomicLong();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AtomicLong CommittedOperationsConflict = new AtomicLong();

			  public virtual void ReportAlreadyInUseError()
			  {
					AlreadyInUseErrorsConflict.incrementAndGet();
			  }

			  public virtual void ReportTransactionNotPresentError()
			  {
					TransactionNotPresentErrorsConflict.incrementAndGet();
			  }

			  public virtual void ReportCommittedOperation()
			  {
					CommittedOperationsConflict.incrementAndGet();
			  }

			  public virtual AtomicLong AlreadyInUseErrors
			  {
				  get
				  {
						return AlreadyInUseErrorsConflict;
				  }
			  }

			  public virtual AtomicLong TransactionNotPresentErrors
			  {
				  get
				  {
						return TransactionNotPresentErrorsConflict;
				  }
			  }

			  public virtual AtomicLong CommittedOperations
			  {
				  get
				  {
						return CommittedOperationsConflict;
				  }
			  }

			  public virtual bool SuccessfulExecution
			  {
				  get
				  {
						return CommittedOperationsConflict.get() > ((AlreadyInUseErrorsConflict.get() + TransactionNotPresentErrorsConflict.get()) * 10);
				  }
			  }
		 }
	}

}