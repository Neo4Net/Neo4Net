using System;

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

	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class StoreCopyProcess
	{
		 private readonly FileSystemAbstraction _fs;
		 private readonly PageCache _pageCache;
		 private readonly LocalDatabase _localDatabase;
		 private readonly CopiedStoreRecovery _copiedStoreRecovery;
		 private readonly Log _log;
		 private readonly RemoteStore _remoteStore;

		 public StoreCopyProcess( FileSystemAbstraction fs, PageCache pageCache, LocalDatabase localDatabase, CopiedStoreRecovery copiedStoreRecovery, RemoteStore remoteStore, LogProvider logProvider )
		 {
			  this._fs = fs;
			  this._pageCache = pageCache;
			  this._localDatabase = localDatabase;
			  this._copiedStoreRecovery = copiedStoreRecovery;
			  this._remoteStore = remoteStore;
			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void replaceWithStoreFrom(org.neo4j.causalclustering.catchup.CatchupAddressProvider addressProvider, org.neo4j.causalclustering.identity.StoreId expectedStoreId) throws java.io.IOException, StoreCopyFailedException, DatabaseShutdownException
		 public virtual void ReplaceWithStoreFrom( CatchupAddressProvider addressProvider, StoreId expectedStoreId )
		 {
			  using ( TemporaryStoreDirectory tempStore = new TemporaryStoreDirectory( _fs, _pageCache, _localDatabase.databaseLayout().databaseDirectory() ) )
			  {
					_remoteStore.copy( addressProvider, expectedStoreId, tempStore.DatabaseLayout(), false );
					try
					{
						 _copiedStoreRecovery.recoverCopiedStore( tempStore.DatabaseLayout() );
					}
					catch ( Exception e )
					{
						 /*
						  * We keep the store until the next store copy attempt. If the exception
						  * is fatal then the store will stay around for potential forensics.
						  */
						 tempStore.KeepStore();
						 throw e;
					}
					_localDatabase.replaceWith( tempStore.DatabaseLayout().databaseDirectory() );
			  }
			  _log.info( "Replaced store successfully" );
		 }
	}

}