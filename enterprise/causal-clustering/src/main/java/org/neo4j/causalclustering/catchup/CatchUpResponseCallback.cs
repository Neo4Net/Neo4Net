﻿/*
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
	using StoreCopyFinishedResponse = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyFinishedResponse;
	using PrepareStoreCopyResponse = Org.Neo4j.causalclustering.catchup.storecopy.PrepareStoreCopyResponse;
	using TxPullResponse = Org.Neo4j.causalclustering.catchup.tx.TxPullResponse;
	using TxStreamFinishedResponse = Org.Neo4j.causalclustering.catchup.tx.TxStreamFinishedResponse;
	using CoreSnapshot = Org.Neo4j.causalclustering.core.state.snapshot.CoreSnapshot;

	public interface CatchUpResponseCallback<T>
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