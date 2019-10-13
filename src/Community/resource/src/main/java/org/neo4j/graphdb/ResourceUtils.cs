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
namespace Neo4Net.Graphdb
{

	public class ResourceUtils
	{
		 /// <param name="resources"> <seealso cref="System.Collections.IEnumerable"/> over resources to close. </param>
		 public static void CloseAll<T>( IEnumerable<T> resources ) where T : Resource
		 {
			  CloseAll( StreamSupport.stream( resources.spliterator(), false ) );
		 }

		 /// <param name="resources"> Array of resources to close. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T extends Resource> void closeAll(T... resources)
		 public static void CloseAll<T>( params T[] resources ) where T : Resource
		 {
			  CloseAll( Arrays.stream( resources ) );
		 }

		 /// <summary>
		 /// Close all resources. Does NOT guarantee all being closed in case of unchecked exception.
		 /// </summary>
		 /// <param name="resources"> Stream of resources to close. </param>
		 public static void CloseAll<T>( Stream<T> resources ) where T : Resource
		 {
			  resources.filter( Objects.nonNull ).forEach( Resource.close );
		 }
	}

}