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
namespace Neo4Net.causalclustering.catchup.storecopy
{

	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using IOUtils = Neo4Net.Io.IOUtils;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using StandalonePageCacheFactory = Neo4Net.Io.pagecache.impl.muninn.StandalonePageCacheFactory;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;

	internal class SimpleCatchupClient : IDisposable
	{
		 private readonly GraphDatabaseAPI _graphDb;
		 private readonly FileSystemAbstraction _fsa;
		 private readonly CatchUpClient _catchUpClient;
		 private readonly TestCatchupServer _catchupServer;

		 private readonly AdvertisedSocketAddress _from;
		 private readonly StoreId _correctStoreId;
		 private readonly StreamToDiskProvider _streamToDiskProvider;
		 private readonly PageCache _clientPageCache;
		 private readonly IJobScheduler _jobScheduler;
		 private readonly Log _log;
		 private readonly LogProvider _logProvider;

		 internal SimpleCatchupClient( GraphDatabaseAPI graphDb, FileSystemAbstraction fileSystemAbstraction, CatchUpClient catchUpClient, TestCatchupServer catchupServer, File temporaryDirectory, LogProvider logProvider )
		 {
			  this._graphDb = graphDb;
			  this._fsa = fileSystemAbstraction;
			  this._catchUpClient = catchUpClient;
			  this._catchupServer = catchupServer;

			  _from = CatchupServerAddress;
			  _correctStoreId = GetStoreIdFromKernelStoreId( graphDb );
			  _jobScheduler = new ThreadPoolJobScheduler();
			  _clientPageCache = CreatePageCache();
			  _streamToDiskProvider = new StreamToDiskProvider( temporaryDirectory, _fsa, new Monitors() );
			  _log = logProvider.GetLog( typeof( SimpleCatchupClient ) );
			  this._logProvider = logProvider;
		 }

		 private PageCache CreatePageCache()
		 {
			  return StandalonePageCacheFactory.createPageCache( _fsa, _jobScheduler );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PrepareStoreCopyResponse requestListOfFilesFromServer() throws org.Neo4Net.causalclustering.catchup.CatchUpClientException
		 internal virtual PrepareStoreCopyResponse RequestListOfFilesFromServer()
		 {
			  return RequestListOfFilesFromServer( _correctStoreId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PrepareStoreCopyResponse requestListOfFilesFromServer(org.Neo4Net.causalclustering.identity.StoreId expectedStoreId) throws org.Neo4Net.causalclustering.catchup.CatchUpClientException
		 internal virtual PrepareStoreCopyResponse RequestListOfFilesFromServer( StoreId expectedStoreId )
		 {
			  return _catchUpClient.makeBlockingRequest( _from, new PrepareStoreCopyRequest( expectedStoreId ), StoreCopyResponseAdaptors.PrepareStoreCopyAdaptor( _streamToDiskProvider, _logProvider.getLog( typeof( SimpleCatchupClient ) ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: StoreCopyFinishedResponse requestIndividualFile(java.io.File file) throws org.Neo4Net.causalclustering.catchup.CatchUpClientException
		 internal virtual StoreCopyFinishedResponse RequestIndividualFile( File file )
		 {
			  return RequestIndividualFile( file, _correctStoreId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: StoreCopyFinishedResponse requestIndividualFile(java.io.File file, org.Neo4Net.causalclustering.identity.StoreId expectedStoreId) throws org.Neo4Net.causalclustering.catchup.CatchUpClientException
		 internal virtual StoreCopyFinishedResponse RequestIndividualFile( File file, StoreId expectedStoreId )
		 {
			  long lastTransactionId = GetCheckPointer( _graphDb ).lastCheckPointedTransactionId();
			  GetStoreFileRequest storeFileRequest = new GetStoreFileRequest( expectedStoreId, file, lastTransactionId );
			  return _catchUpClient.makeBlockingRequest( _from, storeFileRequest, StoreCopyResponseAdaptors.FilesCopyAdaptor( _streamToDiskProvider, _log ) );
		 }

		 private StoreId GetStoreIdFromKernelStoreId( GraphDatabaseAPI graphDb )
		 {
			  Neo4Net.Storageengine.Api.StoreId storeId = graphDb.StoreId();
			  return new StoreId( storeId.CreationTime, storeId.RandomId, storeId.UpgradeTime, storeId.UpgradeId );
		 }

		 private AdvertisedSocketAddress CatchupServerAddress
		 {
			 get
			 {
				  return new AdvertisedSocketAddress( "localhost", _catchupServer.address().Port );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: StoreCopyFinishedResponse requestIndexSnapshot(long indexId) throws org.Neo4Net.causalclustering.catchup.CatchUpClientException
		 internal virtual StoreCopyFinishedResponse RequestIndexSnapshot( long indexId )
		 {
			  long lastCheckPointedTransactionId = GetCheckPointer( _graphDb ).lastCheckPointedTransactionId();
			  StoreId storeId = GetStoreIdFromKernelStoreId( _graphDb );
			  GetIndexFilesRequest request = new GetIndexFilesRequest( storeId, indexId, lastCheckPointedTransactionId );
			  return _catchUpClient.makeBlockingRequest( _from, request, StoreCopyResponseAdaptors.FilesCopyAdaptor( _streamToDiskProvider, _log ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  IOUtils.closeAll( _clientPageCache, _jobScheduler );
		 }

		 private static CheckPointer GetCheckPointer( GraphDatabaseAPI graphDb )
		 {
			  return graphDb.DependencyResolver.resolveDependency( typeof( CheckPointer ) );
		 }
	}

}