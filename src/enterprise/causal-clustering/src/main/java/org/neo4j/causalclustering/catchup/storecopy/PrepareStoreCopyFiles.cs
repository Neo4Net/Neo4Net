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
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;


	using Neo4Net.GraphDb;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using StoreFileMetadata = Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileUtils.relativePath;

	public class PrepareStoreCopyFiles : IDisposable
	{
		 private readonly NeoStoreDataSource _neoStoreDataSource;
		 private readonly FileSystemAbstraction _fileSystemAbstraction;
		 private readonly CloseablesListener _closeablesListener = new CloseablesListener();

		 internal PrepareStoreCopyFiles( NeoStoreDataSource neoStoreDataSource, FileSystemAbstraction fileSystemAbstraction )
		 {
			  this._neoStoreDataSource = neoStoreDataSource;
			  this._fileSystemAbstraction = fileSystemAbstraction;
		 }

		 internal virtual LongSet NonAtomicIndexIds
		 {
			 get
			 {
				  return LongSets.immutable.empty();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: StoreResource[] getAtomicFilesSnapshot() throws java.io.IOException
		 internal virtual StoreResource[] AtomicFilesSnapshot
		 {
			 get
			 {
				  ResourceIterator<StoreFileMetadata> neoStoreFilesIterator = _closeablesListener.add( _neoStoreDataSource.NeoStoreFileListing.builder().excludeAll().includeNeoStoreFiles().build() );
				  ResourceIterator<StoreFileMetadata> indexIterator = _closeablesListener.add( _neoStoreDataSource.NeoStoreFileListing.builder().excludeAll().includeExplicitIndexStoreStoreFiles().includeAdditionalProviders().includeLabelScanStoreFiles().includeSchemaIndexStoreFiles().build() );
   
	//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				  return Stream.concat( neoStoreFilesIterator.Where( IsCountFile( _neoStoreDataSource.DatabaseLayout ) ), indexIterator.Stream() ).map(MapToStoreResource()).toArray(StoreResource[]::new);
			 }
		 }

		 private System.Func<StoreFileMetadata, StoreResource> MapToStoreResource()
		 {
			  return storeFileMetadata =>
			  {
				try
				{
					 return ToStoreResource( storeFileMetadata );
				}
				catch ( IOException e )
				{
					 throw new System.InvalidOperationException( "Unable to create store resource", e );
				}
			  };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.io.File[] listReplayableFiles() throws java.io.IOException
		 internal virtual File[] ListReplayableFiles()
		 {
			  using ( Stream<StoreFileMetadata> stream = _neoStoreDataSource.NeoStoreFileListing.builder().excludeLogFiles().excludeExplicitIndexStoreFiles().excludeSchemaIndexStoreFiles().excludeAdditionalProviders().build().stream() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					return stream.filter( IsCountFile( _neoStoreDataSource.DatabaseLayout ).negate() ).map(StoreFileMetadata::file).toArray(File[]::new);
			  }
		 }

		 private static System.Predicate<StoreFileMetadata> IsCountFile( DatabaseLayout databaseLayout )
		 {
			  return storeFileMetadata => databaseLayout.CountStoreB().Equals(storeFileMetadata.file()) || databaseLayout.CountStoreA().Equals(storeFileMetadata.file());
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private StoreResource toStoreResource(org.Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata storeFileMetadata) throws java.io.IOException
		 private StoreResource ToStoreResource( StoreFileMetadata storeFileMetadata )
		 {
			  File databaseDirectory = _neoStoreDataSource.DatabaseLayout.databaseDirectory();
			  File file = storeFileMetadata.File();
			  string relativePath = relativePath( databaseDirectory, file );
			  return new StoreResource( file, relativePath, storeFileMetadata.RecordSize(), _fileSystemAbstraction );
		 }

		 public override void Close()
		 {
			  _closeablesListener.close();
		 }
	}

}