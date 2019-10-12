using System;
using System.Threading;

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
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;


	using Org.Neo4j.causalclustering.catchup;
	using TimeoutStrategy = Org.Neo4j.causalclustering.helper.TimeoutStrategy;
	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using CatchUpRequest = Org.Neo4j.causalclustering.messaging.CatchUpRequest;
	using StoreCopyClientMonitor = Org.Neo4j.com.storecopy.StoreCopyClientMonitor;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.StoreCopyResponseAdaptors.filesCopyAdaptor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.StoreCopyResponseAdaptors.prepareStoreCopyAdaptor;

	public class StoreCopyClient
	{
		 private readonly CatchUpClient _catchUpClient;
		 private readonly Log _log;
		 private TimeoutStrategy _backOffStrategy;
		 private readonly Monitors _monitors;

		 public StoreCopyClient( CatchUpClient catchUpClient, Monitors monitors, LogProvider logProvider, TimeoutStrategy backOffStrategy )
		 {
			  this._catchUpClient = catchUpClient;
			  this._monitors = monitors;
			  _log = logProvider.getLog( this.GetType() );
			  this._backOffStrategy = backOffStrategy;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long copyStoreFiles(org.neo4j.causalclustering.catchup.CatchupAddressProvider catchupAddressProvider, org.neo4j.causalclustering.identity.StoreId expectedStoreId, StoreFileStreamProvider storeFileStreamProvider, System.Func<TerminationCondition> requestWiseTerminationCondition, java.io.File destDir) throws StoreCopyFailedException
		 internal virtual long CopyStoreFiles( CatchupAddressProvider catchupAddressProvider, StoreId expectedStoreId, StoreFileStreamProvider storeFileStreamProvider, System.Func<TerminationCondition> requestWiseTerminationCondition, File destDir )
		 {
			  try
			  {
					PrepareStoreCopyResponse prepareStoreCopyResponse = PrepareStoreCopy( catchupAddressProvider.Primary(), expectedStoreId, storeFileStreamProvider );
					CopyFilesIndividually( prepareStoreCopyResponse, expectedStoreId, catchupAddressProvider, storeFileStreamProvider, requestWiseTerminationCondition, destDir );
					CopyIndexSnapshotIndividually( prepareStoreCopyResponse, expectedStoreId, catchupAddressProvider, storeFileStreamProvider, requestWiseTerminationCondition );
					return prepareStoreCopyResponse.LastTransactionId();
			  }
			  catch ( Exception e ) when ( e is CatchupAddressResolutionException || e is CatchUpClientException )
			  {
					throw new StoreCopyFailedException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void copyFilesIndividually(PrepareStoreCopyResponse prepareStoreCopyResponse, org.neo4j.causalclustering.identity.StoreId expectedStoreId, org.neo4j.causalclustering.catchup.CatchupAddressProvider addressProvider, StoreFileStreamProvider storeFileStream, System.Func<TerminationCondition> terminationConditions, java.io.File destDir) throws StoreCopyFailedException
		 private void CopyFilesIndividually( PrepareStoreCopyResponse prepareStoreCopyResponse, StoreId expectedStoreId, CatchupAddressProvider addressProvider, StoreFileStreamProvider storeFileStream, System.Func<TerminationCondition> terminationConditions, File destDir )
		 {
			  StoreCopyClientMonitor storeCopyClientMonitor = _monitors.newMonitor( typeof( StoreCopyClientMonitor ) );
			  storeCopyClientMonitor.StartReceivingStoreFiles();
			  long lastTransactionId = prepareStoreCopyResponse.LastTransactionId();
			  foreach ( File file in prepareStoreCopyResponse.Files )
			  {
					storeCopyClientMonitor.StartReceivingStoreFile( Paths.get( destDir.ToString(), file.Name ).ToString() );
					PersistentCallToSecondary( new GetStoreFileRequest( expectedStoreId, file, lastTransactionId ), filesCopyAdaptor( storeFileStream, _log ), addressProvider, terminationConditions() );
					storeCopyClientMonitor.FinishReceivingStoreFile( Paths.get( destDir.ToString(), file.Name ).ToString() );
			  }
			  storeCopyClientMonitor.FinishReceivingStoreFiles();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void copyIndexSnapshotIndividually(PrepareStoreCopyResponse prepareStoreCopyResponse, org.neo4j.causalclustering.identity.StoreId expectedStoreId, org.neo4j.causalclustering.catchup.CatchupAddressProvider addressProvider, StoreFileStreamProvider storeFileStream, System.Func<TerminationCondition> terminationConditions) throws StoreCopyFailedException
		 private void CopyIndexSnapshotIndividually( PrepareStoreCopyResponse prepareStoreCopyResponse, StoreId expectedStoreId, CatchupAddressProvider addressProvider, StoreFileStreamProvider storeFileStream, System.Func<TerminationCondition> terminationConditions )
		 {
			  StoreCopyClientMonitor storeCopyClientMonitor = _monitors.newMonitor( typeof( StoreCopyClientMonitor ) );
			  long lastTransactionId = prepareStoreCopyResponse.LastTransactionId();
			  LongIterator indexIds = prepareStoreCopyResponse.IndexIds.longIterator();
			  storeCopyClientMonitor.StartReceivingIndexSnapshots();
			  while ( indexIds.hasNext() )
			  {
					long indexId = indexIds.next();
					storeCopyClientMonitor.StartReceivingIndexSnapshot( indexId );
					PersistentCallToSecondary( new GetIndexFilesRequest( expectedStoreId, indexId, lastTransactionId ), filesCopyAdaptor( storeFileStream, _log ), addressProvider, terminationConditions() );
					storeCopyClientMonitor.FinishReceivingIndexSnapshot( indexId );
			  }
			  storeCopyClientMonitor.FinishReceivingIndexSnapshots();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void persistentCallToSecondary(org.neo4j.causalclustering.messaging.CatchUpRequest request, org.neo4j.causalclustering.catchup.CatchUpResponseAdaptor<StoreCopyFinishedResponse> copyHandler, org.neo4j.causalclustering.catchup.CatchupAddressProvider addressProvider, TerminationCondition terminationCondition) throws StoreCopyFailedException
		 private void PersistentCallToSecondary( CatchUpRequest request, CatchUpResponseAdaptor<StoreCopyFinishedResponse> copyHandler, CatchupAddressProvider addressProvider, TerminationCondition terminationCondition )
		 {
			  Org.Neo4j.causalclustering.helper.TimeoutStrategy_Timeout timeout = _backOffStrategy.newTimeout();
			  while ( true )
			  {
					try
					{
						 AdvertisedSocketAddress address = addressProvider.Secondary();
						 _log.info( format( "Sending request '%s' to '%s'", request, address ) );
						 StoreCopyFinishedResponse response = _catchUpClient.makeBlockingRequest( address, request, copyHandler );
						 if ( SuccessfulRequest( response, request ) )
						 {
							  break;
						 }
					}
					catch ( CatchUpClientException e )
					{
						 Exception cause = e.InnerException;
						 if ( cause is ConnectException )
						 {
							  _log.warn( cause.Message );
						 }
						 else
						 {
							  _log.warn( format( "Request failed exceptionally '%s'.", request ), e );
						 }
					}
					catch ( CatchupAddressResolutionException e )
					{
						 _log.warn( "Unable to resolve address for '%s'. %s", request, e.Message );
					}
					terminationCondition();
					AwaitAndIncrementTimeout( timeout );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitAndIncrementTimeout(org.neo4j.causalclustering.helper.TimeoutStrategy_Timeout timeout) throws StoreCopyFailedException
		 private void AwaitAndIncrementTimeout( Org.Neo4j.causalclustering.helper.TimeoutStrategy_Timeout timeout )
		 {
			  try
			  {
					Thread.Sleep( timeout.Millis );
					timeout.Increment();
			  }
			  catch ( InterruptedException )
			  {
					throw new StoreCopyFailedException( "Thread interrupted" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private PrepareStoreCopyResponse prepareStoreCopy(org.neo4j.helpers.AdvertisedSocketAddress from, org.neo4j.causalclustering.identity.StoreId expectedStoreId, StoreFileStreamProvider storeFileStream) throws org.neo4j.causalclustering.catchup.CatchUpClientException, StoreCopyFailedException
		 private PrepareStoreCopyResponse PrepareStoreCopy( AdvertisedSocketAddress from, StoreId expectedStoreId, StoreFileStreamProvider storeFileStream )
		 {
			  _log.info( "Requesting store listing from: " + from );
			  PrepareStoreCopyResponse prepareStoreCopyResponse = _catchUpClient.makeBlockingRequest( from, new PrepareStoreCopyRequest( expectedStoreId ), prepareStoreCopyAdaptor( storeFileStream, _log ) );
			  if ( prepareStoreCopyResponse.Status() != PrepareStoreCopyResponse.Status.Success )
			  {
					throw new StoreCopyFailedException( "Preparing store failed due to: " + prepareStoreCopyResponse.Status() );
			  }
			  return prepareStoreCopyResponse;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.identity.StoreId fetchStoreId(org.neo4j.helpers.AdvertisedSocketAddress fromAddress) throws StoreIdDownloadFailedException
		 public virtual StoreId FetchStoreId( AdvertisedSocketAddress fromAddress )
		 {
			  try
			  {
					CatchUpResponseAdaptor<StoreId> responseHandler = new CatchUpResponseAdaptorAnonymousInnerClass( this );
					return _catchUpClient.makeBlockingRequest( fromAddress, new GetStoreIdRequest(), responseHandler );
			  }
			  catch ( CatchUpClientException e )
			  {
					throw new StoreIdDownloadFailedException( e );
			  }
		 }

		 private class CatchUpResponseAdaptorAnonymousInnerClass : CatchUpResponseAdaptor<StoreId>
		 {
			 private readonly StoreCopyClient _outerInstance;

			 public CatchUpResponseAdaptorAnonymousInnerClass( StoreCopyClient outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void onGetStoreIdResponse( CompletableFuture<StoreId> signal, GetStoreIdResponse response )
			 {
				  signal.complete( response.StoreId() );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean successfulRequest(StoreCopyFinishedResponse response, org.neo4j.causalclustering.messaging.CatchUpRequest request) throws StoreCopyFailedException
		 private bool SuccessfulRequest( StoreCopyFinishedResponse response, CatchUpRequest request )
		 {
			  StoreCopyFinishedResponse.Status responseStatus = response.Status();
			  if ( responseStatus == StoreCopyFinishedResponse.Status.Success )
			  {
					_log.info( format( "Request was successful '%s'", request ) );
					return true;
			  }
			  else if ( StoreCopyFinishedResponse.Status.ETooFarBehind == responseStatus || StoreCopyFinishedResponse.Status.EUnknown == responseStatus || StoreCopyFinishedResponse.Status.EStoreIdMismatch == responseStatus )
			  {
					_log.warn( format( "Request failed '%s'. With response: %s", request, response.Status() ) );
					return false;
			  }
			  else
			  {
					throw new StoreCopyFailedException( format( "Request responded with an unknown response type: %s. '%s'", responseStatus, request ) );
			  }
		 }
	}

}