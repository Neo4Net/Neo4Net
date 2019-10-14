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
	using Neo4Net.Index.@internal.gbptree;
	using AbstractIndexKeyLengthValidator = Neo4Net.Kernel.Impl.Api.AbstractIndexKeyLengthValidator;
	using Value = Neo4Net.Values.Storable.Value;

	public class NativeIndexKeyLengthValidator<KEY, VALUE> : AbstractIndexKeyLengthValidator where KEY : NativeIndexKey where VALUE : NativeIndexValue
	{
		 private readonly Layout<KEY, VALUE> _layout;

		 internal NativeIndexKeyLengthValidator( int maxByteLength, Layout<KEY, VALUE> layout ) : base( maxByteLength )
		 {
			  this._layout = layout;
		 }

		 protected internal override int IndexKeyLength( Value value )
		 {
			  KEY key = _layout.newKey();
			  key.initFromValue( 0, value, NativeIndexKey.Inclusion.Neutral );
			  return _layout.keySize( key );
		 }
	}

}