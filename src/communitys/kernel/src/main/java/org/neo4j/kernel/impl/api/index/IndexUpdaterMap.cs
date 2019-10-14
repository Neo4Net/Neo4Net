using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using Neo4Net.Helpers.Collections;
	using Neo4Net.Helpers.Collections;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using MultipleUnderlyingStorageExceptions = Neo4Net.Kernel.impl.store.MultipleUnderlyingStorageExceptions;
	using UnderlyingStorageException = Neo4Net.Kernel.impl.store.UnderlyingStorageException;

	/// <summary>
	/// Holds currently open index updaters that are created dynamically when requested for any existing index in
	/// the given indexMap.
	/// 
	/// Also provides a close method for closing and removing any remaining open index updaters.
	/// 
	/// All updaters retrieved from this map must be either closed manually or handle duplicate calls to close
	/// or must all be closed indirectly by calling close on this updater map.
	/// </summary>
	internal class IndexUpdaterMap : AutoCloseable, IEnumerable<IndexUpdater>
	{
		 private readonly IndexUpdateMode _indexUpdateMode;
		 private readonly IndexMap _indexMap;
		 private readonly IDictionary<SchemaDescriptor, IndexUpdater> _updaterMap;

		 internal IndexUpdaterMap( IndexMap indexMap, IndexUpdateMode indexUpdateMode )
		 {
			  this._indexUpdateMode = indexUpdateMode;
			  this._indexMap = indexMap;
			  this._updaterMap = new Dictionary<SchemaDescriptor, IndexUpdater>();
		 }

		 internal virtual IndexUpdater GetUpdater( SchemaDescriptor descriptor )
		 {
			  IndexUpdater updater = _updaterMap[descriptor];
			  if ( null == updater )
			  {
					IndexProxy indexProxy = _indexMap.getIndexProxy( descriptor );
					if ( null != indexProxy )
					{
						 updater = indexProxy.NewUpdater( _indexUpdateMode );
						 _updaterMap[descriptor] = updater;
					}
			  }
			  return updater;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws org.neo4j.kernel.impl.store.UnderlyingStorageException
		 public override void Close()
		 {
			  ISet<Pair<SchemaDescriptor, UnderlyingStorageException>> exceptions = null;

			  foreach ( KeyValuePair<SchemaDescriptor, IndexUpdater> updaterEntry in _updaterMap.SetOfKeyValuePairs() )
			  {
					IndexUpdater updater = updaterEntry.Value;
					try
					{
						 updater.Close();
					}
					catch ( Exception e ) when ( e is UncheckedIOException || e is IndexEntryConflictException )
					{
						 if ( null == exceptions )
						 {
							  exceptions = new HashSet<Pair<SchemaDescriptor, UnderlyingStorageException>>();
						 }
						 exceptions.Add( Pair.of( updaterEntry.Key, new UnderlyingStorageException( e ) ) );
					}
			  }

			  Clear();

			  if ( null != exceptions )
			  {
					throw new MultipleUnderlyingStorageExceptions( exceptions );
			  }
		 }

		 public virtual void Clear()
		 {
			  _updaterMap.Clear();
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				  return _updaterMap.Count == 0;
			 }
		 }

		 public virtual int Size()
		 {
			  return _updaterMap.Count;
		 }

		 public override IEnumerator<IndexUpdater> Iterator()
		 {
			  return new PrefetchingIteratorAnonymousInnerClass( this );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<IndexUpdater>
		 {
			 private readonly IndexUpdaterMap _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass( IndexUpdaterMap outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 descriptors = outerInstance.indexMap.Descriptors();
			 }

			 private IEnumerator<SchemaDescriptor> descriptors;
			 protected internal override IndexUpdater fetchNextOrNull()
			 {
				  if ( descriptors.hasNext() )
				  {
						return outerInstance.GetUpdater( descriptors.next() );
				  }
				  return null;
			 }
		 }
	}

}