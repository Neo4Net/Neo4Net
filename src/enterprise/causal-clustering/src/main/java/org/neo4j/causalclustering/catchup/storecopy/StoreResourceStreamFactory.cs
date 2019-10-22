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

	using Neo4Net.Cursors;
	using Neo4Net.GraphDb;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using StoreFileMetadata = Neo4Net.Storageengine.Api.StoreFileMetadata;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileUtils.relativePath;

	public class StoreResourceStreamFactory
	{
		 private readonly FileSystemAbstraction _fs;
		 private readonly System.Func<NeoStoreDataSource> _dataSourceSupplier;

		 public StoreResourceStreamFactory( FileSystemAbstraction fs, System.Func<NeoStoreDataSource> dataSourceSupplier )
		 {
			  this._fs = fs;
			  this._dataSourceSupplier = dataSourceSupplier;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.cursor.RawCursor<StoreResource,java.io.IOException> create() throws java.io.IOException
		 internal virtual IRawCursor<StoreResource, IOException> Create()
		 {
			  NeoStoreDataSource dataSource = _dataSourceSupplier.get();

			  File databaseDirectory = dataSource.DatabaseLayout.databaseDirectory();
			  ResourceIterator<StoreFileMetadata> files = dataSource.ListStoreFiles( false );

			  return new RawCursorAnonymousInnerClass( this, databaseDirectory, files );
		 }

		 private class RawCursorAnonymousInnerClass : IRawCursor<StoreResource, IOException>
		 {
			 private readonly StoreResourceStreamFactory _outerInstance;

			 private File _databaseDirectory;
			 private ResourceIterator<StoreFileMetadata> _files;

			 public RawCursorAnonymousInnerClass( StoreResourceStreamFactory outerInstance, File databaseDirectory, ResourceIterator<StoreFileMetadata> files )
			 {
				 this.outerInstance = outerInstance;
				 this._databaseDirectory = databaseDirectory;
				 this._files = files;
			 }

			 private StoreResource resource;

			 public StoreResource get()
			 {
				  return resource;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
			 public bool next()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  if ( !_files.hasNext() )
				  {
						resource = null;
						return false;
				  }

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  StoreFileMetadata md = _files.next();

				  resource = new StoreResource( md.File(), relativePath(_databaseDirectory, md.File()), md.RecordSize(), _outerInstance.fs );
				  return true;
			 }

			 public void close()
			 {
				  _files.close();
			 }
		 }
	}

}