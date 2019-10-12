using System;
using System.Collections.Generic;

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
namespace Neo4Net.Consistency.checking.index
{
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;


	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexProviderMap = Neo4Net.Kernel.Impl.Api.index.IndexProviderMap;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using Neo4Net.Kernel.impl.store;
	using SchemaStorage = Neo4Net.Kernel.impl.store.SchemaStorage;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

	public class IndexAccessors : System.IDisposable
	{
		 private readonly MutableLongObjectMap<IndexAccessor> _accessors = new LongObjectHashMap<IndexAccessor>();
		 private readonly IList<StoreIndexDescriptor> _onlineIndexRules = new List<StoreIndexDescriptor>();
		 private readonly IList<StoreIndexDescriptor> _notOnlineIndexRules = new List<StoreIndexDescriptor>();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public IndexAccessors(org.neo4j.kernel.impl.api.index.IndexProviderMap providers, org.neo4j.kernel.impl.store.RecordStore<org.neo4j.kernel.impl.store.record.DynamicRecord> schemaStore, org.neo4j.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig) throws java.io.IOException
		 public IndexAccessors( IndexProviderMap providers, RecordStore<DynamicRecord> schemaStore, IndexSamplingConfig samplingConfig )
		 {
			  IEnumerator<StoreIndexDescriptor> indexes = ( new SchemaStorage( schemaStore ) ).indexesGetAll();
			  for ( ; ; )
			  {
					try
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( indexes.hasNext() )
						 {
							  // we intentionally only check indexes that are online since
							  // - populating indexes will be rebuilt on next startup
							  // - failed indexes have to be dropped by the user anyways
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  StoreIndexDescriptor indexDescriptor = indexes.next();
							  if ( indexDescriptor.IndexWithoutOwningConstraint )
							  {
									_notOnlineIndexRules.Add( indexDescriptor );
							  }
							  else
							  {
									if ( InternalIndexState.ONLINE == Provider( providers, indexDescriptor ).getInitialState( indexDescriptor ) )
									{
										 _onlineIndexRules.Add( indexDescriptor );
									}
									else
									{
										 _notOnlineIndexRules.Add( indexDescriptor );
									}
							  }
						 }
						 else
						 {
							  break;
						 }
					}
					catch ( Exception )
					{
						 // ignore; inconsistencies of the schema store are specifically handled elsewhere.
					}
			  }

			  foreach ( StoreIndexDescriptor indexRule in _onlineIndexRules )
			  {
					long indexId = indexRule.Id;
					_accessors.put( indexId, Provider( providers, indexRule ).getOnlineAccessor( indexRule, samplingConfig ) );
			  }
		 }

		 private IndexProvider Provider( IndexProviderMap providers, StoreIndexDescriptor indexRule )
		 {
			  return providers.Lookup( indexRule.ProviderDescriptor() );
		 }

		 public virtual ICollection<StoreIndexDescriptor> NotOnlineRules()
		 {
			  return _notOnlineIndexRules;
		 }

		 public virtual IndexAccessor AccessorFor( StoreIndexDescriptor indexRule )
		 {
			  return _accessors.get( indexRule.Id );
		 }

		 public virtual IEnumerable<StoreIndexDescriptor> OnlineRules()
		 {
			  return _onlineIndexRules;
		 }

		 public virtual void Remove( StoreIndexDescriptor descriptor )
		 {
			  IndexAccessor remove = _accessors.remove( descriptor.Id );
			  if ( remove != null )
			  {
					remove.Dispose();
			  }
			  _onlineIndexRules.Remove( descriptor );
			  _notOnlineIndexRules.Remove( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  foreach ( IndexAccessor accessor in _accessors )
			  {
					accessor.Dispose();
			  }
			  _accessors.clear();
			  _onlineIndexRules.Clear();
			  _notOnlineIndexRules.Clear();
		 }
	}

}