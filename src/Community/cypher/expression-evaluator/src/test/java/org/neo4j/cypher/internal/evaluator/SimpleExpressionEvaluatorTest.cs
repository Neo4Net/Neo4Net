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
namespace Neo4Net.Cypher.@internal.evaluator
{
	using Test = org.junit.jupiter.api.Test;

	using MapUtil = Neo4Net.Helpers.Collection.MapUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class SimpleExpressionEvaluatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConvertToSpecificType() throws EvaluationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldConvertToSpecificType()
		 {
			  // Given
			  ExpressionEvaluator evaluator = Evaluator.ExpressionEvaluator();

			  // When
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<?> list = evaluator.evaluate("[1, 2, 3]", java.util.List.class);
			  IList<object> list = evaluator.Evaluate( "[1, 2, 3]", typeof( System.Collections.IList ) );

			  // Then
			  assertEquals( asList( 1L, 2L, 3L ), list );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConvertToObject() throws EvaluationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldConvertToObject()
		 {
			  // Given
			  ExpressionEvaluator evaluator = Evaluator.ExpressionEvaluator();

			  // When
			  object @object = evaluator.Evaluate( "{prop: 42}", typeof( object ) );

			  // Then
			  assertEquals( MapUtil.map( "prop", 42L ), @object );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowIfWrongType()
		 internal virtual void ShouldThrowIfWrongType()
		 {
			  // Given
			  ExpressionEvaluator evaluator = Evaluator.ExpressionEvaluator();

			  // Expect
			  assertThrows( typeof( EvaluationException ), () => evaluator.Evaluate("{prop: 42}", typeof(System.Collections.IList)) );
		 }
	}

}