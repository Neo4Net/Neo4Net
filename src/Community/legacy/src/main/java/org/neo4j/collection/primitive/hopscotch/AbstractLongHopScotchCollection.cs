using System;

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
	using Neo4Net.Collection.primitive;

	public abstract class AbstractLongHopScotchCollection<VALUE> : AbstractHopScotchCollection<VALUE>, PrimitiveLongCollection
	{
		 public AbstractLongHopScotchCollection( Table<VALUE> table ) : base( table )
		 {
		 }

		 public override PrimitiveLongIterator Iterator()
		 {
			  return new TableKeyIterator<>( Table, this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void visitKeys(org.neo4j.collection.primitive.PrimitiveLongVisitor<E> visitor) throws E
		 public override void VisitKeys<E>( PrimitiveLongVisitor<E> visitor ) where E : Exception
		 {
			  int capacity = Table.capacity();
			  long nullKey = Table.nullKey();
			  for ( int i = 0; i < capacity; i++ )
			  {
					long key = Table.key( i );
					if ( key != nullKey && visitor.Visited( key ) )
					{
						 return;
					}
			  }
		 }
	}

}