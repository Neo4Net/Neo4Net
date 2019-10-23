using System;
using System.Collections.Generic;
using System.Reflection;

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
namespace Neo4Net.Kernel.impl.proc
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Neo4NetTypes = Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes;
	using TypeChecker = Neo4Net.Kernel.impl.proc.TypeMappers.TypeChecker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTAny;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTFloat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTInteger;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTNumber;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTString;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class TypeMappersTest
	public class TypeMappersTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public Type javaClass;
		 public Type JavaClass;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.AnyType neoType;
		 public Neo4NetTypes.AnyType NeoType;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public Object javaValue;
		 public object JavaValue;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(3) public Object expectedNeoValue;
		 public object ExpectedNeoValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0} to {1}") public static java.util.List<Object[]> conversions()
		 public static IList<object[]> Conversions()
		 {
			  return new IList<object[]>
			  {
				  new object[]{ typeof( object ), NTAny, "", "" },
				  new object[]{ typeof( object ), NTAny, null, null },
				  new object[]{ typeof( object ), NTAny, 1, 1 },
				  new object[]{ typeof( object ), NTAny, true, true },
				  new object[]{ typeof( object ), NTAny, asList( 1, 2, 3 ), asList( 1, 2, 3 ) },
				  new object[]{ typeof( object ), NTAny, new Dictionary<>(), new Dictionary<>() },
				  new object[]{ typeof( string ), NTString, "", "" },
				  new object[]{ typeof( string ), NTString, "not empty", "not empty" },
				  new object[]{ typeof( string ), NTString, null, null },
				  new object[]{ typeof( System.Collections.IDictionary ), NTMap, new Dictionary<>(), new Dictionary<>() },
				  new object[]{ typeof( System.Collections.IDictionary ), NTMap, KMap, KMap },
				  new object[]{ typeof( System.Collections.IDictionary ), NTMap, null, null },
				  new object[]{ typeof( System.Collections.IList ), NTList( NTAny ), emptyList(), emptyList() },
				  new object[]{ typeof( System.Collections.IList ), NTList( NTAny ), asList( 1, 2, 3, 4 ), asList( 1, 2, 3, 4 ) },
				  new object[]{ typeof( System.Collections.IList ), NTList( NTAny ), asList( asList( 1, 2 ), asList( "three", "four" ) ), asList( asList( 1, 2 ), asList( "three", "four" ) ) },
				  new object[]{ typeof( System.Collections.IList ), NTList( NTAny ), null, null },
				  new object[]{ ListOfListOfMap, NTList( NTList( NTMap ) ), asList(), asList() },
				  new object[]{ typeof( bool ), NTBoolean, false, false },
				  new object[]{ typeof( bool ), NTBoolean, true, true },
				  new object[]{ typeof( bool ), NTBoolean, null, null },
				  new object[]{ typeof( Boolean ), NTBoolean, false, false },
				  new object[]{ typeof( Boolean ), NTBoolean, true, true },
				  new object[]{ typeof( Boolean ), NTBoolean, null, null },
				  new object[]{ typeof( Number ), NTNumber, 1L, 1L },
				  new object[]{ typeof( Number ), NTNumber, 0L, 0L },
				  new object[]{ typeof( Number ), NTNumber, null, null },
				  new object[]{ typeof( Number ), NTNumber, long.MinValue, long.MinValue },
				  new object[]{ typeof( Number ), NTNumber, long.MaxValue, long.MaxValue },
				  new object[]{ typeof( Number ), NTNumber, 1D, 1D },
				  new object[]{ typeof( Number ), NTNumber, 0D, 0D },
				  new object[]{ typeof( Number ), NTNumber, 1.234D, 1.234D },
				  new object[]{ typeof( Number ), NTNumber, null, null },
				  new object[]{ typeof( Number ), NTNumber, double.Epsilon, double.Epsilon },
				  new object[]{ typeof( Number ), NTNumber, double.MaxValue, double.MaxValue },
				  new object[]{ typeof( long ), NTInteger, 1L, 1L },
				  new object[]{ typeof( long ), NTInteger, 0L, 0L },
				  new object[]{ typeof( long ), NTInteger, null, null },
				  new object[]{ typeof( long ), NTInteger, long.MinValue, long.MinValue },
				  new object[]{ typeof( long ), NTInteger, long.MaxValue, long.MaxValue },
				  new object[]{ typeof( Long ), NTInteger, 1L, 1L },
				  new object[]{ typeof( Long ), NTInteger, 0L, 0L },
				  new object[]{ typeof( Long ), NTInteger, null, null },
				  new object[]{ typeof( Long ), NTInteger, long.MinValue, long.MinValue },
				  new object[]{ typeof( Long ), NTInteger, long.MaxValue, long.MaxValue },
				  new object[]{ typeof( double ), NTFloat, 1D, 1D },
				  new object[]{ typeof( double ), NTFloat, 0D, 0D },
				  new object[]{ typeof( double ), NTFloat, 1.234D, 1.234D },
				  new object[]{ typeof( double ), NTFloat, null, null },
				  new object[]{ typeof( double ), NTFloat, double.Epsilon, double.Epsilon },
				  new object[]{ typeof( double ), NTFloat, double.MaxValue, double.MaxValue },
				  new object[]{ typeof( Double ), NTFloat, 1D, 1D },
				  new object[]{ typeof( Double ), NTFloat, 0D, 0D },
				  new object[]{ typeof( Double ), NTFloat, 1.234D, 1.234D },
				  new object[]{ typeof( Double ), NTFloat, null, null },
				  new object[]{ typeof( Double ), NTFloat, double.Epsilon, double.Epsilon },
				  new object[]{ typeof( Double ), NTFloat, double.MaxValue, double.MaxValue }
			  };
		 }

		 private static Dictionary<string, object> KMap
		 {
			 get
			 {
				  return new HashMapAnonymousInnerClass();
			 }
		 }

		 private class HashMapAnonymousInnerClass : Dictionary<string, object>
		 {
	//		 {
	//		  put("k", 1);
	//	 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectCorrectType() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectCorrectType()
		 {
			  // When
			  Neo4NetTypes.AnyType type = ( new TypeMappers() ).ToNeo4NetType(JavaClass);

			  // Then
			  assertEquals( NeoType, type );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMapCorrectly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMapCorrectly()
		 {
			  // Given
			  TypeChecker mapper = ( new TypeMappers() ).CheckerFor(JavaClass);

			  // When
			  object converted = mapper.TypeCheck( JavaValue );

			  // Then
			  assertEquals( ExpectedNeoValue, converted );
		 }

		 internal static Type ListOfListOfMap = TypeOf( "listOfListOfMap" );

		 internal interface ClassToGetGenericTypeSignatures
		 {
			  void ListOfListOfMap( IList<IList<IDictionary<string, object>>> arg );
		 }

		 internal static Type TypeOf( string methodName )
		 {
			  try
			  {
					foreach ( System.Reflection.MethodInfo method in typeof( ClassToGetGenericTypeSignatures ).GetMethods( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance ) )
					{
						 if ( method.Name.Equals( methodName ) )
						 {
							  return method.GenericParameterTypes[0];
						 }
					}
					throw new AssertionError( "No method named " + methodName );
			  }
			  catch ( Exception e )
			  {
					throw new AssertionError( e );
			  }
		 }
	}

}