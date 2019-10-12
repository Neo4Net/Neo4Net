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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{
	using Test = org.junit.Test;


	using TransactionLogCatchUpFactory = Org.Neo4j.causalclustering.catchup.tx.TransactionLogCatchUpFactory;
	using TransactionLogCatchUpWriter = Org.Neo4j.causalclustering.catchup.tx.TransactionLogCatchUpWriter;
	using TxPullClient = Org.Neo4j.causalclustering.catchup.tx.TxPullClient;
	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

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
//	import static org.neo4j.causalclustering.catchup.CatchupResult.SUCCESS_END_OF_STREAM;

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
//ORIGINAL LINE: private static org.neo4j.causalclustering.catchup.tx.TransactionLogCatchUpFactory factory(org.neo4j.causalclustering.catchup.tx.TransactionLogCatchUpWriter writer) throws java.io.IOException
		 private static TransactionLogCatchUpFactory Factory( TransactionLogCatchUpWriter writer )
		 {
			  TransactionLogCatchUpFactory factory = mock( typeof( TransactionLogCatchUpFactory ) );
			  when( factory.Create( any(), any(typeof(FileSystemAbstraction)), Null, any(typeof(Config)), any(typeof(LogProvider)), anyLong(), anyBoolean(), anyBoolean(), anyBoolean() ) ).thenReturn(writer);
			  return factory;
		 }
	}

}