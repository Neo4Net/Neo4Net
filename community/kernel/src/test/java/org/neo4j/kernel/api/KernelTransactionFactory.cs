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
namespace Org.Neo4j.Kernel.api
{

	using Org.Neo4j.Collection.pool;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using AutoIndexing = Org.Neo4j.Kernel.api.explicitindex.AutoIndexing;
	using AuxiliaryTransactionStateManager = Org.Neo4j.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateManager;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using KernelTransactionImplementation = Org.Neo4j.Kernel.Impl.Api.KernelTransactionImplementation;
	using SchemaState = Org.Neo4j.Kernel.Impl.Api.SchemaState;
	using SchemaWriteGuard = Org.Neo4j.Kernel.Impl.Api.SchemaWriteGuard;
	using StatementOperationParts = Org.Neo4j.Kernel.Impl.Api.StatementOperationParts;
	using TransactionHeaderInformation = Org.Neo4j.Kernel.Impl.Api.TransactionHeaderInformation;
	using TransactionHooks = Org.Neo4j.Kernel.Impl.Api.TransactionHooks;
	using TransactionRepresentationCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using ConstraintIndexCreator = Org.Neo4j.Kernel.Impl.Api.state.ConstraintIndexCreator;
	using StandardConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.StandardConstraintSemantics;
	using CanWrite = Org.Neo4j.Kernel.impl.factory.CanWrite;
	using ExplicitIndexStore = Org.Neo4j.Kernel.impl.index.ExplicitIndexStore;
	using NoOpClient = Org.Neo4j.Kernel.impl.locking.NoOpClient;
	using SimpleStatementLocks = Org.Neo4j.Kernel.impl.locking.SimpleStatementLocks;
	using StatementLocks = Org.Neo4j.Kernel.impl.locking.StatementLocks;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using TransactionHeaderInformationFactory = Org.Neo4j.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionMonitor = Org.Neo4j.Kernel.impl.transaction.TransactionMonitor;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using CpuClock = Org.Neo4j.Resources.CpuClock;
	using HeapAllocation = Org.Neo4j.Resources.HeapAllocation;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;
	using Clocks = Org.Neo4j.Time.Clocks;

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

			  KernelTransactionImplementation transaction = new KernelTransactionImplementation( Config.defaults(), mock(typeof(StatementOperationParts)), mock(typeof(SchemaWriteGuard)), new TransactionHooks(), mock(typeof(ConstraintIndexCreator)), new Procedures(), headerInformationFactory, mock(typeof(TransactionRepresentationCommitProcess)), mock(typeof(TransactionMonitor)), mock(typeof(AuxiliaryTransactionStateManager)), mock(typeof(Pool)), Clocks.nanoClock(), new AtomicReference<CpuClock>(CpuClock.NOT_AVAILABLE), new AtomicReference<HeapAllocation>(HeapAllocation.NOT_AVAILABLE), NULL, LockTracer.NONE, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, storageEngine, new CanWrite(), AutoIndexing.UNSUPPORTED, mock(typeof(ExplicitIndexStore)), EmptyVersionContextSupplier.EMPTY, ON_HEAP, new StandardConstraintSemantics(), mock(typeof(SchemaState)), mock(typeof(IndexingService)), mockedTokenHolders(), new Dependencies() );

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