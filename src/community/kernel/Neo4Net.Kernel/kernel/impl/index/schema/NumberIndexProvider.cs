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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using Neo4Net.Index.Internal.gbptree;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using IndexCapability = Neo4Net.Internal.Kernel.Api.IndexCapability;
	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using IndexValueCapability = Neo4Net.Internal.Kernel.Api.IndexValueCapability;
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using ValueCategory = Neo4Net.Values.Storable.ValueCategory;

	/// <summary>
	/// Schema index provider for native indexes backed by e.g. <seealso cref="GBPTree"/>.
	/// </summary>
	public class NumberIndexProvider : NativeIndexProvider<NumberIndexKey, NativeIndexValue, NumberLayout>
	{
		 public const string KEY = "native";
		 public static readonly IndexProviderDescriptor NativeProviderDescriptor = new IndexProviderDescriptor( KEY, "1.0" );
		 internal static readonly IndexCapability Capability = new NumberIndexCapability();

		 public NumberIndexProvider( PageCache pageCache, FileSystemAbstraction fs, IndexDirectoryStructure.Factory directoryStructure, Monitor monitor, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, bool readOnly ) : base( NativeProviderDescriptor, directoryStructure, pageCache, fs, monitor, recoveryCleanupWorkCollector, readOnly )
		 {
		 }

		 internal override NumberLayout Layout( StoreIndexDescriptor descriptor, File storeFile )
		 {
			  // split like this due to legacy reasons, there are old stores out there with these different identifiers
			  switch ( descriptor.Type() )
			  {
			  case GENERAL:
					return new NumberLayoutNonUnique();
			  case UNIQUE:
					return new NumberLayoutUnique();
			  default:
					throw new System.ArgumentException( "Unknown index type " + descriptor.Type() );
			  }
		 }

		 protected internal override IndexPopulator NewIndexPopulator( File storeFile, NumberLayout layout, StoreIndexDescriptor descriptor, ByteBufferFactory bufferFactory )
		 {
			  return new WorkSyncedNativeIndexPopulator<>( new NumberIndexPopulator( PageCache, Fs, storeFile, layout, Monitor, descriptor ) );
		 }

		 protected internal override IndexAccessor NewIndexAccessor( File storeFile, NumberLayout layout, StoreIndexDescriptor descriptor, bool readOnly )
		 {
			  return new NumberIndexAccessor( PageCache, Fs, storeFile, layout, RecoveryCleanupWorkCollector, Monitor, descriptor, readOnly );
		 }

		 public override IndexCapability GetCapability( StoreIndexDescriptor descriptor )
		 {
			  return Capability;
		 }

		 /// <summary>
		 /// For single property number queries capabilities are
		 /// Order: ASCENDING
		 /// Value: YES (can provide exact value)
		 /// 
		 /// For other queries there is no support
		 /// </summary>
		 private class NumberIndexCapability : IndexCapability
		 {
			  public override IndexOrder[] OrderCapability( params ValueCategory[] valueCategories )
			  {
					if ( Support( valueCategories ) )
					{
						 return Neo4Net.Internal.Kernel.Api.IndexCapability_Fields.OrderBoth;
					}
					return Neo4Net.Internal.Kernel.Api.IndexCapability_Fields.OrderNone;
			  }

			  public override IndexValueCapability ValueCapability( params ValueCategory[] valueCategories )
			  {
					if ( Support( valueCategories ) )
					{
						 return IndexValueCapability.YES;
					}
					if ( SingleWildcard( valueCategories ) )
					{
						 return IndexValueCapability.PARTIAL;
					}
					return IndexValueCapability.NO;
			  }

			  public virtual bool FulltextIndex
			  {
				  get
				  {
						return false;
				  }
			  }

			  public virtual bool EventuallyConsistent
			  {
				  get
				  {
						return false;
				  }
			  }

			  internal virtual bool Support( ValueCategory[] valueCategories )
			  {
					return valueCategories.Length == 1 && valueCategories[0] == ValueCategory.NUMBER;
			  }
		 }
	}

}