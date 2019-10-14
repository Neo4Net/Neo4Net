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
namespace Neo4Net.Kernel.impl.store.counts.keys
{
	internal abstract class IndexKey : CountsKey
	{
		public abstract void Accept( Neo4Net.Kernel.Impl.Api.CountsVisitor visitor, long first, long second );
		 private readonly long _indexId;
		 private readonly CountsKeyType _type;

		 internal IndexKey( long indexId, CountsKeyType type )
		 {
			  this._indexId = indexId;
			  this._type = type;
		 }

		 public virtual long IndexId()
		 {
			  return _indexId;
		 }

		 public override string ToString()
		 {
			  return string.Format( "IndexKey[{0}:{1:D}]", _type.name(), _indexId );
		 }

		 public override CountsKeyType RecordType()
		 {
			  return _type;
		 }

		 public override int GetHashCode()
		 {
			  return 31 * ( int ) _indexId + _type.GetHashCode();
		 }

		 public override bool Equals( object other )
		 {
			  if ( this == other )
			  {
					return true;
			  }
			  if ( other == null || this.GetType() != other.GetType() )
			  {
					return false;
			  }
			  return ( ( IndexKey ) other ).IndexId() == _indexId;
		 }

		 public override int CompareTo( CountsKey other )
		 {
			  if ( other is IndexKey )
			  {
					return Long.compare( _indexId, ( ( IndexKey ) other ).IndexId() );
			  }
			  return RecordType().compareTo(other.RecordType());
		 }
	}

}