using System;
using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;


	using ConstantTimeTimeoutStrategy = Neo4Net.causalclustering.helper.ConstantTimeTimeoutStrategy;
	using TimeoutStrategy = Neo4Net.causalclustering.helper.TimeoutStrategy;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using CatchUpRequest = Neo4Net.causalclustering.messaging.CatchUpRequest;
	using StoreCopyClientMonitor = Neo4Net.com.storecopy.StoreCopyClientMonitor;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using Level = Neo4Net.Logging.Level;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.atLeast;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class StoreCopyClientTest
	{
		private bool InstanceFieldsInitialized = false;

		public StoreCopyClientTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_catchupAddressProvider = CatchupAddressProvider.fromSingleAddress( _expectedAdvertisedAddress );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException ExpectedException = ExpectedException.none();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.SuppressOutput suppressOutput = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

		 private readonly CatchUpClient _catchUpClient = mock( typeof( CatchUpClient ) );

		 private StoreCopyClient _subject;
		 private readonly LogProvider _logProvider = FormattedLogProvider.withDefaultLogLevel( Level.DEBUG ).toOutputStream( System.out );
		 private readonly Monitors _monitors = new Monitors();

		 // params
		 private readonly AdvertisedSocketAddress _expectedAdvertisedAddress = new AdvertisedSocketAddress( "host", 1234 );
		 private CatchupAddressProvider _catchupAddressProvider;
		 private readonly StoreId _expectedStoreId = new StoreId( 1, 2, 3, 4 );
		 private readonly StoreFileStreamProvider _expectedStoreFileStream = mock( typeof( StoreFileStreamProvider ) );

		 // helpers
		 private File[] _serverFiles = new File[]
		 {
			 new File( "fileA.txt" ),
			 new File( "fileB.bmp" )
		 };
		 private File _targetLocation = new File( "targetLocation" );
		 private LongSet _indexIds = LongSets.immutable.of( 13 );
		 private ConstantTimeTimeoutStrategy _backOffStrategy;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _backOffStrategy = new ConstantTimeTimeoutStrategy( 1, TimeUnit.MILLISECONDS );
			  _subject = new StoreCopyClient( _catchUpClient, _monitors, _logProvider, _backOffStrategy );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clientRequestsAllFilesListedInListingResponse() throws StoreCopyFailedException, org.Neo4Net.causalclustering.catchup.CatchUpClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ClientRequestsAllFilesListedInListingResponse()
		 {
			  // given a bunch of fake files on the server
			  PrepareStoreCopyResponse prepareStoreCopyResponse = PrepareStoreCopyResponse.Success( _serverFiles, _indexIds, -123L );
			  when( _catchUpClient.makeBlockingRequest( any(), any(typeof(PrepareStoreCopyRequest)), any() ) ).thenReturn(prepareStoreCopyResponse);

			  // and any request for a file will be successful
			  StoreCopyFinishedResponse success = new StoreCopyFinishedResponse( StoreCopyFinishedResponse.Status.Success );
			  when( _catchUpClient.makeBlockingRequest( any(), any(typeof(GetStoreFileRequest)), any() ) ).thenReturn(success);

			  // and any request for a file will be successful
			  when( _catchUpClient.makeBlockingRequest( any(), any(typeof(GetIndexFilesRequest)), any() ) ).thenReturn(success);

			  // when client requests catchup
			  _subject.copyStoreFiles( _catchupAddressProvider, _expectedStoreId, _expectedStoreFileStream, ContinueIndefinitely(), _targetLocation );

			  // then there are as many requests to the server for individual requests
			  IList<string> filteredRequests = FilenamesFromIndividualFileRequests( Requests );
			  IList<string> expectedFiles = Stream.of( _serverFiles ).map( File.getName ).collect( Collectors.toList() );
			  assertThat( expectedFiles, containsInAnyOrder( filteredRequests.ToArray() ) );
		 }

		 private System.Func<TerminationCondition> ContinueIndefinitely()
		 {
			  return () => TerminationCondition_Fields.ContinueIndefinitely;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeIdCanBeRetrieved() throws StoreIdDownloadFailedException, org.Neo4Net.causalclustering.catchup.CatchUpClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreIdCanBeRetrieved()
		 {
			  // given remote has expected store ID
			  StoreId remoteStoreId = new StoreId( 6, 3, 2, 6 );

			  // and we know the remote address
			  AdvertisedSocketAddress remoteAddress = new AdvertisedSocketAddress( "host", 1234 );

			  // and server responds with correct data to correct params
			  when( _catchUpClient.makeBlockingRequest( eq( remoteAddress ), any( typeof( GetStoreIdRequest ) ), any() ) ).thenReturn(remoteStoreId);

			  // when client requests the remote store id
			  StoreId actualStoreId = _subject.fetchStoreId( remoteAddress );

			  // then store id matches
			  assertEquals( remoteStoreId, actualStoreId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAwaitOnSuccess() throws org.Neo4Net.causalclustering.catchup.CatchUpClientException, StoreCopyFailedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAwaitOnSuccess()
		 {
			  // given
			  Neo4Net.causalclustering.helper.TimeoutStrategy_Timeout mockedTimeout = mock( typeof( Neo4Net.causalclustering.helper.TimeoutStrategy_Timeout ) );
			  TimeoutStrategy backoffStrategy = mock( typeof( TimeoutStrategy ) );
			  when( backoffStrategy.NewTimeout() ).thenReturn(mockedTimeout);

			  // and
			  _subject = new StoreCopyClient( _catchUpClient, _monitors, _logProvider, backoffStrategy );

			  // and
			  PrepareStoreCopyResponse prepareStoreCopyResponse = PrepareStoreCopyResponse.Success( _serverFiles, _indexIds, -123L );
			  when( _catchUpClient.makeBlockingRequest( any(), any(typeof(PrepareStoreCopyRequest)), any() ) ).thenReturn(prepareStoreCopyResponse);

			  // and
			  StoreCopyFinishedResponse success = new StoreCopyFinishedResponse( StoreCopyFinishedResponse.Status.Success );
			  when( _catchUpClient.makeBlockingRequest( any(), any(typeof(GetStoreFileRequest)), any() ) ).thenReturn(success);

			  // and
			  when( _catchUpClient.makeBlockingRequest( any(), any(typeof(GetIndexFilesRequest)), any() ) ).thenReturn(success);

			  // when
			  _subject.copyStoreFiles( _catchupAddressProvider, _expectedStoreId, _expectedStoreFileStream, ContinueIndefinitely(), _targetLocation );

			  // then
			  verify( mockedTimeout, never() ).increment();
			  verify( mockedTimeout, never() ).Millis;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfTerminationConditionFails() throws org.Neo4Net.causalclustering.catchup.CatchUpClientException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfTerminationConditionFails()
		 {
			  // given a file will fail an expected number of times
			  _subject = new StoreCopyClient( _catchUpClient, _monitors, _logProvider, _backOffStrategy );

			  // and requesting the individual file will fail
			  when( _catchUpClient.makeBlockingRequest( any(), any(), any() ) ).thenReturn(new StoreCopyFinishedResponse(StoreCopyFinishedResponse.Status.ETooFarBehind));

			  // and the initial list+count store files request is successful
			  PrepareStoreCopyResponse initialListingOfFilesResponse = PrepareStoreCopyResponse.Success( _serverFiles, _indexIds, -123L );
			  when( _catchUpClient.makeBlockingRequest( any(), any(typeof(PrepareStoreCopyRequest)), any() ) ).thenReturn(initialListingOfFilesResponse);

			  // when we perform catchup
			  try
			  {
					_subject.copyStoreFiles(_catchupAddressProvider, _expectedStoreId, _expectedStoreFileStream, () => () =>
					{
					 throw new StoreCopyFailedException( "This can't go on" );
					}, _targetLocation);
					fail( "Expected exception: " + typeof( StoreCopyFailedException ) );
			  }
			  catch ( StoreCopyFailedException expectedException )
			  {
					assertEquals( "This can't go on", expectedException.Message );
					return;
			  }

			  fail( "Expected a StoreCopyFailedException" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void errorOnListingStore() throws org.Neo4Net.causalclustering.catchup.CatchUpClientException, StoreCopyFailedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ErrorOnListingStore()
		 {
			  // given store listing fails
			  PrepareStoreCopyResponse prepareStoreCopyResponse = PrepareStoreCopyResponse.Error( PrepareStoreCopyResponse.Status.EListingStore );
			  when( _catchUpClient.makeBlockingRequest( any(), any(), any() ) ).thenReturn(prepareStoreCopyResponse).thenThrow(new Exception("Should not be accessible"));

			  // then
			  ExpectedException.expectMessage( "Preparing store failed due to: E_LISTING_STORE" );
			  ExpectedException.expect( typeof( StoreCopyFailedException ) );

			  // when
			  _subject.copyStoreFiles( _catchupAddressProvider, _expectedStoreId, _expectedStoreFileStream, ContinueIndefinitely(), _targetLocation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeIdMismatchOnListing() throws org.Neo4Net.causalclustering.catchup.CatchUpClientException, StoreCopyFailedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreIdMismatchOnListing()
		 {
			  // given store listing fails
			  PrepareStoreCopyResponse prepareStoreCopyResponse = PrepareStoreCopyResponse.Error( PrepareStoreCopyResponse.Status.EStoreIdMismatch );
			  when( _catchUpClient.makeBlockingRequest( any(), any(), any() ) ).thenReturn(prepareStoreCopyResponse).thenThrow(new Exception("Should not be accessible"));

			  // then
			  ExpectedException.expectMessage( "Preparing store failed due to: E_STORE_ID_MISMATCH" );
			  ExpectedException.expect( typeof( StoreCopyFailedException ) );

			  // when
			  _subject.copyStoreFiles( _catchupAddressProvider, _expectedStoreId, _expectedStoreFileStream, ContinueIndefinitely(), _targetLocation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeFileEventsAreReported() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreFileEventsAreReported()
		 {
			  // given
			  PrepareStoreCopyResponse prepareStoreCopyResponse = PrepareStoreCopyResponse.Success( _serverFiles, _indexIds, -123L );
			  when( _catchUpClient.makeBlockingRequest( any(), any(typeof(PrepareStoreCopyRequest)), any() ) ).thenReturn(prepareStoreCopyResponse);

			  // and
			  StoreCopyFinishedResponse success = new StoreCopyFinishedResponse( StoreCopyFinishedResponse.Status.Success );
			  when( _catchUpClient.makeBlockingRequest( any(), any(typeof(GetStoreFileRequest)), any() ) ).thenReturn(success);

			  // and
			  when( _catchUpClient.makeBlockingRequest( any(), any(typeof(GetIndexFilesRequest)), any() ) ).thenReturn(success);

			  // and
			  StoreCopyClientMonitor storeCopyClientMonitor = mock( typeof( StoreCopyClientMonitor ) );
			  _monitors.addMonitorListener( storeCopyClientMonitor );

			  // when
			  _subject.copyStoreFiles( _catchupAddressProvider, _expectedStoreId, _expectedStoreFileStream, ContinueIndefinitely(), _targetLocation );

			  // then
			  verify( storeCopyClientMonitor ).startReceivingStoreFiles();
			  foreach ( File storeFileRequested in _serverFiles )
			  {
					verify( storeCopyClientMonitor ).startReceivingStoreFile( Paths.get( _targetLocation.ToString(), storeFileRequested.ToString() ).ToString() );
					verify( storeCopyClientMonitor ).finishReceivingStoreFile( Paths.get( _targetLocation.ToString(), storeFileRequested.ToString() ).ToString() );
			  }
			  verify( storeCopyClientMonitor ).finishReceivingStoreFiles();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void snapshotEventsAreReported() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SnapshotEventsAreReported()
		 {
			  // given
			  PrepareStoreCopyResponse prepareStoreCopyResponse = PrepareStoreCopyResponse.Success( _serverFiles, _indexIds, -123L );
			  when( _catchUpClient.makeBlockingRequest( any(), any(typeof(PrepareStoreCopyRequest)), any() ) ).thenReturn(prepareStoreCopyResponse);

			  // and
			  StoreCopyFinishedResponse success = new StoreCopyFinishedResponse( StoreCopyFinishedResponse.Status.Success );
			  when( _catchUpClient.makeBlockingRequest( any(), any(typeof(GetStoreFileRequest)), any() ) ).thenReturn(success);

			  // and
			  when( _catchUpClient.makeBlockingRequest( any(), any(typeof(GetIndexFilesRequest)), any() ) ).thenReturn(success);

			  // and
			  StoreCopyClientMonitor storeCopyClientMonitor = mock( typeof( StoreCopyClientMonitor ) );
			  _monitors.addMonitorListener( storeCopyClientMonitor );

			  // when
			  _subject.copyStoreFiles( _catchupAddressProvider, _expectedStoreId, _expectedStoreFileStream, ContinueIndefinitely(), _targetLocation );

			  // then
			  verify( storeCopyClientMonitor ).startReceivingIndexSnapshots();
			  LongIterator iterator = _indexIds.longIterator();
			  while ( iterator.hasNext() )
			  {
					long indexSnapshotIdRequested = iterator.next();
					verify( storeCopyClientMonitor ).startReceivingIndexSnapshot( indexSnapshotIdRequested );
					verify( storeCopyClientMonitor ).finishReceivingIndexSnapshot( indexSnapshotIdRequested );
			  }
			  verify( storeCopyClientMonitor ).finishReceivingIndexSnapshots();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<org.Neo4Net.causalclustering.messaging.CatchUpRequest> getRequests() throws org.Neo4Net.causalclustering.catchup.CatchUpClientException
		 private IList<CatchUpRequest> Requests
		 {
			 get
			 {
				  ArgumentCaptor<CatchUpRequest> fileRequestArgumentCaptor = ArgumentCaptor.forClass( typeof( CatchUpRequest ) );
				  verify( _catchUpClient, atLeast( 0 ) ).makeBlockingRequest( any(), fileRequestArgumentCaptor.capture(), any() );
				  return fileRequestArgumentCaptor.AllValues;
			 }
		 }

		 private IList<string> FilenamesFromIndividualFileRequests( IList<CatchUpRequest> fileRequests )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return fileRequests.Where( typeof( GetStoreFileRequest ).isInstance ).Select( obj => ( GetStoreFileRequest ) obj ).Select( GetStoreFileRequest::file ).Select( File.getName ).ToList();
		 }
	}


}