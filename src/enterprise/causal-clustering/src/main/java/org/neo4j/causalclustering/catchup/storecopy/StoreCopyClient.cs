using System;
using System.Threading;

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


	using Neo4Net.causalclustering.catchup;
	using TimeoutStrategy = Neo4Net.causalclustering.helper.TimeoutStrategy;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using CatchUpRequest = Neo4Net.causalclustering.messaging.CatchUpRequest;
	using StoreCopyClientMonitor = Neo4Net.com.storecopy.StoreCopyClientMonitor;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.catchup.storecopy.StoreCopyResponseAdaptors.filesCopyAdaptor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.catchup.storecopy.StoreCopyResponseAdaptors.prepareStoreCopyAdaptor;

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
//ORIGINAL LINE: long copyStoreFiles(Neo4Net.causalclustering.catchup.CatchupAddressProvider catchupAddressProvider, Neo4Net.causalclustering.identity.StoreId expectedStoreId, StoreFileStreamProvider storeFileStreamProvider, System.Func<TerminationCondition> requestWiseTerminationCondition, java.io.File destDir) throws StoreCopyFailedException
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
//ORIGINAL LINE: private void copyFilesIndividually(PrepareStoreCopyResponse prepareStoreCopyResponse, Neo4Net.causalclustering.identity.StoreId expectedStoreId, Neo4Net.causalclustering.catchup.CatchupAddressProvider addressProvider, StoreFileStreamProvider storeFileStream, System.Func<TerminationCondition> terminationConditions, java.io.File destDir) throws StoreCopyFailedException
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
//ORIGINAL LINE: private void copyIndexSnapshotIndividually(PrepareStoreCopyResponse prepareStoreCopyResponse, Neo4Net.causalclustering.identity.StoreId expectedStoreId, Neo4Net.causalclustering.catchup.CatchupAddressProvider addressProvider, StoreFileStreamProvider storeFileStream, System.Func<TerminationCondition> terminationConditions) throws StoreCopyFailedException
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
//ORIGINAL LINE: private void persistentCallToSecondary(Neo4Net.causalclustering.messaging.CatchUpRequest request, Neo4Net.causalclustering.catchup.CatchUpResponseAdaptor<StoreCopyFinishedResponse> copyHandler, Neo4Net.causalclustering.catchup.CatchupAddressProvider addressProvider, TerminationCondition terminationCondition) throws StoreCopyFailedException
		 private void PersistentCallToSecondary( CatchUpRequest request, CatchUpResponseAdaptor<StoreCopyFinishedResponse> copyHandler, CatchupAddressProvider addressProvider, TerminationCondition terminationCondition )
		 {
			  Neo4Net.causalclustering.helper.TimeoutStrategy_Timeout timeout = _backOffStrategy.newTimeout();
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
//ORIGINAL LINE: private void awaitAndIncrementTimeout(Neo4Net.causalclustering.helper.TimeoutStrategy_Timeout timeout) throws StoreCopyFailedException
		 private void AwaitAndIncrementTimeout( Neo4Net.causalclustering.helper.TimeoutStrategy_Timeout timeout )
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
//ORIGINAL LINE: private PrepareStoreCopyResponse prepareStoreCopy(Neo4Net.helpers.AdvertisedSocketAddress from, Neo4Net.causalclustering.identity.StoreId expectedStoreId, StoreFileStreamProvider storeFileStream) throws Neo4Net.causalclustering.catchup.CatchUpClientException, StoreCopyFailedException
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
//ORIGINAL LINE: public Neo4Net.causalclustering.identity.StoreId fetchStoreId(Neo4Net.helpers.AdvertisedSocketAddress fromAddress) throws StoreIdDownloadFailedException
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
//ORIGINAL LINE: private boolean successfulRequest(StoreCopyFinishedResponse response, Neo4Net.causalclustering.messaging.CatchUpRequest request) throws StoreCopyFailedException
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