using System.Collections.Generic;

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
namespace Neo4Net.Index.Internal.gbptree
{

	internal class GBPTreeTestUtil
	{
		 internal static bool Contains<KEY>( IList<KEY> expectedKeys, KEY key, IComparer<KEY> comparator )
		 {
			  return expectedKeys.Select( Bind( comparator.compare, key ) ).Any( System.Predicate.isEqual( 0 ) );
		 }

		 private static System.Func<U, R> Bind<T, U, R>( System.Func<T, U, R> f, T t )
		 {
			  return u => f( t, u );
		 }
	}

}