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

	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using Value = Neo4Net.Values.Storable.Value;

	public class PropertyPhysicalToLogicalConverter
	{
		 private static readonly IComparer<PropertyBlock> _blockComparator = ( o1, o2 ) => Integer.compare( o1.KeyIndexId, o2.KeyIndexId );

		 private readonly PropertyStore _propertyStore;
		 private PropertyBlock[] _beforeBlocks = new PropertyBlock[8];
		 private int _beforeBlocksCursor;
		 private PropertyBlock[] _afterBlocks = new PropertyBlock[8];
		 private int _afterBlocksCursor;

		 public PropertyPhysicalToLogicalConverter( PropertyStore propertyStore )
		 {
			  this._propertyStore = propertyStore;
		 }

		 /// <summary>
		 /// Converts physical changes to PropertyRecords for a entity into logical updates
		 /// </summary>
		 public virtual void ConvertPropertyRecord<T1>( EntityCommandGrouper<T1> changes, EntityUpdates.Builder properties )
		 {
			  MapBlocks( changes );

			  int bc = 0;
			  int ac = 0;
			  while ( bc < _beforeBlocksCursor || ac < _afterBlocksCursor )
			  {
					PropertyBlock beforeBlock = null;
					PropertyBlock afterBlock = null;

					int beforeKey = int.MaxValue;
					int afterKey = int.MaxValue;
					int key;
					if ( bc < _beforeBlocksCursor )
					{
						 beforeBlock = _beforeBlocks[bc];
						 beforeKey = beforeBlock.KeyIndexId;
					}
					if ( ac < _afterBlocksCursor )
					{
						 afterBlock = _afterBlocks[ac];
						 afterKey = afterBlock.KeyIndexId;
					}

					if ( beforeKey < afterKey )
					{
						 afterBlock = null;
						 key = beforeKey;
						 bc++;
					}
					else if ( beforeKey > afterKey )
					{
						 beforeBlock = null;
						 key = afterKey;
						 ac++;
					}
					else
					{
						 // They are the same
						 key = afterKey;
						 bc++;
						 ac++;
					}

					if ( beforeBlock != null && afterBlock != null )
					{
						 // CHANGE
						 if ( !beforeBlock.HasSameContentsAs( afterBlock ) )
						 {
							  Value beforeVal = ValueOf( beforeBlock );
							  Value afterVal = ValueOf( afterBlock );
							  properties.Changed( key, beforeVal, afterVal );
						 }
					}
					else
					{
						 // ADD/REMOVE
						 if ( afterBlock != null )
						 {
							  properties.Added( key, ValueOf( afterBlock ) );
						 }
						 else
						 {
							  properties.Removed( key, ValueOf( beforeBlock ) );
						 }
					}
			  }
		 }

		 private void MapBlocks<T1>( EntityCommandGrouper<T1> changes )
		 {
			  _beforeBlocksCursor = 0;
			  _afterBlocksCursor = 0;
			  while ( true )
			  {
					Command.PropertyCommand change = changes.nextProperty();
					if ( change == null )
					{
						 break;
					}

					foreach ( PropertyBlock block in change.Before )
					{
						 if ( _beforeBlocksCursor == _beforeBlocks.Length )
						 {
							  _beforeBlocks = Arrays.copyOf( _beforeBlocks, _beforeBlocksCursor * 2 );
						 }
						 _beforeBlocks[_beforeBlocksCursor++] = block;
					}
					foreach ( PropertyBlock block in change.After )
					{
						 if ( _afterBlocksCursor == _afterBlocks.Length )
						 {
							  _afterBlocks = Arrays.copyOf( _afterBlocks, _afterBlocksCursor * 2 );
						 }
						 _afterBlocks[_afterBlocksCursor++] = block;
					}
			  }
			  Arrays.sort( _beforeBlocks, 0, _beforeBlocksCursor, _blockComparator );
			  Arrays.sort( _afterBlocks, 0, _afterBlocksCursor, _blockComparator );
		 }

		 private Value ValueOf( PropertyBlock block )
		 {
			  if ( block == null )
			  {
					return null;
			  }
			  return block.Type.value( block, _propertyStore );
		 }
	}

}