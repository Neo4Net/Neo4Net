using System;
using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;


	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

	internal class TestCatchupServerHandler : CatchupServerHandler
	{
		 private readonly ISet<FakeFile> _filesystem = new HashSet<FakeFile>();
		 private readonly ISet<FakeFile> _indexFiles = new HashSet<FakeFile>();
		 private readonly IDictionary<string, int> _pathToRequestCountMapping = new Dictionary<string, int>();
		 private readonly Log _log;
		 private TestDirectory _testDirectory;
		 private FileSystemAbstraction _fileSystemAbstraction;

		 internal TestCatchupServerHandler( LogProvider logProvider, TestDirectory testDirectory, FileSystemAbstraction fileSystemAbstraction )
		 {
			  _log = logProvider.GetLog( typeof( TestCatchupServerHandler ) );
			  this._testDirectory = testDirectory;
			  this._fileSystemAbstraction = fileSystemAbstraction;
		 }

		 internal virtual void AddFile( FakeFile fakeFile )
		 {
			  _filesystem.Add( fakeFile );
		 }

		 internal virtual void AddIndexFile( FakeFile fakeFile )
		 {
			  _indexFiles.Add( fakeFile );
		 }

		 public virtual int GetRequestCount( string file )
		 {
			  return _pathToRequestCountMapping.getOrDefault( file, 0 );
		 }

		 public override ChannelHandler GetStoreFileRequestHandler( CatchupServerProtocol catchupServerProtocol )
		 {
			  return new SimpleChannelInboundHandlerAnonymousInnerClass( this, catchupServerProtocol );
		 }

		 private class SimpleChannelInboundHandlerAnonymousInnerClass : SimpleChannelInboundHandler<GetStoreFileRequest>
		 {
			 private readonly TestCatchupServerHandler _outerInstance;

			 private CatchupServerProtocol _catchupServerProtocol;

			 public SimpleChannelInboundHandlerAnonymousInnerClass( TestCatchupServerHandler outerInstance, CatchupServerProtocol catchupServerProtocol )
			 {
				 this.outerInstance = outerInstance;
				 this._catchupServerProtocol = catchupServerProtocol;
			 }

			 protected internal override void channelRead0( ChannelHandlerContext channelHandlerContext, GetStoreFileRequest getStoreFileRequest )
			 {
				  _outerInstance.log.info( "Received request for file %s", getStoreFileRequest.File().Name );
				  outerInstance.incrementRequestCount( getStoreFileRequest.File() );
				  try
				  {
						if ( outerInstance.handleFileDoesNotExist( channelHandlerContext, getStoreFileRequest ) )
						{
							 _catchupServerProtocol.expect( CatchupServerProtocol.State.MESSAGE_TYPE );
							 return;
						}
						outerInstance.handleFileExists( channelHandlerContext, getStoreFileRequest.File() );
				  }
				  finally
				  {
						_catchupServerProtocol.expect( CatchupServerProtocol.State.MESSAGE_TYPE );
				  }
			 }
		 }

		 private bool HandleFileDoesNotExist( ChannelHandlerContext channelHandlerContext, GetStoreFileRequest getStoreFileRequest )
		 {
			  FakeFile file = FindFile( _filesystem, getStoreFileRequest.File().Name );
			  if ( file.RemainingFailed > 0 )
			  {
					file.RemainingFailed = file.RemainingFailed - 1;
					_log.info( "FakeServer failing for file %s", getStoreFileRequest.File() );
					Failed( channelHandlerContext );
					return true;
			  }
			  return false;
		 }

		 private void Failed( ChannelHandlerContext channelHandlerContext )
		 {
			  ( new StoreFileStreamingProtocol() ).End(channelHandlerContext, StoreCopyFinishedResponse.Status.ETooFarBehind);
		 }

		 private FakeFile FindFile( ISet<FakeFile> filesystem, string filename )
		 {
			  return filesystem.Where( fakeFile => filename.Equals( fakeFile.Filename ) ).First().orElseThrow(() => new Exception("FakeFile should handle all cases with regards to how server should respond"));
		 }

		 private void HandleFileExists( ChannelHandlerContext channelHandlerContext, File file )
		 {
			  _log.info( "FakeServer File %s does exist", file );
			  channelHandlerContext.writeAndFlush( ResponseMessageType.FILE );
			  channelHandlerContext.writeAndFlush( new FileHeader( file.Name ) );
			  StoreResource storeResource = StoreResourceFromEntry( file );
			  channelHandlerContext.writeAndFlush( new FileSender( storeResource ) );
			  ( new StoreFileStreamingProtocol() ).End(channelHandlerContext, StoreCopyFinishedResponse.Status.Success);
		 }

		 private void IncrementRequestCount( File file )
		 {
			  string path = file.Name;
			  int count = _pathToRequestCountMapping.getOrDefault( path, 0 );
			  _pathToRequestCountMapping[path] = count + 1;
		 }

		 private StoreResource StoreResourceFromEntry( File file )
		 {
			  file = _testDirectory.file( file.Name );
			  return new StoreResource( file, file.AbsolutePath, 16, _fileSystemAbstraction );
		 }

		 public override ChannelHandler TxPullRequestHandler( CatchupServerProtocol catchupServerProtocol )
		 {
			  return new ChannelInboundHandlerAdapter();
		 }

		 public override ChannelHandler GetStoreIdRequestHandler( CatchupServerProtocol catchupServerProtocol )
		 {
			  return new ChannelInboundHandlerAdapter();
		 }

		 public override ChannelHandler StoreListingRequestHandler( CatchupServerProtocol catchupServerProtocol )
		 {
			  return new SimpleChannelInboundHandlerAnonymousInnerClass2( this, catchupServerProtocol );
		 }

		 private class SimpleChannelInboundHandlerAnonymousInnerClass2 : SimpleChannelInboundHandler<PrepareStoreCopyRequest>
		 {
			 private readonly TestCatchupServerHandler _outerInstance;

			 private CatchupServerProtocol _catchupServerProtocol;

			 public SimpleChannelInboundHandlerAnonymousInnerClass2( TestCatchupServerHandler outerInstance, CatchupServerProtocol catchupServerProtocol )
			 {
				 this.outerInstance = outerInstance;
				 this._catchupServerProtocol = catchupServerProtocol;
			 }


			 protected internal override void channelRead0( ChannelHandlerContext channelHandlerContext, PrepareStoreCopyRequest prepareStoreCopyRequest )
			 {
				  channelHandlerContext.writeAndFlush( ResponseMessageType.PREPARE_STORE_COPY_RESPONSE );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				  IList<File> list = _outerInstance.filesystem.Select( FakeFile::getFile ).ToList();
				  File[] files = new File[list.Count];
				  files = list.toArray( files );
				  long transactionId = 123L;
				  LongSet indexIds = LongSets.immutable.of( 13 );
				  channelHandlerContext.writeAndFlush( PrepareStoreCopyResponse.Success( files, indexIds, transactionId ) );
				  _catchupServerProtocol.expect( CatchupServerProtocol.State.MESSAGE_TYPE );
			 }
		 }

		 public override ChannelHandler GetIndexSnapshotRequestHandler( CatchupServerProtocol catchupServerProtocol )
		 {
			  return new SimpleChannelInboundHandlerAnonymousInnerClass3( this, catchupServerProtocol );
		 }

		 private class SimpleChannelInboundHandlerAnonymousInnerClass3 : SimpleChannelInboundHandler<GetIndexFilesRequest>
		 {
			 private readonly TestCatchupServerHandler _outerInstance;

			 private CatchupServerProtocol _catchupServerProtocol;

			 public SimpleChannelInboundHandlerAnonymousInnerClass3( TestCatchupServerHandler outerInstance, CatchupServerProtocol catchupServerProtocol )
			 {
				 this.outerInstance = outerInstance;
				 this._catchupServerProtocol = catchupServerProtocol;
			 }

			 protected internal override void channelRead0( ChannelHandlerContext channelHandlerContext, GetIndexFilesRequest snapshotRequest )
			 {
				  _outerInstance.log.info( "Received request for index %s", snapshotRequest.IndexId() );
				  try
				  {
						foreach ( FakeFile indexFile in _outerInstance.indexFiles )
						{
							 _outerInstance.log.info( "FakeServer File %s does exist", indexFile.File );
							 channelHandlerContext.writeAndFlush( ResponseMessageType.FILE );
							 channelHandlerContext.writeAndFlush( new FileHeader( indexFile.File.Name ) );
							 StoreResource storeResource = outerInstance.storeResourceFromEntry( indexFile.File );
							 channelHandlerContext.writeAndFlush( new FileSender( storeResource ) );
						}
						( new StoreFileStreamingProtocol() ).End(channelHandlerContext, StoreCopyFinishedResponse.Status.Success);
				  }
				  finally
				  {
						_catchupServerProtocol.expect( CatchupServerProtocol.State.MESSAGE_TYPE );
				  }
			 }
		 }

		 public override Optional<ChannelHandler> SnapshotHandler( CatchupServerProtocol catchupServerProtocol )
		 {
			  return null;
		 }

		 public virtual StoreId StoreId
		 {
			 get
			 {
				  return new StoreId( 1, 2, 3, 4 );
			 }
		 }
	}

}