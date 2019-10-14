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
namespace Neo4Net.Collections.primitive.hopscotch
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.fill;

	public class IntKeyObjectValueTable<VALUE> : IntKeyTable<VALUE>
	{
		 private VALUE[] _values;

		 public IntKeyObjectValueTable( int capacity ) : base( capacity, null )
		 {
		 }

		 public override VALUE Value( int index )
		 {
			  return _values[index];
		 }

		 public override void Put( int index, long key, VALUE value )
		 {
			  base.Put( index, key, value );
			  _values[index] = value;
		 }

		 public override VALUE PutValue( int index, VALUE value )
		 {
			  VALUE previous = _values[index];
			  _values[index] = value;
			  return previous;
		 }

		 public override long Move( int fromIndex, int toIndex )
		 {
			  _values[toIndex] = _values[fromIndex];
			  _values[fromIndex] = default( VALUE );
			  return base.Move( fromIndex, toIndex );
		 }

		 public override VALUE Remove( int index )
		 {
			  base.Remove( index );
			  VALUE existing = _values[index];
			  _values[index] = default( VALUE );
			  return existing;
		 }

		 protected internal override IntKeyObjectValueTable<VALUE> NewInstance( int newCapacity )
		 {
			  return new IntKeyObjectValueTable<VALUE>( newCapacity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override protected void initializeTable()
		 protected internal override void InitializeTable()
		 {
			  base.InitializeTable();
			  _values = ( VALUE[] ) new object[capacity];
		 }

		 protected internal override void ClearTable()
		 {
			  base.ClearTable();
			  fill( _values, null );
		 }
	}

}