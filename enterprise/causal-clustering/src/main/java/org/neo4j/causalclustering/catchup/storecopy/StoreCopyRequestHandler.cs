﻿using System.Collections.Generic;

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
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;


	using StoreCopyRequest = Org.Neo4j.causalclustering.messaging.StoreCopyRequest;
	using Org.Neo4j.Graphdb;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using StoreFileMetadata = Org.Neo4j.Storageengine.Api.StoreFileMetadata;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.DataSourceChecks.hasSameStoreId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.DataSourceChecks.isTransactionWithinReach;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.fs.FileUtils.relativePath;

	public abstract class StoreCopyRequestHandler<T> : SimpleChannelInboundHandler<T> where T : Org.Neo4j.causalclustering.messaging.StoreCopyRequest
	{
		 private readonly CatchupServerProtocol _protocol;
		 private readonly System.Func<NeoStoreDataSource> _dataSource;
		 private readonly CheckPointerService _checkPointerService;
		 private readonly StoreFileStreamingProtocol _storeFileStreamingProtocol;

		 private readonly FileSystemAbstraction _fs;
		 private readonly Log _log;

		 internal StoreCopyRequestHandler( CatchupServerProtocol protocol, System.Func<NeoStoreDataSource> dataSource, CheckPointerService checkPointerService, StoreFileStreamingProtocol storeFileStreamingProtocol, FileSystemAbstraction fs, LogProvider logProvider )
		 {
			  this._protocol = protocol;
			  this._dataSource = dataSource;
			  this._storeFileStreamingProtocol = storeFileStreamingProtocol;
			  this._fs = fs;
			  this._log = logProvider.GetLog( typeof( StoreCopyRequestHandler ) );
			  this._checkPointerService = checkPointerService;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void channelRead0(io.netty.channel.ChannelHandlerContext ctx, T request) throws Exception
		 protected internal override void ChannelRead0( ChannelHandlerContext ctx, T request )
		 {
			  _log.debug( "Handling request %s", request );
			  StoreCopyFinishedResponse.Status responseStatus = StoreCopyFinishedResponse.Status.EUnknown;
			  try
			  {
					NeoStoreDataSource neoStoreDataSource = _dataSource.get();
					if ( !hasSameStoreId( request.expectedStoreId(), neoStoreDataSource ) )
					{
						 responseStatus = StoreCopyFinishedResponse.Status.EStoreIdMismatch;
					}
					else if ( !isTransactionWithinReach( request.requiredTransactionId(), _checkPointerService ) )
					{
						 responseStatus = StoreCopyFinishedResponse.Status.ETooFarBehind;
						 _checkPointerService.tryAsyncCheckpoint( e => _log.error( "Failed to do a checkpoint that was invoked after a too far behind error on store copy request", e ) );
					}
					else
					{
						 File databaseDirectory = neoStoreDataSource.DatabaseLayout.databaseDirectory();
						 using ( ResourceIterator<StoreFileMetadata> resourceIterator = Files( request, neoStoreDataSource ) )
						 {
							  while ( resourceIterator.MoveNext() )
							  {
									StoreFileMetadata storeFileMetadata = resourceIterator.Current;
									StoreResource storeResource = new StoreResource( storeFileMetadata.File(), relativePath(databaseDirectory, storeFileMetadata.File()), storeFileMetadata.RecordSize(), _fs );
									_storeFileStreamingProtocol.stream( ctx, storeResource );
							  }
						 }
						 responseStatus = StoreCopyFinishedResponse.Status.Success;
					}
			  }
			  finally
			  {
					_storeFileStreamingProtocol.end( ctx, responseStatus );
					_protocol.expect( CatchupServerProtocol.State.MESSAGE_TYPE );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract org.neo4j.graphdb.ResourceIterator<org.neo4j.storageengine.api.StoreFileMetadata> files(T request, org.neo4j.kernel.NeoStoreDataSource neoStoreDataSource) throws java.io.IOException;
		 internal abstract ResourceIterator<StoreFileMetadata> Files( T request, NeoStoreDataSource neoStoreDataSource );

		 private static IEnumerator<StoreFileMetadata> OnlyOne( IList<StoreFileMetadata> files, string description )
		 {
			  if ( Files.Count != 1 )
			  {
					throw new System.InvalidOperationException( format( "Expected exactly one file '%s'. Got %d", description, Files.Count ) );
			  }
			  return Files.GetEnumerator();
		 }

		 private static System.Predicate<StoreFileMetadata> MatchesRequested( string fileName )
		 {
			  return f => f.file().Name.Equals(fileName);
		 }

		 public class GetStoreFileRequestHandler : StoreCopyRequestHandler<GetStoreFileRequest>
		 {
			  public GetStoreFileRequestHandler( CatchupServerProtocol protocol, System.Func<NeoStoreDataSource> dataSource, CheckPointerService checkPointerService, StoreFileStreamingProtocol storeFileStreamingProtocol, FileSystemAbstraction fs, LogProvider logProvider ) : base( protocol, dataSource, checkPointerService, storeFileStreamingProtocol, fs, logProvider )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ResourceIterator<org.neo4j.storageengine.api.StoreFileMetadata> files(GetStoreFileRequest request, org.neo4j.kernel.NeoStoreDataSource neoStoreDataSource) throws java.io.IOException
			  internal override ResourceIterator<StoreFileMetadata> Files( GetStoreFileRequest request, NeoStoreDataSource neoStoreDataSource )
			  {
					using ( ResourceIterator<StoreFileMetadata> resourceIterator = neoStoreDataSource.ListStoreFiles( false ) )
					{
						 string fileName = request.File().Name;
						 return Iterators.asResourceIterator( OnlyOne( resourceIterator.Where( MatchesRequested( fileName ) ).ToList(), fileName ) );
					}
			  }
		 }

		 public class GetIndexSnapshotRequestHandler : StoreCopyRequestHandler<GetIndexFilesRequest>
		 {
			  public GetIndexSnapshotRequestHandler( CatchupServerProtocol protocol, System.Func<NeoStoreDataSource> dataSource, CheckPointerService checkPointerService, StoreFileStreamingProtocol storeFileStreamingProtocol, FileSystemAbstraction fs, LogProvider logProvider ) : base( protocol, dataSource, checkPointerService, storeFileStreamingProtocol, fs, logProvider )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ResourceIterator<org.neo4j.storageengine.api.StoreFileMetadata> files(GetIndexFilesRequest request, org.neo4j.kernel.NeoStoreDataSource neoStoreDataSource) throws java.io.IOException
			  internal override ResourceIterator<StoreFileMetadata> Files( GetIndexFilesRequest request, NeoStoreDataSource neoStoreDataSource )
			  {
					return neoStoreDataSource.NeoStoreFileListing.NeoStoreFileIndexListing.getSnapshot( request.IndexId() );
			  }
		 }
	}

}