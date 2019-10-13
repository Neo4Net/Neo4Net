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
namespace Neo4Net.Kernel.impl.transaction.log
{
	using Test = org.junit.Test;


	using Neo4Net.Cursors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.alwaysTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.@in;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.not;

	public class FilteringIOCursorTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFilterWhenNothingToFilter() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFilterWhenNothingToFilter()
		 {
			  string[] strings = new string[] { "a", "b", "c" };

			  IOCursor<string> @delegate = new ArrayIOCursor<string>( strings );
			  FilteringIOCursor<string> cursor = new FilteringIOCursor<string>( @delegate, alwaysTrue() );

			  assertEquals( asList( strings ), ExtractCursorContent( cursor ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFilterFirstObject() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFilterFirstObject()
		 {
			  string[] strings = new string[] { "a", "b", "c" };

			  IOCursor<string> @delegate = new ArrayIOCursor<string>( strings );
			  FilteringIOCursor<string> cursor = new FilteringIOCursor<string>( @delegate, not( @in( "a" ) ) );

			  assertEquals( Exclude( asList( strings ), "a" ), ExtractCursorContent( cursor ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFilterMiddleObject() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFilterMiddleObject()
		 {
			  string[] strings = new string[] { "a", "b", "c" };

			  IOCursor<string> @delegate = new ArrayIOCursor<string>( strings );
			  FilteringIOCursor<string> cursor = new FilteringIOCursor<string>( @delegate, not( @in( "b" ) ) );

			  assertEquals( Exclude( asList( strings ), "b" ), ExtractCursorContent( cursor ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFilterLastObject() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFilterLastObject()
		 {
			  string[] strings = new string[] { "a", "b", "c" };

			  IOCursor<string> @delegate = new ArrayIOCursor<string>( strings );
			  FilteringIOCursor<string> cursor = new FilteringIOCursor<string>( @delegate, not( @in( "c" ) ) );

			  assertEquals( Exclude( asList( strings ), "c" ), ExtractCursorContent( cursor ) );
		 }

		 private IList<T> Exclude<T>( IList<T> list, params T[] toExclude )
		 {
			  IList<T> toReturn = new List<T>( list );

			  foreach ( T item in toExclude )
			  {
					while ( toReturn.Remove( item ) )
					{
						 // Continue
					}
			  }

			  return toReturn;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T> java.util.List<T> extractCursorContent(FilteringIOCursor<T> cursor) throws java.io.IOException
		 private IList<T> ExtractCursorContent<T>( FilteringIOCursor<T> cursor )
		 {
			  IList<T> list = new List<T>();

			  while ( cursor.Next() )
			  {
					list.Add( cursor.Get() );
			  }

			  return list;
		 }
	}

}