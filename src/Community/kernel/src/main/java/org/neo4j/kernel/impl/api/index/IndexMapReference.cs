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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using Neo4Net.Function;
	using IndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using IndexBackedConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.IndexBackedConstraintDescriptor;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using Value = Neo4Net.Values.Storable.Value;

	public class IndexMapReference : IndexMapSnapshotProvider
	{
		 private volatile IndexMap _indexMap = new IndexMap();

		 public override IndexMap IndexMapSnapshot()
		 {
			  return _indexMap.clone();
		 }

		 /// <summary>
		 /// Modifies the index map under synchronization. Accepts a <seealso cref="ThrowingFunction"/> which gets as input
		 /// a snapshot of the current <seealso cref="IndexMap"/>. That <seealso cref="IndexMap"/> is meant to be modified by the function
		 /// and in the end returned. The function can also return another <seealso cref="IndexMap"/> instance if it wants to, e.g.
		 /// for clearing the map. The returned map will be set as the current index map before exiting the method.
		 /// 
		 /// This is the only way contents of the <seealso cref="IndexMap"/> considered the current one can be modified.
		 /// </summary>
		 /// <param name="modifier"> the function modifying the snapshot. </param>
		 /// <exception cref="E"> exception thrown by the function. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized <E extends Exception> void modify(System.Func<IndexMap,IndexMap> modifier) throws E
		 public virtual void Modify<E>( System.Func<IndexMap, IndexMap> modifier ) where E : Exception
		 {
			 lock ( this )
			 {
				  IndexMap snapshot = IndexMapSnapshot();
				  _indexMap = modifier( snapshot );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public IndexProxy getIndexProxy(long indexId) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public virtual IndexProxy GetIndexProxy( long indexId )
		 {
			  IndexProxy proxy = _indexMap.getIndexProxy( indexId );
			  if ( proxy == null )
			  {
					throw new IndexNotFoundKernelException( "No index for index id " + indexId + " exists." );
			  }
			  return proxy;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public IndexProxy getIndexProxy(org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public virtual IndexProxy GetIndexProxy( SchemaDescriptor descriptor )
		 {
			  IndexProxy proxy = _indexMap.getIndexProxy( descriptor );
			  if ( proxy == null )
			  {
					throw new IndexNotFoundKernelException( "No index for " + descriptor + " exists." );
			  }
			  return proxy;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getIndexId(org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public virtual long GetIndexId( SchemaDescriptor descriptor )
		 {
			  IndexProxy proxy = _indexMap.getIndexProxy( descriptor );
			  if ( proxy == null )
			  {
					throw new IndexNotFoundKernelException( "No index for " + descriptor + " exists." );
			  }
			  return _indexMap.getIndexId( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getOnlineIndexId(org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public virtual long GetOnlineIndexId( SchemaDescriptor descriptor )
		 {
			  IndexProxy proxy = GetIndexProxy( descriptor );
			  switch ( proxy.State )
			  {
			  case ONLINE:
					return _indexMap.getIndexId( descriptor );

			  default:
					throw new IndexNotFoundKernelException( "Expected index on " + descriptor + " to be online." );
			  }
		 }

		 public virtual IEnumerable<IndexProxy> AllIndexProxies
		 {
			 get
			 {
				  return _indexMap.AllIndexProxies;
			 }
		 }

		 public virtual ICollection<SchemaDescriptor> GetRelatedIndexes( long[] changedEntityTokens, long[] unchangedEntityTokens, int[] sortedProperties, bool propertyListIsComplete, EntityType entityType )
		 {
			  return _indexMap.getRelatedIndexes( changedEntityTokens, unchangedEntityTokens, sortedProperties, propertyListIsComplete, entityType );
		 }

		 public virtual ICollection<IndexBackedConstraintDescriptor> GetRelatedConstraints( long[] changedLabels, long[] unchangedLabels, int[] sortedProperties, bool propertyListIsComplete, EntityType entityType )
		 {
			  return _indexMap.getRelatedConstraints( changedLabels, unchangedLabels, sortedProperties, propertyListIsComplete, entityType );
		 }

		 public virtual bool HasRelatedSchema( long[] labels, int propertyKey, EntityType entityType )
		 {
			  return _indexMap.hasRelatedSchema( labels, propertyKey, entityType );
		 }

		 public virtual bool HasRelatedSchema( int label, EntityType entityType )
		 {
			  return _indexMap.hasRelatedSchema( label, entityType );
		 }

		 public virtual IndexUpdaterMap CreateIndexUpdaterMap( IndexUpdateMode mode )
		 {
			  return new IndexUpdaterMap( _indexMap, mode );
		 }

		 public virtual void ValidateBeforeCommit( SchemaDescriptor index, Value[] tuple )
		 {
			  IndexProxy proxy = _indexMap.getIndexProxy( index );
			  if ( proxy != null )
			  {
					// Do this null-check since from the outside there's a best-effort matching going on between updates and actual indexes backing those.
					proxy.ValidateBeforeCommit( tuple );
			  }
		 }
	}

}