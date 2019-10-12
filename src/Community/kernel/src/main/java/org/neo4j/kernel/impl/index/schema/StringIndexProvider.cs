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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using Neo4Net.Index.@internal.gbptree;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using IndexCapability = Neo4Net.@internal.Kernel.Api.IndexCapability;
	using IndexLimitation = Neo4Net.@internal.Kernel.Api.IndexLimitation;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexValueCapability = Neo4Net.@internal.Kernel.Api.IndexValueCapability;
	using IndexProviderDescriptor = Neo4Net.@internal.Kernel.Api.schema.IndexProviderDescriptor;
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
	public class StringIndexProvider : NativeIndexProvider<StringIndexKey, NativeIndexValue, StringLayout>
	{
		 public const string KEY = "string";
		 internal static readonly IndexCapability Capability = new StringIndexCapability();
		 private static readonly IndexProviderDescriptor _stringProviderDescriptor = new IndexProviderDescriptor( KEY, "1.0" );

		 public StringIndexProvider( PageCache pageCache, FileSystemAbstraction fs, IndexDirectoryStructure.Factory directoryStructure, Monitor monitor, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, bool readOnly ) : base( _stringProviderDescriptor, directoryStructure, pageCache, fs, monitor, recoveryCleanupWorkCollector, readOnly )
		 {
		 }

		 internal override StringLayout Layout( StoreIndexDescriptor descriptor, File storeFile )
		 {
			  return new StringLayout();
		 }

		 protected internal override IndexPopulator NewIndexPopulator( File storeFile, StringLayout layout, StoreIndexDescriptor descriptor, ByteBufferFactory bufferFactory )
		 {
			  return new WorkSyncedNativeIndexPopulator<>( new StringIndexPopulator( PageCache, Fs, storeFile, layout, Monitor, descriptor ) );
		 }

		 protected internal override IndexAccessor NewIndexAccessor( File storeFile, StringLayout layout, StoreIndexDescriptor descriptor, bool readOnly )
		 {
			  return new StringIndexAccessor( PageCache, Fs, storeFile, layout, RecoveryCleanupWorkCollector, Monitor, descriptor, readOnly );
		 }

		 public override IndexCapability GetCapability( StoreIndexDescriptor descriptor )
		 {
			  return Capability;
		 }

		 /// <summary>
		 /// For single property string queries capabilities are
		 /// Order: ASCENDING
		 /// Value: YES (can provide exact value)
		 /// 
		 /// For other queries there is no support
		 /// </summary>
		 private class StringIndexCapability : IndexCapability
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IndexLimitation[] LimitationsConflict = new IndexLimitation[] { IndexLimitation.SLOW_CONTAINS };

			  public override IndexOrder[] OrderCapability( params ValueCategory[] valueCategories )
			  {
					if ( Support( valueCategories ) )
					{
						 return Neo4Net.@internal.Kernel.Api.IndexCapability_Fields.OrderBoth;
					}
					return Neo4Net.@internal.Kernel.Api.IndexCapability_Fields.OrderNone;
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

			  public override IndexLimitation[] Limitations()
			  {
					return LimitationsConflict;
			  }

			  internal virtual bool Support( ValueCategory[] valueCategories )
			  {
					return valueCategories.Length == 1 && valueCategories[0] == ValueCategory.TEXT;
			  }
		 }
	}

}