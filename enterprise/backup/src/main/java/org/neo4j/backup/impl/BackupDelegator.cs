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
namespace Org.Neo4j.backup.impl
{

	using CatchUpClient = Org.Neo4j.causalclustering.catchup.CatchUpClient;
	using CatchupAddressProvider = Org.Neo4j.causalclustering.catchup.CatchupAddressProvider;
	using CatchupResult = Org.Neo4j.causalclustering.catchup.CatchupResult;
	using RemoteStore = Org.Neo4j.causalclustering.catchup.storecopy.RemoteStore;
	using StoreCopyClient = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyClient;
	using StoreCopyFailedException = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyFailedException;
	using StoreIdDownloadFailedException = Org.Neo4j.causalclustering.catchup.storecopy.StoreIdDownloadFailedException;
	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;

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
//ORIGINAL LINE: void copy(org.neo4j.helpers.AdvertisedSocketAddress fromAddress, org.neo4j.causalclustering.identity.StoreId expectedStoreId, org.neo4j.io.layout.DatabaseLayout databaseLayout) throws org.neo4j.causalclustering.catchup.storecopy.StoreCopyFailedException
		 internal virtual void Copy( AdvertisedSocketAddress fromAddress, StoreId expectedStoreId, DatabaseLayout databaseLayout )
		 {
			  _remoteStore.copy( new Org.Neo4j.causalclustering.catchup.CatchupAddressProvider_SingleAddressProvider( fromAddress ), expectedStoreId, databaseLayout, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.causalclustering.catchup.CatchupResult tryCatchingUp(org.neo4j.helpers.AdvertisedSocketAddress fromAddress, org.neo4j.causalclustering.identity.StoreId expectedStoreId, org.neo4j.io.layout.DatabaseLayout databaseLayout) throws org.neo4j.causalclustering.catchup.storecopy.StoreCopyFailedException
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
//ORIGINAL LINE: public org.neo4j.causalclustering.identity.StoreId fetchStoreId(org.neo4j.helpers.AdvertisedSocketAddress fromAddress) throws org.neo4j.causalclustering.catchup.storecopy.StoreIdDownloadFailedException
		 public virtual StoreId FetchStoreId( AdvertisedSocketAddress fromAddress )
		 {
			  return _storeCopyClient.fetchStoreId( fromAddress );
		 }
	}

}