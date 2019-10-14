using System;
using System.Collections.Generic;
using System.Text;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;
	using Charsets = org.apache.commons.compress.utils.Charsets;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using Matchers = org.hamcrest.Matchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ConstantTimeTimeoutStrategy = Neo4Net.causalclustering.helper.ConstantTimeTimeoutStrategy;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using Server = Neo4Net.causalclustering.net.Server;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using DuplicatingLogProvider = Neo4Net.Logging.DuplicatingLogProvider;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using Level = Neo4Net.Logging.Level;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class StoreCopyClientIT
	{
		private bool InstanceFieldsInitialized = false;

		public StoreCopyClientIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			TestDirectory = TestDirectory.testDirectory( _fsa );
		}

		 private readonly FileSystemAbstraction _fsa = new DefaultFileSystemAbstraction();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory(fsa);
		 public TestDirectory TestDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

		 private readonly AssertableLogProvider _assertableLogProvider = new AssertableLogProvider( true );
		 private readonly TerminationCondition _defaultTerminationCondition = TerminationCondition_Fields.ContinueIndefinitely;
		 private readonly FakeFile _fileA = new FakeFile( "fileA", "This is file a content" );
		 private readonly FakeFile _fileB = new FakeFile( "another-file-b", "Totally different content 123" );
		 private readonly FakeFile _indexFileA = new FakeFile( "lucene", "Lucene 123" );
		 private readonly File _targetLocation = new File( "copyTargetLocation" );
		 private LogProvider _logProvider;
		 private StoreCopyClient _subject;
		 private Server _catchupServer;
		 private TestCatchupServerHandler _serverHandler;

		 private static void WriteContents( FileSystemAbstraction fileSystemAbstraction, File file, string contents )
		 {
			  sbyte[] bytes = contents.GetBytes();
			  try
			  {
					  using ( StoreChannel storeChannel = fileSystemAbstraction.Create( file ) )
					  {
						storeChannel.write( ByteBuffer.wrap( bytes ) );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _logProvider = new DuplicatingLogProvider( _assertableLogProvider, FormattedLogProvider.withDefaultLogLevel( Level.DEBUG ).toOutputStream( System.out ) );
			  _serverHandler = new TestCatchupServerHandler( _logProvider, TestDirectory, _fsa );
			  _serverHandler.addFile( _fileA );
			  _serverHandler.addFile( _fileB );
			  _serverHandler.addIndexFile( _indexFileA );
			  WriteContents( _fsa, Relative( _fileA.Filename ), _fileA.Content );
			  WriteContents( _fsa, Relative( _fileB.Filename ), _fileB.Content );
			  WriteContents( _fsa, Relative( _indexFileA.Filename ), _indexFileA.Content );

			  ListenSocketAddress listenAddress = new ListenSocketAddress( "localhost", PortAuthority.allocatePort() );
			  _catchupServer = ( new CatchupServerBuilder( _serverHandler ) ).listenAddress( listenAddress ).build();
			  _catchupServer.start();

			  CatchUpClient catchUpClient = ( new CatchupClientBuilder() ).build();
			  catchUpClient.Start();

			  ConstantTimeTimeoutStrategy storeCopyBackoffStrategy = new ConstantTimeTimeoutStrategy( 1, TimeUnit.MILLISECONDS );

			  Monitors monitors = new Monitors();
			  _subject = new StoreCopyClient( catchUpClient, monitors, _logProvider, storeCopyBackoffStrategy );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutdown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Shutdown()
		 {
			  _catchupServer.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canPerformCatchup() throws StoreCopyFailedException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanPerformCatchup()
		 {
			  // given local client has a store
			  InMemoryStoreStreamProvider storeFileStream = new InMemoryStoreStreamProvider();

			  // when catchup is performed for valid transactionId and StoreId
			  CatchupAddressProvider catchupAddressProvider = CatchupAddressProvider.fromSingleAddress( From( _catchupServer.address().Port ) );
			  _subject.copyStoreFiles( catchupAddressProvider, _serverHandler.StoreId, storeFileStream, () => _defaultTerminationCondition, _targetLocation );

			  // then the catchup is successful
			  ISet<string> expectedFiles = new HashSet<string>( Arrays.asList( _fileA.Filename, _fileB.Filename, _indexFileA.Filename ) );
			  assertEquals( expectedFiles, storeFileStream.FileStreams().Keys );
			  assertEquals( FileContent( Relative( _fileA.Filename ) ), ClientFileContents( storeFileStream, _fileA.Filename ) );
			  assertEquals( FileContent( Relative( _fileB.Filename ) ), ClientFileContents( storeFileStream, _fileB.Filename ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failedFileCopyShouldRetry() throws StoreCopyFailedException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailedFileCopyShouldRetry()
		 {
			  // given a file will fail twice before succeeding
			  _fileB.RemainingFailed = 2;

			  // and remote node has a store
			  // and local client has a store
			  InMemoryStoreStreamProvider clientStoreFileStream = new InMemoryStoreStreamProvider();

			  // when catchup is performed for valid transactionId and StoreId
			  CatchupAddressProvider catchupAddressProvider = CatchupAddressProvider.fromSingleAddress( From( _catchupServer.address().Port ) );
			  _subject.copyStoreFiles( catchupAddressProvider, _serverHandler.StoreId, clientStoreFileStream, () => _defaultTerminationCondition, _targetLocation );

			  // then the catchup is successful
			  ISet<string> expectedFiles = new HashSet<string>( Arrays.asList( _fileA.Filename, _fileB.Filename, _indexFileA.Filename ) );
			  assertEquals( expectedFiles, clientStoreFileStream.FileStreams().Keys );

			  // and
			  assertEquals( FileContent( Relative( _fileA.Filename ) ), ClientFileContents( clientStoreFileStream, _fileA.Filename ) );
			  assertEquals( FileContent( Relative( _fileB.Filename ) ), ClientFileContents( clientStoreFileStream, _fileB.Filename ) );

			  // and verify server had exactly 2 failed calls before having a 3rd succeeding request
			  assertEquals( 3, _serverHandler.getRequestCount( _fileB.Filename ) );

			  // and verify server had exactly 1 call for all other files
			  assertEquals( 1, _serverHandler.getRequestCount( _fileA.Filename ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAppendToFileWhenRetryingWithNewFile() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAppendToFileWhenRetryingWithNewFile()
		 {
			  // given
			  string fileName = "foo";
			  string copyFileName = "bar";
			  string unfinishedContent = "abcd";
			  string finishedContent = "abcdefgh";
			  IEnumerator<string> contents = Iterators.iterator( unfinishedContent, finishedContent );

			  // and
			  TestCatchupServerHandler halfWayFailingServerhandler = new TestCatchupServerHandlerAnonymousInnerClass( this, _logProvider, TestDirectory, _fsa, fileName, copyFileName, contents );

			  Server halfWayFailingServer = null;

			  try
			  {
					// when
					ListenSocketAddress listenAddress = new ListenSocketAddress( "localhost", PortAuthority.allocatePort() );
					halfWayFailingServer = ( new CatchupServerBuilder( halfWayFailingServerhandler ) ).listenAddress( listenAddress ).build();
					halfWayFailingServer.Start();

					CatchupAddressProvider addressProvider = CatchupAddressProvider.fromSingleAddress( new AdvertisedSocketAddress( listenAddress.Hostname, listenAddress.Port ) );

					StoreId storeId = halfWayFailingServerhandler.StoreId;
					File databaseDir = TestDirectory.databaseDir();
					StreamToDiskProvider streamToDiskProvider = new StreamToDiskProvider( databaseDir, _fsa, new Monitors() );

					// and
					_subject.copyStoreFiles( addressProvider, storeId, streamToDiskProvider, () => _defaultTerminationCondition, _targetLocation );

					// then
					assertEquals( FileContent( new File( databaseDir, fileName ) ), finishedContent );

					// and
					File fileCopy = new File( databaseDir, copyFileName );

					ByteBuffer buffer = ByteBuffer.wrap( new sbyte[finishedContent.Length] );
					using ( StoreChannel storeChannel = _fsa.create( fileCopy ) )
					{
						 storeChannel.read( buffer );
					}
					assertEquals( finishedContent, new string( buffer.array(), Charsets.UTF_8 ) );
			  }
			  finally
			  {
					halfWayFailingServer.Stop();
					halfWayFailingServer.Shutdown();
			  }
		 }

		 private class TestCatchupServerHandlerAnonymousInnerClass : TestCatchupServerHandler
		 {
			 private readonly StoreCopyClientIT _outerInstance;

			 private string _fileName;
			 private string _copyFileName;
			 private IEnumerator<string> _contents;

			 public TestCatchupServerHandlerAnonymousInnerClass( StoreCopyClientIT outerInstance, LogProvider logProvider, TestDirectory testDirectory, FileSystemAbstraction fsa, string fileName, string copyFileName, IEnumerator<string> contents ) : base( logProvider, testDirectory, fsa )
			 {
				 this.outerInstance = outerInstance;
				 this._fileName = fileName;
				 this._copyFileName = copyFileName;
				 this._contents = contents;
			 }

			 public override ChannelHandler getStoreFileRequestHandler( CatchupServerProtocol catchupServerProtocol )
			 {
				  return new SimpleChannelInboundHandlerAnonymousInnerClass( this, catchupServerProtocol );
			 }

			 private class SimpleChannelInboundHandlerAnonymousInnerClass : SimpleChannelInboundHandler<GetStoreFileRequest>
			 {
				 private readonly TestCatchupServerHandlerAnonymousInnerClass _outerInstance;

				 private CatchupServerProtocol _catchupServerProtocol;

				 public SimpleChannelInboundHandlerAnonymousInnerClass( TestCatchupServerHandlerAnonymousInnerClass outerInstance, CatchupServerProtocol catchupServerProtocol )
				 {
					 this.outerInstance = outerInstance;
					 this._catchupServerProtocol = catchupServerProtocol;
				 }

				 protected internal override void channelRead0( ChannelHandlerContext ctx, GetStoreFileRequest msg )
				 {
					  // create the files and write the given content
					  File file = new File( _outerInstance.fileName );
					  File fileCopy = new File( _outerInstance.copyFileName );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					  string thisConent = _outerInstance.contents.next();
					  WriteContents( _outerInstance.outerInstance.fsa, file, thisConent );
					  WriteContents( _outerInstance.outerInstance.fsa, fileCopy, thisConent );

					  sendFile( ctx, file );
					  sendFile( ctx, fileCopy );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					  StoreCopyFinishedResponse.Status status = _outerInstance.contents.hasNext() ? StoreCopyFinishedResponse.Status.EUnknown : StoreCopyFinishedResponse.Status.Success;
					  ( new StoreFileStreamingProtocol() ).End(ctx, status);
					  _catchupServerProtocol.expect( CatchupServerProtocol.State.MESSAGE_TYPE );
				 }

				 private void sendFile( ChannelHandlerContext ctx, File file )
				 {
					  ctx.write( ResponseMessageType.FILE );
					  ctx.write( new FileHeader( file.Name ) );
					  ctx.writeAndFlush( new FileSender( new StoreResource( file, file.Name, 16, _outerInstance.outerInstance.fsa ) ) ).addListener( future => _outerInstance.outerInstance.fsa.deleteFile( file ) );
				 }
			 }

			 public override ChannelHandler storeListingRequestHandler( CatchupServerProtocol catchupServerProtocol )
			 {
				  return new SimpleChannelInboundHandlerAnonymousInnerClass2( this, catchupServerProtocol );
			 }

			 private class SimpleChannelInboundHandlerAnonymousInnerClass2 : SimpleChannelInboundHandler<PrepareStoreCopyRequest>
			 {
				 private readonly TestCatchupServerHandlerAnonymousInnerClass _outerInstance;

				 private CatchupServerProtocol _catchupServerProtocol;

				 public SimpleChannelInboundHandlerAnonymousInnerClass2( TestCatchupServerHandlerAnonymousInnerClass outerInstance, CatchupServerProtocol catchupServerProtocol )
				 {
					 this.outerInstance = outerInstance;
					 this._catchupServerProtocol = catchupServerProtocol;
				 }

				 protected internal override void channelRead0( ChannelHandlerContext ctx, PrepareStoreCopyRequest msg )
				 {
					  ctx.write( ResponseMessageType.PREPARE_STORE_COPY_RESPONSE );
					  ctx.writeAndFlush( PrepareStoreCopyResponse.Success( new File[]{ new File( _outerInstance.fileName ) }, LongSets.immutable.empty(), 1 ) );
					  _catchupServerProtocol.expect( CatchupServerProtocol.State.MESSAGE_TYPE );
				 }
			 }

			 public override ChannelHandler getIndexSnapshotRequestHandler( CatchupServerProtocol catchupServerProtocol )
			 {
				  return new SimpleChannelInboundHandlerAnonymousInnerClass3( this );
			 }

			 private class SimpleChannelInboundHandlerAnonymousInnerClass3 : SimpleChannelInboundHandler<GetIndexFilesRequest>
			 {
				 private readonly TestCatchupServerHandlerAnonymousInnerClass _outerInstance;

				 public SimpleChannelInboundHandlerAnonymousInnerClass3( TestCatchupServerHandlerAnonymousInnerClass outerInstance )
				 {
					 this.outerInstance = outerInstance;
				 }

				 protected internal override void channelRead0( ChannelHandlerContext ctx, GetIndexFilesRequest msg )
				 {
					  throw new System.InvalidOperationException( "There should not be any index requests" );
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogConnetionRefusedMessage()
		 public virtual void ShouldLogConnetionRefusedMessage()
		 {
			  InMemoryStoreStreamProvider clientStoreFileStream = new InMemoryStoreStreamProvider();
			  int port = PortAuthority.allocatePort();
			  try
			  {
					_subject.copyStoreFiles(new CatchupAddressProviderAnonymousInnerClass(this, port)
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				  , _serverHandler.StoreId, clientStoreFileStream, Once::new, _targetLocation);
					fail();
			  }
			  catch ( StoreCopyFailedException )
			  {

					_assertableLogProvider.rawMessageMatcher().assertContainsSingle(Matchers.allOf(Matchers.containsString("Connection refused: localhost/127.0.0.1:" + port)));
			  }
		 }

		 private class CatchupAddressProviderAnonymousInnerClass : CatchupAddressProvider
		 {
			 private readonly StoreCopyClientIT _outerInstance;

			 private int _port;

			 public CatchupAddressProviderAnonymousInnerClass( StoreCopyClientIT outerInstance, int port )
			 {
				 this.outerInstance = outerInstance;
				 this._port = port;
			 }

			 public AdvertisedSocketAddress primary()
			 {
				  return From( _outerInstance.catchupServer.address().Port );
			 }

			 public AdvertisedSocketAddress secondary()
			 {

				  return new AdvertisedSocketAddress( "localhost", _port );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUpstreamIssueMessage()
		 public virtual void ShouldLogUpstreamIssueMessage()
		 {
			  InMemoryStoreStreamProvider clientStoreFileStream = new InMemoryStoreStreamProvider();
			  CatchupAddressResolutionException catchupAddressResolutionException = new CatchupAddressResolutionException( new MemberId( System.Guid.randomUUID() ) );
			  try
			  {
					_subject.copyStoreFiles(new CatchupAddressProviderAnonymousInnerClass2(this, catchupAddressResolutionException)
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				  , _serverHandler.StoreId, clientStoreFileStream, Once::new, _targetLocation);
					fail();
			  }
			  catch ( StoreCopyFailedException )
			  {

					_assertableLogProvider.rawMessageMatcher().assertContains(Matchers.allOf(Matchers.containsString("Unable to resolve address for '%s'. %s")));

					// assertableLogProvider.assertLogStringContains(catchupAddressResolutionException.getMessage() );
			  }
		 }

		 private class CatchupAddressProviderAnonymousInnerClass2 : CatchupAddressProvider
		 {
			 private readonly StoreCopyClientIT _outerInstance;

			 private CatchupAddressResolutionException _catchupAddressResolutionException;

			 public CatchupAddressProviderAnonymousInnerClass2( StoreCopyClientIT outerInstance, CatchupAddressResolutionException catchupAddressResolutionException )
			 {
				 this.outerInstance = outerInstance;
				 this._catchupAddressResolutionException = catchupAddressResolutionException;
			 }

			 public AdvertisedSocketAddress primary()
			 {
				  return From( _outerInstance.catchupServer.address().Port );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.helpers.AdvertisedSocketAddress secondary() throws org.neo4j.causalclustering.catchup.CatchupAddressResolutionException
			 public AdvertisedSocketAddress secondary()
			 {
				  throw _catchupAddressResolutionException;
			 }
		 }

		 private static AdvertisedSocketAddress From( int port )
		 {
			  return new AdvertisedSocketAddress( "localhost", port );
		 }

		 private File Relative( string filename )
		 {
			  return TestDirectory.file( filename );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String fileContent(java.io.File file) throws java.io.IOException
		 private string FileContent( File file )
		 {
			  return FileContent( file, _fsa );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static String fileContent(java.io.File file, org.neo4j.io.fs.FileSystemAbstraction fsa) throws java.io.IOException
		 internal static string FileContent( File file, FileSystemAbstraction fsa )
		 {
			  int chunkSize = 128;
			  StringBuilder stringBuilder = new StringBuilder();
			  using ( Reader reader = fsa.OpenAsReader( file, Charsets.UTF_8 ) )
			  {
					CharBuffer charBuffer = CharBuffer.wrap( new char[chunkSize] );
					while ( reader.read( charBuffer ) != -1 )
					{
						 charBuffer.flip();
						 stringBuilder.Append( charBuffer );
						 charBuffer.clear();
					}
			  }
			  return stringBuilder.ToString();
		 }

		 private static string ClientFileContents( InMemoryStoreStreamProvider storeFileStreamsProvider, string filename )
		 {
			  return storeFileStreamsProvider.FileStreams()[filename].ToString();
		 }

		 private class Once : TerminationCondition
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void assertContinue() throws StoreCopyFailedException
			  public override void AssertContinue()
			  {
					throw new StoreCopyFailedException( "One try only" );
			  }
		 }
	}

}