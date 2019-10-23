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
namespace Neo4Net.Server.web
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;

	public class HttpMethodTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLookupExistingMethodByName()
		 public virtual void ShouldLookupExistingMethodByName()
		 {
			  foreach ( HttpMethod method in HttpMethod.values() )
			  {
					assertEquals( method, HttpMethod.ValueOfOrNull( method.ToString() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLookupNonExistingMethodByName()
		 public virtual void ShouldLookupNonExistingMethodByName()
		 {
			  assertNull( HttpMethod.ValueOfOrNull( "get" ) );
			  assertNull( HttpMethod.ValueOfOrNull( "post" ) );
			  assertNull( HttpMethod.ValueOfOrNull( "PoSt" ) );
			  assertNull( HttpMethod.ValueOfOrNull( "WRONG" ) );
			  assertNull( HttpMethod.ValueOfOrNull( "" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLookupNothingByNull()
		 public virtual void ShouldLookupNothingByNull()
		 {
			  assertNull( HttpMethod.ValueOfOrNull( null ) );
		 }
	}

}