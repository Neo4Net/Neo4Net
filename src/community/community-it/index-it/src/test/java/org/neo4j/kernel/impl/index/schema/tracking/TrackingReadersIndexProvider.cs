/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.Impl.Index.Schema.tracking
{

	using IndexCapability = Neo4Net.Kernel.Api.Internal.IndexCapability;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.Schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using StoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;

	public class TrackingReadersIndexProvider : IndexProvider
	{
		 private readonly IndexProvider _indexProvider;

		 internal TrackingReadersIndexProvider( IndexProvider copySource ) : base( copySource )
		 {
			  this._indexProvider = copySource;
		 }

		 public override IndexPopulator GetPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
		 {
			  return _indexProvider.getPopulator( descriptor, samplingConfig, bufferFactory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.kernel.api.index.IndexAccessor getOnlineAccessor(Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor descriptor, Neo4Net.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig) throws java.io.IOException
		 public override IndexAccessor GetOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
		 {
			  return new TrackingReadersIndexAccessor( _indexProvider.getOnlineAccessor( descriptor, samplingConfig ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getPopulationFailure(Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor descriptor) throws IllegalStateException
		 public override string GetPopulationFailure( StoreIndexDescriptor descriptor )
		 {
			  return _indexProvider.getPopulationFailure( descriptor );
		 }

		 public override InternalIndexState GetInitialState( StoreIndexDescriptor descriptor )
		 {
			  return _indexProvider.getInitialState( descriptor );
		 }

		 public override IndexCapability GetCapability( StoreIndexDescriptor descriptor )
		 {
			  return _indexProvider.getCapability( descriptor );
		 }

		 public override IndexProviderDescriptor ProviderDescriptor
		 {
			 get
			 {
				  return _indexProvider.ProviderDescriptor;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  return _indexProvider.Equals( o );
		 }

		 public override int GetHashCode()
		 {
			  return _indexProvider.GetHashCode();
		 }

		 public override IndexDirectoryStructure DirectoryStructure()
		 {
			  return _indexProvider.directoryStructure();
		 }

		 public override StoreMigrationParticipant StoreMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache )
		 {
			  return _indexProvider.storeMigrationParticipant( fs, pageCache );
		 }
	}

}