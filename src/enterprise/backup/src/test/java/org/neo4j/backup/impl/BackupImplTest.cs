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
namespace Neo4Net.backup.impl
{
	using Test = org.junit.Test;

	using RequestContext = Neo4Net.com.RequestContext;
	using StoreCopyServer = Neo4Net.com.storecopy.StoreCopyServer;
	using StoreWriter = Neo4Net.com.storecopy.StoreWriter;
	using LogFileInformation = Neo4Net.Kernel.impl.transaction.log.LogFileInformation;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class BackupImplTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flushStoreFilesWithCorrectCheckpointTriggerName()
		 public virtual void FlushStoreFilesWithCorrectCheckpointTriggerName()
		 {
			  StoreCopyServer storeCopyServer = mock( typeof( StoreCopyServer ) );
			  when( storeCopyServer.FlushStoresAndStreamStoreFiles( anyString(), any(typeof(StoreWriter)), anyBoolean() ) ).thenReturn(RequestContext.EMPTY);

			  BackupImpl backup = new BackupImpl( storeCopyServer, mock( typeof( LogicalTransactionStore ) ), mock( typeof( TransactionIdStore ) ), mock( typeof( LogFileInformation ) ), DefaultStoreIdSupplier(), NullLogProvider.Instance );

			  backup.FullBackup( mock( typeof( StoreWriter ) ), false ).close();

			  verify( storeCopyServer ).flushStoresAndStreamStoreFiles( eq( BackupImpl.FULL_BACKUP_CHECKPOINT_TRIGGER ), any( typeof( StoreWriter ) ), eq( false ) );
		 }

		 private static System.Func<StoreId> DefaultStoreIdSupplier()
		 {
			  return () => StoreId.DEFAULT;
		 }
	}

}