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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using Neo4Net.Helpers.Collection;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using PullingProducerStep = Neo4Net.@unsafe.Impl.Batchimport.staging.PullingProducerStep;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.prefetching;

	/// <summary>
	/// Reads <seealso cref="RelationshipGroupRecord group records"/> from <seealso cref="RelationshipGroupCache"/>, sending
	/// them downstream in batches.
	/// </summary>
	public class ReadGroupsFromCacheStep : PullingProducerStep
	{
		 private readonly int _itemSize;
		 private readonly PrefetchingIterator<RelationshipGroupRecord> _data;
		 private RelationshipGroupRecord[] _scratch;
		 private int _cursor;

		 public ReadGroupsFromCacheStep( StageControl control, Configuration config, IEnumerator<RelationshipGroupRecord> groups, int itemSize ) : base( control, config )
		 {
			  this._data = prefetching( groups );
			  this._itemSize = itemSize;
			  this._scratch = new RelationshipGroupRecord[config.BatchSize() * 2]; // grows on demand
		 }

		 protected internal override object NextBatchOrNull( long ticket, int batchSize )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( !_data.hasNext() )
			  {
					return null;
			  }

			  int i = 0;
			  long lastOwner = -1;
			  for ( ; _data.MoveNext(); i++ )
			  {
					// Logic below makes it so that all groups for a specific node ends up in the same batch,
					// which means that batches are slightly bigger (varying) than the requested size.
					RelationshipGroupRecord item = _data.peek();
					if ( i == batchSize - 1 )
					{
						 // Remember which owner this "last" group has...
						 lastOwner = item.OwningNode;
					}
					else if ( i >= batchSize )
					{
						 // ...and continue including groups in this batch until next owner comes
						 if ( item.OwningNode != lastOwner )
						 {
							  break;
						 }
					}

					if ( i >= _scratch.Length )
					{
						 _scratch = Arrays.copyOf( _scratch, _scratch.Length * 2 );
					}
					_scratch[i] = _data.Current; // which is "item", but also advances the iterator
					_cursor++;
			  }
			  return Arrays.copyOf( _scratch, i );
		 }

		 protected internal override long Position()
		 {
			  return _cursor * _itemSize;
		 }
	}

}