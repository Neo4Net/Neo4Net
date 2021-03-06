﻿using System.Diagnostics;

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
namespace Org.Neo4j.Kernel.Api.Index
{

	using LabelSchemaSupplier = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaSupplier;
	using SchemaDescriptorSupplier = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptorSupplier;
	using SchemaUtil = Org.Neo4j.@internal.Kernel.Api.schema.SchemaUtil;
	using UpdateMode = Org.Neo4j.Kernel.Impl.Api.index.UpdateMode;
	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// Subclasses of this represent events related to property changes due to property or label addition, deletion or
	/// update.
	/// This is of use in populating indexes that might be relevant to node label and property combinations.
	/// </summary>
	/// @param <INDEX_KEY> <seealso cref="LabelSchemaSupplier"/> specifying the schema </param>
	public class IndexEntryUpdate<INDEX_KEY> where INDEX_KEY : Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptorSupplier
	{
		 private readonly long _entityId;
		 private readonly UpdateMode _updateMode;
		 private readonly Value[] _before;
		 private readonly Value[] _values;
		 private readonly INDEX_KEY _indexKey;

		 private IndexEntryUpdate( long entityId, INDEX_KEY indexKey, UpdateMode updateMode, params Value[] values ) : this( entityId, indexKey, updateMode, null, values )
		 {
		 }

		 private IndexEntryUpdate( long entityId, INDEX_KEY indexKey, UpdateMode updateMode, Value[] before, Value[] values )
		 {
			  // we do not support partial index entries
			  Debug.Assert( indexKey.schema().PropertyIds.length == values.Length, format("IndexEntryUpdate values must be of same length as index compositeness. " + "Index on %s, but got values %s", indexKey.schema().ToString(), Arrays.ToString(values)) );
			  Debug.Assert( before == null || before.Length == values.Length );

			  this._entityId = entityId;
			  this._indexKey = indexKey;
			  this._before = before;
			  this._values = values;
			  this._updateMode = updateMode;
		 }

		 public long EntityId
		 {
			 get
			 {
				  return _entityId;
			 }
		 }

		 public virtual UpdateMode UpdateMode()
		 {
			  return _updateMode;
		 }

		 public virtual INDEX_KEY IndexKey()
		 {
			  return _indexKey;
		 }

		 public virtual Value[] Values()
		 {
			  return _values;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: IndexEntryUpdate<?> that = (IndexEntryUpdate<?>) o;
			  IndexEntryUpdate<object> that = ( IndexEntryUpdate<object> ) o;

			  if ( _entityId != that._entityId )
			  {
					return false;
			  }
			  if ( _updateMode != that._updateMode )
			  {
					return false;
			  }
			  if ( !Arrays.Equals( _before, that._before ) )
			  {
					return false;
			  }
			  if ( !Arrays.Equals( _values, that._values ) )
			  {
					return false;
			  }
			  return _indexKey != null ? _indexKey.schema().Equals(that._indexKey.schema()) : that._indexKey == null;
		 }

		 public override int GetHashCode()
		 {
			  int result = ( int )( _entityId ^ ( ( long )( ( ulong )_entityId >> 32 ) ) );
			  result = 31 * result + ( _updateMode != null ? _updateMode.GetHashCode() : 0 );
			  result = 31 * result + Arrays.GetHashCode( _before );
			  result = 31 * result + Arrays.GetHashCode( _values );
			  result = 31 * result + ( _indexKey != null ? _indexKey.schema().GetHashCode() : 0 );
			  return result;
		 }

		 public override string ToString()
		 {
			  return format( "IndexEntryUpdate[id=%d, mode=%s, %s, beforeValues=%s, values=%s]", _entityId, _updateMode, IndexKey().schema().userDescription(SchemaUtil.idTokenNameLookup), Arrays.ToString(_before), Arrays.ToString(_values) );
		 }

		 public static IndexEntryUpdate<INDEX_KEY> Add<INDEX_KEY>( long entityId, INDEX_KEY indexKey, params Value[] values ) where INDEX_KEY : Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptorSupplier
		 {
			  return new IndexEntryUpdate<INDEX_KEY>( entityId, indexKey, UpdateMode.ADDED, values );
		 }

		 public static IndexEntryUpdate<INDEX_KEY> Remove<INDEX_KEY>( long entityId, INDEX_KEY indexKey, params Value[] values ) where INDEX_KEY : Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptorSupplier
		 {
			  return new IndexEntryUpdate<INDEX_KEY>( entityId, indexKey, UpdateMode.REMOVED, values );
		 }

		 public static IndexEntryUpdate<INDEX_KEY> Change<INDEX_KEY>( long entityId, INDEX_KEY indexKey, Value before, Value after ) where INDEX_KEY : Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptorSupplier
		 {
			  return new IndexEntryUpdate<INDEX_KEY>( entityId, indexKey, UpdateMode.CHANGED, new Value[]{ before }, new Value[]{ after } );
		 }

		 public static IndexEntryUpdate<INDEX_KEY> Change<INDEX_KEY>( long entityId, INDEX_KEY indexKey, Value[] before, Value[] after ) where INDEX_KEY : Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptorSupplier
		 {
			  return new IndexEntryUpdate<INDEX_KEY>( entityId, indexKey, UpdateMode.CHANGED, before, after );
		 }

		 public virtual Value[] BeforeValues()
		 {
			  if ( _before == null )
			  {
					throw new System.NotSupportedException( "beforeValues is only valid for `UpdateMode.CHANGED" );
			  }
			  return _before;
		 }
	}

}