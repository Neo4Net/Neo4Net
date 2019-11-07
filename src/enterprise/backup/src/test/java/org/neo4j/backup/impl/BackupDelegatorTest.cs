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
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;


	using CatchUpClient = Neo4Net.causalclustering.catchup.CatchUpClient;
	using CatchupAddressProvider = Neo4Net.causalclustering.catchup.CatchupAddressProvider;
	using CatchupAddressResolutionException = Neo4Net.causalclustering.catchup.CatchupAddressResolutionException;
	using RemoteStore = Neo4Net.causalclustering.catchup.storecopy.RemoteStore;
	using StoreCopyClient = Neo4Net.causalclustering.catchup.storecopy.StoreCopyClient;
	using StoreCopyFailedException = Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException;
	using StoreIdDownloadFailedException = Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class BackupDelegatorTest
	{
		 private RemoteStore _remoteStore;
		 private CatchUpClient _catchUpClient;
		 private StoreCopyClient _storeCopyClient;

		 internal BackupDelegator Subject;

		 private readonly AdvertisedSocketAddress _anyAddress = new AdvertisedSocketAddress( "any.address", 1234 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _remoteStore = mock( typeof( RemoteStore ) );
			  _catchUpClient = mock( typeof( CatchUpClient ) );
			  _storeCopyClient = mock( typeof( StoreCopyClient ) );
			  Subject = new BackupDelegator( _remoteStore, _catchUpClient, _storeCopyClient );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryCatchingUpDelegatesToRemoteStore() throws Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryCatchingUpDelegatesToRemoteStore()
		 {
			  // given
			  AdvertisedSocketAddress fromAddress = new AdvertisedSocketAddress( "Neo4Net.com", 5432 );
			  StoreId expectedStoreId = new StoreId( 7, 2, 5, 98 );
			  DatabaseLayout databaseLayout = DatabaseLayout.of( new File( "." ) );

			  // when
			  Subject.tryCatchingUp( fromAddress, expectedStoreId, databaseLayout );

			  // then
			  verify( _remoteStore ).tryCatchingUp( fromAddress, expectedStoreId, databaseLayout, true, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startDelegatesToCatchUpClient()
		 public virtual void StartDelegatesToCatchUpClient()
		 {
			  // when
			  Subject.start();

			  // then
			  verify( _catchUpClient ).start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopDelegatesToCatchUpClient()
		 public virtual void StopDelegatesToCatchUpClient()
		 {
			  // when
			  Subject.stop();

			  // then
			  verify( _catchUpClient ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fetchStoreIdDelegatesToStoreCopyClient() throws Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FetchStoreIdDelegatesToStoreCopyClient()
		 {
			  // given
			  AdvertisedSocketAddress fromAddress = new AdvertisedSocketAddress( "neo4.com", 935 );

			  // and
			  StoreId expectedStoreId = new StoreId( 6, 2, 9, 3 );
			  when( _storeCopyClient.fetchStoreId( fromAddress ) ).thenReturn( expectedStoreId );

			  // when
			  StoreId storeId = Subject.fetchStoreId( fromAddress );

			  // then
			  assertEquals( expectedStoreId, storeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void retrieveStoreDelegatesToStoreCopyService() throws Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException, Neo4Net.causalclustering.catchup.CatchupAddressResolutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RetrieveStoreDelegatesToStoreCopyService()
		 {
			  // given
			  StoreId storeId = new StoreId( 92, 5, 7, 32 );
			  DatabaseLayout databaseLayout = DatabaseLayout.of( new File( "." ) );

			  // when
			  Subject.copy( _anyAddress, storeId, databaseLayout );

			  // then
			  ArgumentCaptor<CatchupAddressProvider> argumentCaptor = ArgumentCaptor.forClass( typeof( CatchupAddressProvider ) );
			  verify( _remoteStore ).copy( argumentCaptor.capture(), eq(storeId), eq(databaseLayout), eq(true) );

			  //and
			  assertEquals( _anyAddress, argumentCaptor.Value.primary() );
		 }
	}

}