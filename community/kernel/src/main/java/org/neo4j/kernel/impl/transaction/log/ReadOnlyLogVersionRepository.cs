/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Kernel.impl.transaction.log
{

	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;

	public class ReadOnlyLogVersionRepository : LogVersionRepository
	{
		 private readonly long _logVersion;
		 private volatile bool _incrementVersionCalled;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ReadOnlyLogVersionRepository(org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 public ReadOnlyLogVersionRepository( PageCache pageCache, DatabaseLayout databaseLayout )
		 {
			  File neoStore = databaseLayout.MetadataStore();
			  this._logVersion = ReadLogVersion( pageCache, neoStore );
		 }

		 public virtual long CurrentLogVersion
		 {
			 get
			 {
				  // We can expect a call to this during shutting down, if we have a LogFile using us.
				  // So it's sort of OK.
				  if ( _incrementVersionCalled )
				  {
						throw new System.InvalidOperationException( "Read-only log version repository has observed a call to " + "incrementVersion, which indicates that it's been shut down" );
				  }
				  return _logVersion;
			 }
			 set
			 {
				  throw new System.NotSupportedException( "Can't set current log version in read only version repository." );
			 }
		 }


		 public override long IncrementAndGetVersion()
		 { // We can expect a call to this during shutting down, if we have a LogFile using us.
			  // So it's sort of OK.
			  if ( _incrementVersionCalled )
			  {
					throw new System.InvalidOperationException( "Read-only log version repository only allows " + "to call incrementVersion once, during shutdown" );
			  }
			  _incrementVersionCalled = true;
			  return _logVersion;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long readLogVersion(org.neo4j.io.pagecache.PageCache pageCache, java.io.File neoStore) throws java.io.IOException
		 private static long ReadLogVersion( PageCache pageCache, File neoStore )
		 {
			  try
			  {
					return MetaDataStore.getRecord( pageCache, neoStore, MetaDataStore.Position.LOG_VERSION );
			  }
			  catch ( NoSuchFileException )
			  {
					return 0;
			  }
		 }
	}

}