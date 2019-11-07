﻿/*
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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using Test = org.junit.Test;


	using TransactionLogCatchUpFactory = Neo4Net.causalclustering.catchup.tx.TransactionLogCatchUpFactory;
	using TransactionLogCatchUpWriter = Neo4Net.causalclustering.catchup.tx.TransactionLogCatchUpWriter;
	using TxPullClient = Neo4Net.causalclustering.catchup.tx.TxPullClient;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.catchup.CatchupResult.SUCCESS_END_OF_STREAM;

	public class RemoteStoreTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCopyStoreFilesAndPullTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCopyStoreFilesAndPullTransactions()
		 {
			  // given
			  StoreId storeId = new StoreId( 1, 2, 3, 4 );
			  StoreCopyClient storeCopyClient = mock( typeof( StoreCopyClient ) );
			  TxPullClient txPullClient = mock( typeof( TxPullClient ) );
			  when( txPullClient.PullTransactions( any(), any(), anyLong(), any() ) ).thenReturn(new TxPullRequestResult(SUCCESS_END_OF_STREAM, 13));
			  TransactionLogCatchUpWriter writer = mock( typeof( TransactionLogCatchUpWriter ) );

			  RemoteStore remoteStore = new RemoteStore( NullLogProvider.Instance, mock( typeof( FileSystemAbstraction ) ), null, storeCopyClient, txPullClient, Factory( writer ), Config.defaults(), new Monitors() );

			  // when
			  AdvertisedSocketAddress localhost = new AdvertisedSocketAddress( "127.0.0.1", 1234 );
			  CatchupAddressProvider catchupAddressProvider = CatchupAddressProvider.fromSingleAddress( localhost );
			  remoteStore.Copy( catchupAddressProvider, storeId, DatabaseLayout.of( new File( "destination" ) ), true );

			  // then
			  verify( storeCopyClient ).copyStoreFiles( eq( catchupAddressProvider ), eq( storeId ), any( typeof( StoreFileStreamProvider ) ), any(), any() );
			  verify( txPullClient ).pullTransactions( eq( localhost ), eq( storeId ), anyLong(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetLastPulledTransactionId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetLastPulledTransactionId()
		 {
			  // given
			  long lastFlushedTxId = 12;
			  StoreId wantedStoreId = new StoreId( 1, 2, 3, 4 );
			  AdvertisedSocketAddress localhost = new AdvertisedSocketAddress( "127.0.0.1", 1234 );
			  CatchupAddressProvider catchupAddressProvider = CatchupAddressProvider.fromSingleAddress( localhost );

			  StoreCopyClient storeCopyClient = mock( typeof( StoreCopyClient ) );
			  when( storeCopyClient.CopyStoreFiles( eq( catchupAddressProvider ), eq( wantedStoreId ), any( typeof( StoreFileStreamProvider ) ), any(), any() ) ).thenReturn(lastFlushedTxId);

			  TxPullClient txPullClient = mock( typeof( TxPullClient ) );
			  when( txPullClient.PullTransactions( eq( localhost ), eq( wantedStoreId ), anyLong(), any() ) ).thenReturn(new TxPullRequestResult(SUCCESS_END_OF_STREAM, 13));

			  TransactionLogCatchUpWriter writer = mock( typeof( TransactionLogCatchUpWriter ) );

			  RemoteStore remoteStore = new RemoteStore( NullLogProvider.Instance, mock( typeof( FileSystemAbstraction ) ), null, storeCopyClient, txPullClient, Factory( writer ), Config.defaults(), new Monitors() );

			  // when
			  remoteStore.Copy( catchupAddressProvider, wantedStoreId, DatabaseLayout.of( new File( "destination" ) ), true );

			  // then
			  long previousTxId = lastFlushedTxId - 1; // the interface is defined as asking for the one preceding
			  verify( txPullClient ).pullTransactions( eq( localhost ), eq( wantedStoreId ), eq( previousTxId ), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseDownTxLogWriterIfTxStreamingFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseDownTxLogWriterIfTxStreamingFails()
		 {
			  // given
			  StoreId storeId = new StoreId( 1, 2, 3, 4 );
			  StoreCopyClient storeCopyClient = mock( typeof( StoreCopyClient ) );
			  TxPullClient txPullClient = mock( typeof( TxPullClient ) );
			  TransactionLogCatchUpWriter writer = mock( typeof( TransactionLogCatchUpWriter ) );
			  CatchupAddressProvider catchupAddressProvider = CatchupAddressProvider.fromSingleAddress( null );

			  RemoteStore remoteStore = new RemoteStore( NullLogProvider.Instance, mock( typeof( FileSystemAbstraction ) ), null, storeCopyClient, txPullClient, Factory( writer ), Config.defaults(), new Monitors() );

			  doThrow( typeof( CatchUpClientException ) ).when( txPullClient ).pullTransactions( Null, eq( storeId ), anyLong(), any() );

			  // when
			  try
			  {
					remoteStore.Copy( catchupAddressProvider, storeId, DatabaseLayout.of( new File( "." ) ), true );
			  }
			  catch ( StoreCopyFailedException )
			  {
					// expected
			  }

			  // then
			  verify( writer ).close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static Neo4Net.causalclustering.catchup.tx.TransactionLogCatchUpFactory factory(Neo4Net.causalclustering.catchup.tx.TransactionLogCatchUpWriter writer) throws java.io.IOException
		 private static TransactionLogCatchUpFactory Factory( TransactionLogCatchUpWriter writer )
		 {
			  TransactionLogCatchUpFactory factory = mock( typeof( TransactionLogCatchUpFactory ) );
			  when( factory.Create( any(), any(typeof(FileSystemAbstraction)), Null, any(typeof(Config)), any(typeof(LogProvider)), anyLong(), anyBoolean(), anyBoolean(), anyBoolean() ) ).thenReturn(writer);
			  return factory;
		 }
	}

}