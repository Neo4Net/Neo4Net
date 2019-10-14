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
namespace Neo4Net.Collections.primitive
{

	public interface PrimitiveIntObjectMap<VALUE> : PrimitiveIntCollection
	{
		 VALUE Put( int key, VALUE value );

		 bool ContainsKey( int key );

		 VALUE Get( int key );

		 VALUE Remove( int key );

		 /// <summary>
		 /// Visit the entries of this map, until all have been visited or the visitor returns 'true'.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <E extends Exception> void visitEntries(PrimitiveIntObjectVisitor<VALUE, E> visitor) throws E;
		 void visitEntries<E>( PrimitiveIntObjectVisitor<VALUE, E> visitor );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default VALUE computeIfAbsent(int key, System.Func<int, VALUE> function)
	//	 {
	//		  requireNonNull(function);
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