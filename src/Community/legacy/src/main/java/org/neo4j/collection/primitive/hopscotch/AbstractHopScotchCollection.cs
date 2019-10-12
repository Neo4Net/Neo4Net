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
namespace Neo4Net.Collection.primitive.hopscotch
{
	using Neo4Net.Collection.primitive.hopscotch.HopScotchHashingAlgorithm;

	/// <summary>
	/// Typical design of a hop scotch collection holding a table and communicating with
	/// <seealso cref="HopScotchHashingAlgorithm"/> It's a <seealso cref="ResizeMonitor"/> which will have the <seealso cref="Table"/>
	/// reassigned when it grows.
	/// </summary>
	public abstract class AbstractHopScotchCollection<VALUE> : PrimitiveCollection, ResizeMonitor<VALUE>
	{
		 protected internal Table<VALUE> Table;

		 public AbstractHopScotchCollection( Table<VALUE> table )
		 {
			  this.Table = table;
		 }

		 public override int Size()
		 {
			  return Table.size();
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				  return Table.Empty;
			 }
		 }

		 public override string ToString()
		 {
			  return Table.ToString();
		 }

		 public override void Clear()
		 {
			  Table.clear();
		 }

		 public override void TableGrew( Table<VALUE> newTable )
		 {
			  this.Table = newTable;
		 }

		 public virtual Table<VALUE> LastTable
		 {
			 get
			 {
				  return Table;
			 }
		 }

		 public override void Close()
		 {
			  Table.close();
		 }

		 public override abstract boolean ( object other );

		 public override abstract int ();

		 protected internal bool TypeAndSizeEqual( object other )
		 {
			  if ( this.GetType() == other.GetType() )
			  {
					AbstractHopScotchCollection that = ( AbstractHopScotchCollection ) other;
					if ( this.Size() == that.Size() )
					{
						 return true;
					}
			  }
			  return false;
		 }
	}

}