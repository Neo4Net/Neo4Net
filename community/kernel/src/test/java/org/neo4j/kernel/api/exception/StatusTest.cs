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
namespace Org.Neo4j.Kernel.api.exception
{
	using Test = org.junit.Test;


	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;

	public class StatusTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eachStatusCodeHasAUniqueCode()
		 public virtual void EachStatusCodeHasAUniqueCode()
		 {
			  // given
			  ISet<object> codes = new HashSet<object>();

			  // when
			  foreach ( Status status in Org.Neo4j.Kernel.Api.Exceptions.Status_Code.All() )
			  {
					codes.Add( status.Code().serialize() );
			  }

			  // then
			  assertNotEquals( 0, codes.Count );
			  assertEquals( Org.Neo4j.Kernel.Api.Exceptions.Status_Code.All().Count, codes.Count );
		 }
	}

}