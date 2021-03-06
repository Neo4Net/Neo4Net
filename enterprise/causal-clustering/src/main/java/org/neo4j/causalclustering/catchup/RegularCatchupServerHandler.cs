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
	using ChannelHandler = io.netty.channel.ChannelHandler;


	using GetStoreIdRequestHandler = Org.Neo4j.causalclustering.catchup.storecopy.GetStoreIdRequestHandler;
	using PrepareStoreCopyFilesProvider = Org.Neo4j.causalclustering.catchup.storecopy.PrepareStoreCopyFilesProvider;
	using PrepareStoreCopyRequestHandler = Org.Neo4j.causalclustering.catchup.storecopy.PrepareStoreCopyRequestHandler;
	using Org.Neo4j.causalclustering.catchup.storecopy;
	using StoreFileStreamingProtocol = Org.Neo4j.causalclustering.catchup.storecopy.StoreFileStreamingProtocol;
	using TxPullRequestHandler = Org.Neo4j.causalclustering.catchup.tx.TxPullRequestHandler;
	using CoreSnapshotService = Org.Neo4j.causalclustering.core.state.CoreSnapshotService;
	using CoreSnapshotRequestHandler = Org.Neo4j.causalclustering.core.state.snapshot.CoreSnapshotRequestHandler;
	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class RegularCatchupServerHandler : CatchupServerHandler
	{

		 private readonly Monitors _monitors;
		 private readonly LogProvider _logProvider;
		 private readonly System.Func<StoreId> _storeIdSupplier;
		 private readonly System.Func<NeoStoreDataSource> _dataSourceSupplier;
		 private readonly System.Func<bool> _dataSourceAvailabilitySupplier;
		 private readonly FileSystemAbstraction _fs;
		 private readonly CoreSnapshotService _snapshotService;
		 private readonly CheckPointerService _checkPointerService;

		 public RegularCatchupServerHandler( Monitors monitors, LogProvider logProvider, System.Func<StoreId> storeIdSupplier, System.Func<NeoStoreDataSource> dataSourceSupplier, System.Func<bool> dataSourceAvailabilitySupplier, FileSystemAbstraction fs, CoreSnapshotService snapshotService, CheckPointerService checkPointerService )
		 {
			  this._monitors = monitors;
			  this._logProvider = logProvider;
			  this._storeIdSupplier = storeIdSupplier;
			  this._dataSourceSupplier = dataSourceSupplier;
			  this._dataSourceAvailabilitySupplier = dataSourceAvailabilitySupplier;
			  this._fs = fs;
			  this._snapshotService = snapshotService;
			  this._checkPointerService = checkPointerService;
		 }

		 public override ChannelHandler TxPullRequestHandler( CatchupServerProtocol catchupServerProtocol )
		 {
			  return new TxPullRequestHandler( catchupServerProtocol, _storeIdSupplier, _dataSourceAvailabilitySupplier, _dataSourceSupplier, _monitors, _logProvider );
		 }

		 public override ChannelHandler GetStoreIdRequestHandler( CatchupServerProtocol catchupServerProtocol )
		 {
			  return new GetStoreIdRequestHandler( catchupServerProtocol, _storeIdSupplier );
		 }

		 public override ChannelHandler StoreListingRequestHandler( CatchupServerProtocol catchupServerProtocol )
		 {
			  return new PrepareStoreCopyRequestHandler( catchupServerProtocol, _dataSourceSupplier, new PrepareStoreCopyFilesProvider( _fs ) );
		 }

		 public override ChannelHandler GetStoreFileRequestHandler( CatchupServerProtocol catchupServerProtocol )
		 {
			  return new StoreCopyRequestHandler.GetStoreFileRequestHandler( catchupServerProtocol, _dataSourceSupplier, _checkPointerService, new StoreFileStreamingProtocol(), _fs, _logProvider );
		 }

		 public override ChannelHandler GetIndexSnapshotRequestHandler( CatchupServerProtocol catchupServerProtocol )
		 {
			  return new StoreCopyRequestHandler.GetIndexSnapshotRequestHandler( catchupServerProtocol, _dataSourceSupplier, _checkPointerService, new StoreFileStreamingProtocol(), _fs, _logProvider );
		 }

		 public override Optional<ChannelHandler> SnapshotHandler( CatchupServerProtocol catchupServerProtocol )
		 {
			  return Optional.ofNullable( ( _snapshotService != null ) ? new CoreSnapshotRequestHandler( catchupServerProtocol, _snapshotService ) : null );
		 }
	}

}