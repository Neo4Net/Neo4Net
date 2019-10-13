﻿/*
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
	public interface PrimitiveLongLongMap : PrimitiveLongCollection
	{
		 long Put( long key, long value );

		 bool ContainsKey( long key );

		 long Get( long key );

		 long Remove( long key );

		 /// <summary>
		 /// Visit the entries of this map, until all have been visited or the visitor returns 'true'.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <E extends Exception> void visitEntries(PrimitiveLongLongVisitor<E> visitor) throws E;
		 void visitEntries<E>( PrimitiveLongLongVisitor<E> visitor );
	}

}