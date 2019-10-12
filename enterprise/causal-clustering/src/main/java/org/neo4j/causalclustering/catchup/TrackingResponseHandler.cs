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
namespace Org.Neo4j.causalclustering.catchup
{

	using FileChunk = Org.Neo4j.causalclustering.catchup.storecopy.FileChunk;
	using FileHeader = Org.Neo4j.causalclustering.catchup.storecopy.FileHeader;
	using GetStoreIdResponse = Org.Neo4j.causalclustering.catchup.storecopy.GetStoreIdResponse;
	using PrepareStoreCopyResponse = Org.Neo4j.causalclustering.catchup.storecopy.PrepareStoreCopyResponse;
	using StoreCopyFinishedResponse = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyFinishedResponse;
	using TxPullResponse = Org.Neo4j.causalclustering.catchup.tx.TxPullResponse;
	using TxStreamFinishedResponse = Org.Neo4j.causalclustering.catchup.tx.TxStreamFinishedResponse;
	using CoreSnapshot = Org.Neo4j.causalclustering.core.state.snapshot.CoreSnapshot;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") class TrackingResponseHandler implements CatchUpResponseHandler
	internal class TrackingResponseHandler : CatchUpResponseHandler
	{
		 private CatchUpResponseCallback @delegate;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.concurrent.CompletableFuture<?> requestOutcomeSignal = new java.util.concurrent.CompletableFuture<>();
		 private CompletableFuture<object> _requestOutcomeSignal = new CompletableFuture<object>();
		 private readonly Clock _clock;
		 private long? _lastResponseTime;

		 internal TrackingResponseHandler( CatchUpResponseCallback @delegate, Clock clock )
		 {
			  this.@delegate = @delegate;
			  this._clock = clock;
		 }

		 internal virtual void SetResponseHandler<T1>( CatchUpResponseCallback responseHandler, CompletableFuture<T1> requestOutcomeSignal )
		 {
			  this.@delegate = responseHandler;
			  this._requestOutcomeSignal = requestOutcomeSignal;
		 }

		 public override void OnFileHeader( FileHeader fileHeader )
		 {
			  if ( !_requestOutcomeSignal.Cancelled )
			  {
					RecordLastResponse();
					@delegate.onFileHeader( _requestOutcomeSignal, fileHeader );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean onFileContent(org.neo4j.causalclustering.catchup.storecopy.FileChunk fileChunk) throws java.io.IOException
		 public override bool OnFileContent( FileChunk fileChunk )
		 {
			  if ( !_requestOutcomeSignal.Cancelled )
			  {
					RecordLastResponse();
					return @delegate.onFileContent( _requestOutcomeSignal, fileChunk );
			  }
			  // true means stop
			  return true;
		 }

		 public override void OnFileStreamingComplete( StoreCopyFinishedResponse response )
		 {
			  if ( !_requestOutcomeSignal.Cancelled )
			  {
					RecordLastResponse();
					@delegate.onFileStreamingComplete( _requestOutcomeSignal, response );
			  }
		 }

		 public override void OnTxPullResponse( TxPullResponse tx )
		 {
			  if ( !_requestOutcomeSignal.Cancelled )
			  {
					RecordLastResponse();
					@delegate.onTxPullResponse( _requestOutcomeSignal, tx );
			  }
		 }

		 public override void OnTxStreamFinishedResponse( TxStreamFinishedResponse response )
		 {
			  if ( !_requestOutcomeSignal.Cancelled )
			  {
					RecordLastResponse();
					@delegate.onTxStreamFinishedResponse( _requestOutcomeSignal, response );
			  }
		 }

		 public override void OnGetStoreIdResponse( GetStoreIdResponse response )
		 {
			  if ( !_requestOutcomeSignal.Cancelled )
			  {
					RecordLastResponse();
					@delegate.onGetStoreIdResponse( _requestOutcomeSignal, response );
			  }
		 }

		 public override void OnCoreSnapshot( CoreSnapshot coreSnapshot )
		 {
			  if ( !_requestOutcomeSignal.Cancelled )
			  {
					RecordLastResponse();
					@delegate.onCoreSnapshot( _requestOutcomeSignal, coreSnapshot );
			  }
		 }

		 public override void OnStoreListingResponse( PrepareStoreCopyResponse storeListingRequest )
		 {
			  if ( !_requestOutcomeSignal.Cancelled )
			  {
					RecordLastResponse();
					@delegate.onStoreListingResponse( _requestOutcomeSignal, storeListingRequest );
			  }
		 }

		 public override void OnClose()
		 {
			  _requestOutcomeSignal.completeExceptionally( new ClosedChannelException() );
		 }

		 internal virtual long? LastResponseTime()
		 {
			  return Optional.ofNullable( _lastResponseTime );
		 }

		 private void RecordLastResponse()
		 {
			  _lastResponseTime = _clock.millis();
		 }
	}

}