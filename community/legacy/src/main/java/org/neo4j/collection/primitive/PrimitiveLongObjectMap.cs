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
namespace Org.Neo4j.Collection.primitive
{

	public interface PrimitiveLongObjectMap<VALUE> : PrimitiveLongCollection
	{
		 VALUE Put( long key, VALUE value );

		 bool ContainsKey( long key );

		 VALUE Get( long key );

		 VALUE Remove( long key );

		 /// <summary>
		 /// Visit the entries of this map, until all have been visited or the visitor returns 'true'.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <E extends Exception> void visitEntries(PrimitiveLongObjectVisitor<VALUE, E> visitor) throws E;
		 void visitEntries<E>( PrimitiveLongObjectVisitor<VALUE, E> visitor );

		 /// <summary>
		 /// <seealso cref="System.Collections.IEnumerable"/> with all map values
		 /// </summary>
		 /// <returns> iterable with all map values </returns>
		 IEnumerable<VALUE> Values();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default VALUE computeIfAbsent(long key, System.Func<long, VALUE> function)
	//	 {
	//		  VALUE value = get(key);
	//		  if (value == null)
	//		  {
	//				value = function.apply(key);
	//				put(key, value);
	//		  }
	//		  return value;
	//	 }
	}

}