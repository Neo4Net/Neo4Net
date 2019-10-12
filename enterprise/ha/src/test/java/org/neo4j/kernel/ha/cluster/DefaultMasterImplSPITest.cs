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
namespace Org.Neo4j.Kernel.ha.cluster
{
	using Test = org.junit.Test;

	using StoreWriter = Org.Neo4j.com.storecopy.StoreWriter;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using LogicalTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using StoreCopyCheckPointMutex = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;
	using TriggerInfo = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.TriggerInfo;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

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