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
namespace Neo4Net.Kernel.ha.cluster
{
	using Test = org.junit.Test;

	using StoreWriter = Neo4Net.com.storecopy.StoreWriter;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using StoreCopyCheckPointMutex = Neo4Net.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;
	using TriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.TriggerInfo;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.MockedNeoStores.mockedTokenHolders;

	public class DefaultMasterImplSPITest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flushStoreFilesWithCorrectCheckpointTriggerName() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FlushStoreFilesWithCorrectCheckpointTriggerName()
		 {
			  CheckPointer checkPointer = mock( typeof( CheckPointer ) );
			  StoreCopyCheckPointMutex mutex = new StoreCopyCheckPointMutex();

			  NeoStoreDataSource dataSource = mock( typeof( NeoStoreDataSource ) );
			  when( dataSource.StoreCopyCheckPointMutex ).thenReturn( mutex );
			  when( dataSource.ListStoreFiles( anyBoolean() ) ).thenReturn(Iterators.emptyResourceIterator());

			  DefaultMasterImplSPI master = new DefaultMasterImplSPI( mock( typeof( GraphDatabaseAPI ), RETURNS_MOCKS ), mock( typeof( FileSystemAbstraction ) ), new Monitors(), mockedTokenHolders(), mock(typeof(IdGeneratorFactory)), mock(typeof(TransactionCommitProcess)), checkPointer, mock(typeof(TransactionIdStore)), mock(typeof(LogicalTransactionStore)), dataSource, NullLogProvider.Instance );

			  master.FlushStoresAndStreamStoreFiles( mock( typeof( StoreWriter ) ) );

			  TriggerInfo expectedTriggerInfo = new SimpleTriggerInfo( DefaultMasterImplSPI.STORE_COPY_CHECKPOINT_TRIGGER );
			  verify( checkPointer ).tryCheckPoint( expectedTriggerInfo );
		 }
	}

}