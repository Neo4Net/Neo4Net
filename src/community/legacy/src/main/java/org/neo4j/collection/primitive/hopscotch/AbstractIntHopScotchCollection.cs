using System;

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
	using PrimitiveIntBaseIterator = Neo4Net.Collections.primitive.PrimitiveIntCollections.PrimitiveIntBaseIterator;
	using Neo4Net.Collections.primitive;

	public abstract class AbstractIntHopScotchCollection<VALUE> : AbstractHopScotchCollection<VALUE>, PrimitiveIntCollection
	{
		 public AbstractIntHopScotchCollection( Table<VALUE> table ) : base( table )
		 {
		 }

		 public override PrimitiveIntIterator Iterator()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TableKeyIterator<VALUE> longIterator = new TableKeyIterator<>(table, this);
			  TableKeyIterator<VALUE> longIterator = new TableKeyIterator<VALUE>( Table, this );
			  return new PrimitiveIntBaseIteratorAnonymousInnerClass( this, longIterator );
		 }

		 private class PrimitiveIntBaseIteratorAnonymousInnerClass : PrimitiveIntBaseIterator
		 {
			 private readonly AbstractIntHopScotchCollection<VALUE> _outerInstance;

			 private Neo4Net.Collections.primitive.hopscotch.TableKeyIterator<VALUE> _longIterator;

			 public PrimitiveIntBaseIteratorAnonymousInnerClass( AbstractIntHopScotchCollection<VALUE> outerInstance, Neo4Net.Collections.primitive.hopscotch.TableKeyIterator<VALUE> longIterator )
			 {
				 this.outerInstance = outerInstance;
				 this._longIterator = longIterator;
			 }

			 protected internal override bool fetchNext()
			 {
				  return _longIterator.hasNext() && next((int) _longIterator.next());
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void visitKeys(org.Neo4Net.collection.primitive.PrimitiveIntVisitor<E> visitor) throws E
		 public override void VisitKeys<E>( PrimitiveIntVisitor<E> visitor ) where E : Exception
		 {
			  int capacity = Table.capacity();
			  long nullKey = Table.nullKey();
			  for ( int i = 0; i < capacity; i++ )
			  {
					long key = Table.key( i );
					if ( key != nullKey && visitor.Visited( ( int ) key ) )
					{
						 return;
					}
			  }
		 }
	}

}