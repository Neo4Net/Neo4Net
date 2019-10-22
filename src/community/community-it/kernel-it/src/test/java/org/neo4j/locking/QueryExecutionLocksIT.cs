using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Locking
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Neo4Net.GraphDb.Label;
	using Lock = Neo4Net.GraphDb.Lock;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using CursorFactory = Neo4Net.Internal.Kernel.Api.CursorFactory;
	using ExecutionStatistics = Neo4Net.Internal.Kernel.Api.ExecutionStatistics;
	using ExplicitIndexRead = Neo4Net.Internal.Kernel.Api.ExplicitIndexRead;
	using ExplicitIndexWrite = Neo4Net.Internal.Kernel.Api.ExplicitIndexWrite;
	using Locks = Neo4Net.Internal.Kernel.Api.Locks;
	using NodeCursor = Neo4Net.Internal.Kernel.Api.NodeCursor;
	using Procedures = Neo4Net.Internal.Kernel.Api.Procedures;
	using PropertyCursor = Neo4Net.Internal.Kernel.Api.PropertyCursor;
	using Read = Neo4Net.Internal.Kernel.Api.Read;
	using RelationshipScanCursor = Neo4Net.Internal.Kernel.Api.RelationshipScanCursor;
	using SchemaRead = Neo4Net.Internal.Kernel.Api.SchemaRead;
	using SchemaWrite = Neo4Net.Internal.Kernel.Api.SchemaWrite;
	using TokenRead = Neo4Net.Internal.Kernel.Api.TokenRead;
	using TokenWrite = Neo4Net.Internal.Kernel.Api.TokenWrite;
	using Write = Neo4Net.Internal.Kernel.Api.Write;
	using InvalidTransactionTypeKernelException = Neo4Net.Internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using SchemaKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using AuthSubject = Neo4Net.Internal.Kernel.Api.security.AuthSubject;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using SecurityContext = Neo4Net.Internal.Kernel.Api.security.SecurityContext;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using Statement = Neo4Net.Kernel.api.Statement;
	using DbmsOperations = Neo4Net.Kernel.api.dbms.DbmsOperations;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using TxStateHolder = Neo4Net.Kernel.api.txstate.TxStateHolder;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ClockContext = Neo4Net.Kernel.Impl.Api.ClockContext;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using IPropertyContainerLocker = Neo4Net.Kernel.impl.coreapi.PropertyContainerLocker;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using Neo4NetTransactionalContextFactory = Neo4Net.Kernel.impl.query.Neo4NetTransactionalContextFactory;
	using QueryExecutionEngine = Neo4Net.Kernel.impl.query.QueryExecutionEngine;
	using QueryExecutionKernelException = Neo4Net.Kernel.impl.query.QueryExecutionKernelException;
	using TransactionalContext = Neo4Net.Kernel.impl.query.TransactionalContext;
	using TransactionalContextFactory = Neo4Net.Kernel.impl.query.TransactionalContextFactory;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using StatisticProvider = Neo4Net.Kernel.impl.query.statistic.StatisticProvider;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.EMPTY_MAP;

	public class QueryExecutionLocksIT
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.EmbeddedDatabaseRule databaseRule = new org.Neo4Net.test.rule.EmbeddedDatabaseRule();
		 public EmbeddedDatabaseRule DatabaseRule = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noLocksTakenForQueryWithoutAnyIndexesUsage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NoLocksTakenForQueryWithoutAnyIndexesUsage()
		 {
			  string query = "MATCH (n) return count(n)";
			  IList<LockOperationRecord> lockOperationRecords = TraceQueryLocks( query );
			  assertThat( "Observed list of lock operations is: " + lockOperationRecords, lockOperationRecords, @is( empty() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void takeLabelLockForQueryWithIndexUsages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TakeLabelLockForQueryWithIndexUsages()
		 {
			  string labelName = "Human";
			  Label human = Label.label( labelName );
			  string propertyKey = "name";
			  CreateIndex( human, propertyKey );

			  using ( Transaction transaction = DatabaseRule.beginTx() )
			  {
					Node node = DatabaseRule.createNode( human );
					node.SetProperty( propertyKey, RandomStringUtils.randomAscii( 10 ) );
					transaction.Success();
			  }

			  string query = "MATCH (n:" + labelName + ") where n." + propertyKey + " = \"Fry\" RETURN n ";

			  IList<LockOperationRecord> lockOperationRecords = TraceQueryLocks( query );
			  assertThat( "Observed list of lock operations is: " + lockOperationRecords, lockOperationRecords, hasSize( 1 ) );

			  LockOperationRecord operationRecord = lockOperationRecords[0];
			  assertTrue( operationRecord.Acquisition );
			  assertFalse( operationRecord.Exclusive );
			  assertEquals( ResourceTypes.LABEL, operationRecord.ResourceType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reTakeLabelLockForQueryWithIndexUsagesWhenSchemaStateWasUpdatedDuringLockOperations() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReTakeLabelLockForQueryWithIndexUsagesWhenSchemaStateWasUpdatedDuringLockOperations()
		 {
			  string labelName = "Robot";
			  Label robot = Label.label( labelName );
			  string propertyKey = "name";
			  CreateIndex( robot, propertyKey );

			  using ( Transaction transaction = DatabaseRule.beginTx() )
			  {
					Node node = DatabaseRule.createNode( robot );
					node.SetProperty( propertyKey, RandomStringUtils.randomAscii( 10 ) );
					transaction.Success();
			  }

			  string query = "MATCH (n:" + labelName + ") where n." + propertyKey + " = \"Bender\" RETURN n ";

			  LockOperationListener lockOperationListener = new OnceSchemaFlushListener( this );
			  IList<LockOperationRecord> lockOperationRecords = TraceQueryLocks( query, lockOperationListener );
			  assertThat( "Observed list of lock operations is: " + lockOperationRecords, lockOperationRecords, hasSize( 3 ) );

			  LockOperationRecord operationRecord = lockOperationRecords[0];
			  assertTrue( operationRecord.Acquisition );
			  assertFalse( operationRecord.Exclusive );
			  assertEquals( ResourceTypes.LABEL, operationRecord.ResourceType );

			  LockOperationRecord operationRecord1 = lockOperationRecords[1];
			  assertFalse( operationRecord1.Acquisition );
			  assertFalse( operationRecord1.Exclusive );
			  assertEquals( ResourceTypes.LABEL, operationRecord1.ResourceType );

			  LockOperationRecord operationRecord2 = lockOperationRecords[2];
			  assertTrue( operationRecord2.Acquisition );
			  assertFalse( operationRecord2.Exclusive );
			  assertEquals( ResourceTypes.LABEL, operationRecord2.ResourceType );
		 }

		 private void CreateIndex( Label label, string propertyKey )
		 {
			  using ( Transaction transaction = DatabaseRule.beginTx() )
			  {
					DatabaseRule.schema().indexFor(label).on(propertyKey).create();
					transaction.Success();
			  }
			  using ( Transaction ignored = DatabaseRule.beginTx() )
			  {
					DatabaseRule.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<LockOperationRecord> traceQueryLocks(String query, LockOperationListener... listeners) throws org.Neo4Net.kernel.impl.query.QueryExecutionKernelException
		 private IList<LockOperationRecord> TraceQueryLocks( string query, params LockOperationListener[] listeners )
		 {
			  GraphDatabaseQueryService graph = DatabaseRule.resolveDependency( typeof( GraphDatabaseQueryService ) );
			  QueryExecutionEngine executionEngine = DatabaseRule.resolveDependency( typeof( QueryExecutionEngine ) );
			  using ( InternalTransaction tx = graph.BeginTransaction( KernelTransaction.Type.@implicit, LoginContext.AUTH_DISABLED ) )
			  {
					TransactionalContextWrapper context = new TransactionalContextWrapper( CreateTransactionContext( graph, tx, query ), listeners );
					executionEngine.ExecuteQuery( query, VirtualValues.emptyMap(), context );
					return new List<LockOperationRecord>( context.RecordingLocks.LockOperationRecords );
			  }
		 }

		 private TransactionalContext CreateTransactionContext( GraphDatabaseQueryService graph, InternalTransaction tx, string query )
		 {
			  IPropertyContainerLocker locker = new IPropertyContainerLocker();
			  TransactionalContextFactory contextFactory = Neo4NetTransactionalContextFactory.create( graph, locker );
			  return contextFactory.NewContext( ClientConnectionInfo.EMBEDDED_CONNECTION, tx, query, EMPTY_MAP );
		 }

		 private class TransactionalContextWrapper : TransactionalContext
		 {

			  internal readonly TransactionalContext Delegate;
			  internal readonly IList<LockOperationRecord> RecordedLocks;
			  internal readonly LockOperationListener[] Listeners;
			  internal RecordingLocks RecordingLocks;

			  internal TransactionalContextWrapper( TransactionalContext @delegate, params LockOperationListener[] listeners ) : this( @delegate, new List<>(), listeners )
			  {
			  }

			  internal TransactionalContextWrapper( TransactionalContext @delegate, IList<LockOperationRecord> recordedLocks, params LockOperationListener[] listeners )
			  {
					this.Delegate = @delegate;
					this.RecordedLocks = recordedLocks;
					this.Listeners = listeners;
			  }

			  public override ExecutingQuery ExecutingQuery()
			  {
					return Delegate.executingQuery();
			  }

			  public override DbmsOperations DbmsOperations()
			  {
					return Delegate.dbmsOperations();
			  }

			  public override KernelTransaction KernelTransaction()
			  {
					if ( RecordingLocks == null )
					{
						 RecordingLocks = new RecordingLocks( Delegate.kernelTransaction().locks(), new IList<LockOperationListener> { Listeners }, RecordedLocks );
					}
					return new DelegatingTransaction( Delegate.kernelTransaction(), RecordingLocks );
			  }

			  public virtual bool TopLevelTx
			  {
				  get
				  {
						return Delegate.TopLevelTx;
				  }
			  }

			  public override void Close( bool success )
			  {
					Delegate.close( success );
			  }

			  public override void Terminate()
			  {
					Delegate.terminate();
			  }

			  public override void CommitAndRestartTx()
			  {
					Delegate.commitAndRestartTx();
			  }

			  public override void CleanForReuse()
			  {
					Delegate.cleanForReuse();
			  }

			  public virtual TransactionalContext OrBeginNewIfClosed
			  {
				  get
				  {
						if ( Open )
						{
							 return this;
						}
						else
						{
							 return new TransactionalContextWrapper( Delegate.OrBeginNewIfClosed, RecordedLocks, Listeners );
						}
				  }
			  }

			  public virtual bool Open
			  {
				  get
				  {
						return Delegate.Open;
				  }
			  }

			  public override GraphDatabaseQueryService Graph()
			  {
					return Delegate.graph();
			  }

			  public override Statement Statement()
			  {
					return Delegate.statement();
			  }

			  public override void Check()
			  {
					Delegate.check();
			  }

			  public override TxStateHolder StateView()
			  {
					return Delegate.stateView();
			  }

			  public override Lock AcquireWriteLock( IPropertyContainer p )
			  {
					return Delegate.acquireWriteLock( p );
			  }

			  public override SecurityContext SecurityContext()
			  {
					return Delegate.securityContext();
			  }

			  public override StatisticProvider KernelStatisticProvider()
			  {
					return Delegate.kernelStatisticProvider();
			  }

			  public override Neo4Net.Kernel.api.KernelTransaction_Revertable RestrictCurrentTransaction( SecurityContext context )
			  {
					return Delegate.restrictCurrentTransaction( context );
			  }

			  public override ResourceTracker ResourceTracker()
			  {
					return Delegate.resourceTracker();
			  }
		 }

		 private class RecordingLocks : Locks
		 {
			  internal readonly Locks Delegate;
			  internal readonly IList<LockOperationListener> Listeners;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IList<LockOperationRecord> LockOperationRecordsConflict;

			  internal RecordingLocks( Locks @delegate, IList<LockOperationListener> listeners, IList<LockOperationRecord> lockOperationRecords )
			  {
					this.Delegate = @delegate;
					this.Listeners = listeners;
					this.LockOperationRecordsConflict = lockOperationRecords;
			  }

			  internal virtual IList<LockOperationRecord> LockOperationRecords
			  {
				  get
				  {
						return LockOperationRecordsConflict;
				  }
			  }

			  internal virtual void Record( bool exclusive, bool acquisition, ResourceTypes type, params long[] ids )
			  {
					if ( acquisition )
					{
						 foreach ( LockOperationListener listener in Listeners )
						 {
							  listener.LockAcquired( exclusive, type, ids );
						 }
					}
					LockOperationRecordsConflict.Add( new LockOperationRecord( exclusive, acquisition, type, ids ) );
			  }

			  public override void AcquireExclusiveNodeLock( params long[] ids )
			  {
					Record( true, true, ResourceTypes.NODE, ids );
					Delegate.acquireExclusiveNodeLock( ids );
			  }

			  public override void AcquireExclusiveRelationshipLock( params long[] ids )
			  {
					Record( true, true, ResourceTypes.RELATIONSHIP, ids );
					Delegate.acquireExclusiveRelationshipLock( ids );
			  }

			  public override void AcquireExclusiveExplicitIndexLock( params long[] ids )
			  {
					Record( true, true, ResourceTypes.EXPLICIT_INDEX, ids );
					Delegate.acquireExclusiveExplicitIndexLock( ids );
			  }

			  public override void AcquireExclusiveLabelLock( params long[] ids )
			  {
					Record( true, true, ResourceTypes.LABEL, ids );
					Delegate.acquireExclusiveLabelLock( ids );
			  }

			  public override void ReleaseExclusiveNodeLock( params long[] ids )
			  {
					Record( true, false, ResourceTypes.NODE, ids );
					Delegate.releaseExclusiveNodeLock( ids );
			  }

			  public override void ReleaseExclusiveRelationshipLock( params long[] ids )
			  {
					Record( true, false, ResourceTypes.RELATIONSHIP, ids );
					Delegate.releaseExclusiveRelationshipLock( ids );
			  }

			  public override void ReleaseExclusiveExplicitIndexLock( params long[] ids )
			  {
					Record( true, false, ResourceTypes.EXPLICIT_INDEX, ids );
					Delegate.releaseExclusiveExplicitIndexLock( ids );
			  }

			  public override void ReleaseExclusiveLabelLock( params long[] ids )
			  {
					Record( true, false, ResourceTypes.LABEL, ids );
					Delegate.releaseExclusiveLabelLock( ids );
			  }

			  public override void AcquireSharedNodeLock( params long[] ids )
			  {
					Record( false, true, ResourceTypes.NODE, ids );
					Delegate.acquireSharedNodeLock( ids );
			  }

			  public override void AcquireSharedRelationshipLock( params long[] ids )
			  {
					Record( false, true, ResourceTypes.RELATIONSHIP, ids );
					Delegate.acquireSharedRelationshipLock( ids );
			  }

			  public override void AcquireSharedExplicitIndexLock( params long[] ids )
			  {
					Record( false, true, ResourceTypes.EXPLICIT_INDEX, ids );
					Delegate.acquireSharedExplicitIndexLock( ids );
			  }

			  public override void AcquireSharedLabelLock( params long[] ids )
			  {
					Record( false, true, ResourceTypes.LABEL, ids );
					Delegate.acquireSharedLabelLock( ids );
			  }

			  public override void ReleaseSharedNodeLock( params long[] ids )
			  {
					Record( false, false, ResourceTypes.NODE, ids );
					Delegate.releaseSharedNodeLock( ids );
			  }

			  public override void ReleaseSharedRelationshipLock( params long[] ids )
			  {
					Record( false, false, ResourceTypes.RELATIONSHIP, ids );
					Delegate.releaseSharedRelationshipLock( ids );
			  }

			  public override void ReleaseSharedExplicitIndexLock( params long[] ids )
			  {
					Record( false, false, ResourceTypes.EXPLICIT_INDEX, ids );
					Delegate.releaseSharedExplicitIndexLock( ids );
			  }

			  public override void ReleaseSharedLabelLock( params long[] ids )
			  {
					Record( false, false, ResourceTypes.LABEL, ids );
					Delegate.releaseSharedLabelLock( ids );
			  }
		 }

		 private class LockOperationListener : EventListener
		 {
			  internal virtual void LockAcquired( bool exclusive, ResourceType resourceType, params long[] ids )
			  {
					// empty operation
			  }
		 }

		 private class LockOperationRecord
		 {
			  internal readonly bool Exclusive;
			  internal readonly bool Acquisition;
			  internal readonly ResourceType ResourceType;
			  internal readonly long[] Ids;

			  internal LockOperationRecord( bool exclusive, bool acquisition, ResourceType resourceType, long[] ids )
			  {
					this.Exclusive = exclusive;
					this.Acquisition = acquisition;
					this.ResourceType = resourceType;
					this.Ids = ids;
			  }

			  public override string ToString()
			  {
					return "LockOperationRecord{" + "exclusive=" + Exclusive + ", acquisition=" + Acquisition +
							  ", resourceType=" + ResourceType + ", ids=" + Arrays.ToString( Ids ) + '}';
			  }
		 }

		 private class OnceSchemaFlushListener : LockOperationListener
		 {
			 private readonly QueryExecutionLocksIT _outerInstance;

			 public OnceSchemaFlushListener( QueryExecutionLocksIT outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal bool Executed;

			  internal override void LockAcquired( bool exclusive, ResourceType resourceType, params long[] ids )
			  {
					if ( !Executed )
					{
						 ThreadToStatementContextBridge bridge = outerInstance.DatabaseRule.resolveDependency( typeof( ThreadToStatementContextBridge ) );
						 KernelTransaction ktx = bridge.GetKernelTransactionBoundToThisThread( true );
						 ktx.SchemaRead().schemaStateFlush();
					}
					Executed = true;
			  }
		 }

		 private class DelegatingTransaction : KernelTransaction
		 {
			  internal readonly KernelTransaction Internal;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Locks LocksConflict;

			  internal DelegatingTransaction( KernelTransaction @internal, Locks locks )
			  {
					this.Internal = @internal;
					this.LocksConflict = locks;
			  }

			  public override void Success()
			  {
					Internal.success();
			  }

			  public override void Failure()
			  {
					Internal.failure();
			  }

			  public override Read DataRead()
			  {
					return Internal.dataRead();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.internal.kernel.api.Write dataWrite() throws org.Neo4Net.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException
			  public override Write DataWrite()
			  {
					return Internal.dataWrite();
			  }

			  public override ExplicitIndexRead IndexRead()
			  {
					return Internal.indexRead();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.internal.kernel.api.ExplicitIndexWrite indexWrite() throws org.Neo4Net.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException
			  public override ExplicitIndexWrite IndexWrite()
			  {
					return Internal.indexWrite();
			  }

			  public override TokenRead TokenRead()
			  {
					return Internal.tokenRead();
			  }

			  public override TokenWrite TokenWrite()
			  {
					return Internal.tokenWrite();
			  }

			  public override Neo4Net.Internal.Kernel.Api.Token Token()
			  {
					return Internal.token();
			  }

			  public override SchemaRead SchemaRead()
			  {
					return Internal.schemaRead();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.internal.kernel.api.SchemaWrite schemaWrite() throws org.Neo4Net.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException
			  public override SchemaWrite SchemaWrite()
			  {
					return Internal.schemaWrite();
			  }

			  public override Locks Locks()
			  {
					return LocksConflict;
			  }

			  public override CursorFactory Cursors()
			  {
					return Internal.cursors();
			  }

			  public override Procedures Procedures()
			  {
					return Internal.procedures();
			  }

			  public override ExecutionStatistics ExecutionStatistics()
			  {
					return Internal.executionStatistics();
			  }

			  public override Statement AcquireStatement()
			  {
					return Internal.acquireStatement();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.storageengine.api.schema.IndexDescriptor indexUniqueCreate(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor schema, String provider) throws org.Neo4Net.internal.kernel.api.exceptions.schema.SchemaKernelException
			  public override IndexDescriptor IndexUniqueCreate( SchemaDescriptor schema, string provider )
			  {
					string defaultProvider = Config.defaults().get(GraphDatabaseSettings.default_schema_provider);
					return Internal.indexUniqueCreate( schema, defaultProvider );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long closeTransaction() throws org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
			  public override long CloseTransaction()
			  {
					return Internal.closeTransaction();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
			  public override void Close()
			  {
					Internal.close();
			  }

			  public virtual bool Open
			  {
				  get
				  {
						return Internal.Open;
				  }
			  }

			  public override SecurityContext SecurityContext()
			  {
					return Internal.securityContext();
			  }

			  public override AuthSubject SubjectOrAnonymous()
			  {
					return Internal.subjectOrAnonymous();
			  }

			  public virtual Optional<Status> ReasonIfTerminated
			  {
				  get
				  {
						return Internal.ReasonIfTerminated;
				  }
			  }

			  public virtual bool Terminated
			  {
				  get
				  {
						return Internal.Terminated;
				  }
			  }

			  public override void MarkForTermination( Status reason )
			  {
					Internal.markForTermination( reason );
			  }

			  public override long LastTransactionTimestampWhenStarted()
			  {
					return Internal.lastTransactionTimestampWhenStarted();
			  }

			  public override long LastTransactionIdWhenStarted()
			  {
					return Internal.lastTransactionIdWhenStarted();
			  }

			  public override long StartTime()
			  {
					return Internal.startTime();
			  }

			  public override long StartTimeNanos()
			  {
					return Internal.startTimeNanos();
			  }

			  public override long Timeout()
			  {
					return Internal.timeout();
			  }

			  public override void RegisterCloseListener( Neo4Net.Kernel.api.KernelTransaction_CloseListener listener )
			  {
					Internal.registerCloseListener( listener );
			  }

			  public override Neo4Net.Internal.Kernel.Api.Transaction_Type TransactionType()
			  {
					return Internal.transactionType();
			  }

			  public virtual long TransactionId
			  {
				  get
				  {
						return Internal.TransactionId;
				  }
			  }

			  public virtual long CommitTime
			  {
				  get
				  {
						return Internal.CommitTime;
				  }
			  }

			  public override Neo4Net.Kernel.api.KernelTransaction_Revertable OverrideWith( SecurityContext context )
			  {
					return Internal.overrideWith( context );
			  }

			  public override ClockContext Clocks()
			  {
					return Internal.clocks();
			  }

			  public override NodeCursor AmbientNodeCursor()
			  {
					return Internal.ambientNodeCursor();
			  }

			  public override RelationshipScanCursor AmbientRelationshipCursor()
			  {
					return Internal.ambientRelationshipCursor();
			  }

			  public override PropertyCursor AmbientPropertyCursor()
			  {
					return Internal.ambientPropertyCursor();
			  }

			  public virtual IDictionary<string, object> MetaData
			  {
				  set
				  {
						Internal.MetaData = value;
				  }
				  get
				  {
						return Internal.MetaData;
				  }
			  }


			  public override void AssertOpen()
			  {
					Internal.assertOpen();
			  }

			  public virtual bool SchemaTransaction
			  {
				  get
				  {
						return Internal.SchemaTransaction;
				  }
			  }
		 }
	}

}