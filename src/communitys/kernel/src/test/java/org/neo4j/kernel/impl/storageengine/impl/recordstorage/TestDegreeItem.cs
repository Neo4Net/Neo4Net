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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	public class TestDegreeItem
	{
		 private readonly int _type;
		 private readonly long _outgoing;
		 private readonly long _incoming;

		 public TestDegreeItem( int type, long outgoing, long incoming )
		 {
			  this._type = type;
			  this._outgoing = outgoing;
			  this._incoming = incoming;
		 }

		 public virtual int Type()
		 {
			  return _type;
		 }

		 public virtual long Outgoing()
		 {
			  return _outgoing;
		 }

		 public virtual long Incoming()
		 {
			  return _incoming;
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
			  TestDegreeItem that = ( TestDegreeItem ) o;
			  return _type == that._type && _outgoing == that._outgoing && _incoming == that._incoming;
		 }

		 public override int GetHashCode()
		 {
			  return 31 * ( 31 * _type + ( int )( _outgoing ^ ( ( long )( ( ulong )_outgoing >> 32 ) ) ) ) + ( int )( _incoming ^ ( ( long )( ( ulong )_incoming >> 32 ) ) );
		 }

		 public override string ToString()
		 {
			  return "TestDegreeItem{" +
						"type=" + _type +
						", outgoing=" + _outgoing +
						", incoming=" + _incoming +
						'}';
		 }
	}

}