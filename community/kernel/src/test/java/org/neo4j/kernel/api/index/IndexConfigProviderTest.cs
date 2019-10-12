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
namespace Org.Neo4j.Kernel.Api.Index
{
	using Test = org.junit.jupiter.api.Test;


	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class IndexConfigProviderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putAllNoOverwriteMustAddToSource()
		 internal virtual void PutAllNoOverwriteMustAddToSource()
		 {
			  IDictionary<string, Value> target = new Dictionary<string, Value>();
			  IDictionary<string, Value> source = new Dictionary<string, Value>();
			  target["a"] = Values.intValue( 1 );
			  source["b"] = Values.intValue( 2 );
			  IndexConfigProvider.putAllNoOverwrite( target, source );
			  assertEquals( 2, target.Count );
			  assertEquals( Values.intValue( 1 ), target["a"] );
			  assertEquals( Values.intValue( 2 ), target["b"] );
			  assertEquals( 1, source.Count );
			  assertEquals( Values.intValue( 2 ), source["b"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putAllNoOverwriteMustThrowOnConflict()
		 internal virtual void PutAllNoOverwriteMustThrowOnConflict()
		 {
			  IDictionary<string, Value> target = new Dictionary<string, Value>();
			  IDictionary<string, Value> source = new Dictionary<string, Value>();
			  target["a"] = Values.intValue( 1 );
			  source["a"] = Values.intValue( 2 );
			  System.InvalidOperationException e = assertThrows( typeof( System.InvalidOperationException ), () => IndexConfigProvider.putAllNoOverwrite(target, source) );
			  assertEquals( "Adding config would overwrite existing value: key=a, newValue=Int(2), oldValue=Int(1)", e.Message );
		 }
	}

}