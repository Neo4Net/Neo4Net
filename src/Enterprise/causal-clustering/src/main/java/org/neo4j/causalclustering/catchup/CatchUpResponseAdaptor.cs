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
namespace Neo4Net.causalclustering.catchup
{

	using FileChunk = Neo4Net.causalclustering.catchup.storecopy.FileChunk;
	using FileHeader = Neo4Net.causalclustering.catchup.storecopy.FileHeader;
	using GetStoreIdResponse = Neo4Net.causalclustering.catchup.storecopy.GetStoreIdResponse;
	using StoreCopyFinishedResponse = Neo4Net.causalclustering.catchup.storecopy.StoreCopyFinishedResponse;
	using PrepareStoreCopyResponse = Neo4Net.causalclustering.catchup.storecopy.PrepareStoreCopyResponse;
	using TxPullResponse = Neo4Net.causalclustering.catchup.tx.TxPullResponse;
	using TxStreamFinishedResponse = Neo4Net.causalclustering.catchup.tx.TxStreamFinishedResponse;
	using CoreSnapshot = Neo4Net.causalclustering.core.state.snapshot.CoreSnapshot;

	public class CatchUpResponseAdaptor<T> : CatchUpResponseCallback<T>
	{
		 public override void OnFileHeader( CompletableFuture<T> signal, FileHeader response )
		 {
			  UnimplementedMethod( signal, response );
		 }

		 public override bool OnFileContent( CompletableFuture<T> signal, FileChunk response )
		 {
			  UnimplementedMethod( signal, response );
			  return false;
		 }

		 public override void OnFileStreamingComplete( CompletableFuture<T> signal, StoreCopyFinishedResponse response )
		 {
			  UnimplementedMethod( signal, response );
		 }

		 public override void OnTxPullResponse( CompletableFuture<T> signal, TxPullResponse response )
		 {
			  UnimplementedMethod( signal, response );
		 }

		 public override void OnTxStreamFinishedResponse( CompletableFuture<T> signal, TxStreamFinishedResponse response )
		 {
			  UnimplementedMethod( signal, response );
		 }

		 public override void OnGetStoreIdResponse( CompletableFuture<T> signal, GetStoreIdResponse response )
		 {
			  UnimplementedMethod( signal, response );
		 }

		 public override void OnCoreSnapshot( CompletableFuture<T> signal, CoreSnapshot response )
		 {
			  UnimplementedMethod( signal, response );
		 }

		 public override void OnStoreListingResponse( CompletableFuture<T> signal, PrepareStoreCopyResponse response )
		 {
			  UnimplementedMethod( signal, response );
		 }

		 private void UnimplementedMethod<U>( CompletableFuture<T> signal, U response )
		 {
			  signal.completeExceptionally( new CatchUpProtocolViolationException( "This Adaptor has unimplemented methods for: %s", response ) );
		 }
	}

}