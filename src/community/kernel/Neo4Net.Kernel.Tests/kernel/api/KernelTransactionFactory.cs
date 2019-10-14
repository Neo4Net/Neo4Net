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
namespace Neo4Net.Kernel.api
{

	using Neo4Net.Collections.Pooling;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using AutoIndexing = Neo4Net.Kernel.api.explicitindex.AutoIndexing;
	using AuxiliaryTransactionStateManager = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateManager;
	using Config = Neo4Net.Kernel.configuration.Config;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using SchemaState = Neo4Net.Kernel.Impl.Api.SchemaState;
	using SchemaWriteGuard = Neo4Net.Kernel.Impl.Api.SchemaWriteGuard;
	using StatementOperationParts = Neo4Net.Kernel.Impl.Api.StatementOperationParts;
	using TransactionHeaderInformation = Neo4Net.Kernel.Impl.Api.TransactionHeaderInformation;
	using TransactionHooks = Neo4Net.Kernel.Impl.Api.TransactionHooks;
	using TransactionRepresentationCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ConstraintIndexCreator = Neo4Net.Kernel.Impl.Api.state.ConstraintIndexCreator;
	using StandardConstraintSemantics = Neo4Net.Kernel.impl.constraints.StandardConstraintSemantics;
	using CanWrite = Neo4Net.Kernel.impl.factory.CanWrite;
	using ExplicitIndexStore = Neo4Net.Kernel.impl.index.ExplicitIndexStore;
	using NoOpClient = Neo4Net.Kernel.impl.locking.NoOpClient;
	using SimpleStatementLocks = Neo4Net.Kernel.impl.locking.SimpleStatementLocks;
	using StatementLocks = Neo4Net.Kernel.impl.locking.StatementLocks;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using TransactionHeaderInformationFactory = Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionMonitor = Neo4Net.Kernel.impl.transaction.TransactionMonitor;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using CpuClock = Neo4Net.Resources.CpuClock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.tracing.TransactionTracer_Fields.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.collection.CollectionsFactorySupplier_Fields.ON_HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.MockedNeoStores.mockedTokenHolders;

	public class KernelTransactionFactory
	{
		 public class Instances
		 {
			  public KernelTransactionImplementation Transaction;

			  public Instances( KernelTransactionImplementation transaction )
			  {
					this.Transaction = transaction;
			  }
		 }

		 private KernelTransactionFactory()
		 {
		 }

		 private static Instances KernelTransactionWithInternals( LoginContext loginContext )
		 {
			  TransactionHeaderInformation headerInformation = new TransactionHeaderInformation( -1, -1, new sbyte[0] );
			  TransactionHeaderInformationFactory headerInformationFactory = mock( typeof( TransactionHeaderInformationFactory ) );
			  when( headerInformationFactory.Create() ).thenReturn(headerInformation);

			  StorageEngine storageEngine = mock( typeof( StorageEngine ) );
			  StorageReader storageReader = mock( typeof( StorageReader ) );
			  when( storageEngine.NewReader() ).thenReturn(storageReader);

			  KernelTransactionImplementation transaction = new KernelTransactionImplementation( Config.defaults(), mock(typeof(StatementOperationParts)), mock(typeof(SchemaWriteGuard)), new TransactionHooks(), mock(typeof(ConstraintIndexCreator)), new Procedures(), headerInformationFactory, mock(typeof(TransactionRepresentationCommitProcess)), mock(typeof(TransactionMonitor)), mock(typeof(AuxiliaryTransactionStateManager)), mock(typeof(Pool)), Clocks.nanoClock(), new AtomicReference<CpuClock>(CpuClock.NOT_AVAILABLE), new AtomicReference<HeapAllocation>(HeapAllocation.NOT_AVAILABLE), NULL, LockTracer.NONE, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, storageEngine, new CanWrite(), AutoIndexing.UNSUPPORTED, mock(typeof(ExplicitIndexStore)), EmptyVersionContextSupplier.EMPTY, ON_HEAP, new StandardConstraintSemantics(), mock(typeof(SchemaState)), mock(typeof(IndexingService)), mockedTokenHolders(), new Dependencies() );

			  StatementLocks statementLocks = new SimpleStatementLocks( new NoOpClient() );

			  transaction.Initialize( 0, 0, statementLocks, KernelTransaction.Type.@implicit, loginContext.Authorize( s => -1, GraphDatabaseSettings.DEFAULT_DATABASE_NAME ), 0L, 1L );

			  return new Instances( transaction );
		 }

		 internal static KernelTransaction KernelTransaction( LoginContext loginContext )
		 {
			  return KernelTransactionWithInternals( loginContext ).Transaction;
		 }
	}

}