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

	public interface ICatchUpResponseCallback<T>
	{
		 void OnFileHeader( CompletableFuture<T> signal, FileHeader fileHeader );

		 bool OnFileContent( CompletableFuture<T> signal, FileChunk fileChunk );

		 void OnFileStreamingComplete( CompletableFuture<T> signal, StoreCopyFinishedResponse response );

		 void OnTxPullResponse( CompletableFuture<T> signal, TxPullResponse tx );

		 void OnTxStreamFinishedResponse( CompletableFuture<T> signal, TxStreamFinishedResponse response );

		 void OnGetStoreIdResponse( CompletableFuture<T> signal, GetStoreIdResponse response );

		 void OnCoreSnapshot( CompletableFuture<T> signal, CoreSnapshot coreSnapshot );

		 void OnStoreListingResponse( CompletableFuture<T> signal, PrepareStoreCopyResponse prepareStoreCopyResponse );
	}

}