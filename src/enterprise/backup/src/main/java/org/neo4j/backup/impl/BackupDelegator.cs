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
namespace Neo4Net.backup.impl
{

	using CatchUpClient = Neo4Net.causalclustering.catchup.CatchUpClient;
	using CatchupAddressProvider = Neo4Net.causalclustering.catchup.CatchupAddressProvider;
	using CatchupResult = Neo4Net.causalclustering.catchup.CatchupResult;
	using RemoteStore = Neo4Net.causalclustering.catchup.storecopy.RemoteStore;
	using StoreCopyClient = Neo4Net.causalclustering.catchup.storecopy.StoreCopyClient;
	using StoreCopyFailedException = Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException;
	using StoreIdDownloadFailedException = Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	/// <summary>
	/// Simplifies the process of performing a backup over the transaction protocol by wrapping all the necessary classes
	/// and delegating methods to the correct instances.
	/// </summary>
	internal class BackupDelegator : LifecycleAdapter
	{
		 private readonly RemoteStore _remoteStore;
		 private readonly CatchUpClient _catchUpClient;
		 private readonly StoreCopyClient _storeCopyClient;

		 internal BackupDelegator( RemoteStore remoteStore, CatchUpClient catchUpClient, StoreCopyClient storeCopyClient )
		 {
			  this._remoteStore = remoteStore;
			  this._catchUpClient = catchUpClient;
			  this._storeCopyClient = storeCopyClient;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void copy(org.Neo4Net.helpers.AdvertisedSocketAddress fromAddress, org.Neo4Net.causalclustering.identity.StoreId expectedStoreId, org.Neo4Net.io.layout.DatabaseLayout databaseLayout) throws org.Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException
		 internal virtual void Copy( AdvertisedSocketAddress fromAddress, StoreId expectedStoreId, DatabaseLayout databaseLayout )
		 {
			  _remoteStore.copy( new Neo4Net.causalclustering.catchup.CatchupAddressProvider_SingleAddressProvider( fromAddress ), expectedStoreId, databaseLayout, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.causalclustering.catchup.CatchupResult tryCatchingUp(org.Neo4Net.helpers.AdvertisedSocketAddress fromAddress, org.Neo4Net.causalclustering.identity.StoreId expectedStoreId, org.Neo4Net.io.layout.DatabaseLayout databaseLayout) throws org.Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException
		 internal virtual CatchupResult TryCatchingUp( AdvertisedSocketAddress fromAddress, StoreId expectedStoreId, DatabaseLayout databaseLayout )
		 {
			  try
			  {
					return _remoteStore.tryCatchingUp( fromAddress, expectedStoreId, databaseLayout, true, true );
			  }
			  catch ( IOException e )
			  {
					throw new StoreCopyFailedException( e );
			  }
		 }

		 public override void Start()
		 {
			  _catchUpClient.start();
		 }

		 public override void Stop()
		 {
			  _catchUpClient.stop();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.causalclustering.identity.StoreId fetchStoreId(org.Neo4Net.helpers.AdvertisedSocketAddress fromAddress) throws org.Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException
		 public virtual StoreId FetchStoreId( AdvertisedSocketAddress fromAddress )
		 {
			  return _storeCopyClient.fetchStoreId( fromAddress );
		 }
	}

}