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
namespace Neo4Net.Stream
{

	public class Streams
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("OptionalUsedAsFieldOrParameterType") public static <T> java.util.stream.Stream<T> ofOptional(java.util.Optional<T> opt)
		 public static Stream<T> OfOptional<T>( Optional<T> opt )
		 {
			  return opt.map( Stream.of ).orElse( Stream.empty() );
		 }

		 public static Stream<T> OfNullable<T>( T obj )
		 {
			  if ( obj == default( T ) )
			  {
					return Stream.empty();
			  }
			  else
			  {
					return Stream.of( obj );
			  }
		 }
	}

}