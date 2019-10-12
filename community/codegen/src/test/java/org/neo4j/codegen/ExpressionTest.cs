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
namespace Org.Neo4j.Codegen
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Expression.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Expression.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Expression.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Expression.and;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Expression.equal;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Expression.gt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Expression.gte;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Expression.invoke;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Expression.lt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Expression.lte;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Expression.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Expression.notEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Expression.or;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.MethodReference.methodReference;

	public class ExpressionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNegateTrueToFalse()
		 public virtual void ShouldNegateTrueToFalse()
		 {
			  assertSame( FALSE, not( TRUE ) );
			  assertSame( TRUE, not( FALSE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveDoubleNegation()
		 public virtual void ShouldRemoveDoubleNegation()
		 {
			  Expression expression = invoke( methodReference( this.GetType(), typeof(bool), "TRUE" ) );
			  assertSame( expression, not( not( expression ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOptimizeNullChecks()
		 public virtual void ShouldOptimizeNullChecks()
		 {
			  // given
			  ExpressionVisitor visitor = mock( typeof( ExpressionVisitor ) );
			  Expression expression = invoke( methodReference( this.GetType(), typeof(object), "value" ) );

			  // when
			  equal( expression, NULL ).accept( visitor );

			  // then
			  verify( visitor ).isNull( expression );

			  reset( visitor ); // next

			  // when
			  equal( NULL, expression ).accept( visitor );

			  // then
			  verify( visitor ).isNull( expression );

			  reset( visitor ); // next

			  // when
			  not( equal( expression, NULL ) ).accept( visitor );

			  // then
			  verify( visitor ).notNull( expression );

			  reset( visitor ); // next

			  // when
			  not( equal( NULL, expression ) ).accept( visitor );

			  // then
			  verify( visitor ).notNull( expression );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOptimizeNegatedInequalities()
		 public virtual void ShouldOptimizeNegatedInequalities()
		 {
			  // given
			  ExpressionVisitor visitor = mock( typeof( ExpressionVisitor ) );
			  Expression expression = invoke( methodReference( this.GetType(), typeof(object), "value" ) );

			  // when
			  not( gt( expression, expression ) ).accept( visitor );

			  // then
			  verify( visitor ).lte( expression, expression );

			  reset( visitor ); // next

			  // when
			  not( gte( expression, expression ) ).accept( visitor );

			  // then
			  verify( visitor ).lt( expression, expression );

			  reset( visitor ); // next

			  // when
			  not( lt( expression, expression ) ).accept( visitor );

			  // then
			  verify( visitor ).gte( expression, expression );

			  reset( visitor ); // next

			  // when
			  not( lte( expression, expression ) ).accept( visitor );

			  // then
			  verify( visitor ).gt( expression, expression );

			  reset( visitor ); // next

			  // when
			  not( equal( expression, expression ) ).accept( visitor );

			  // then
			  verify( visitor ).notEqual( expression, expression );

			  reset( visitor ); // next

			  // when
			  not( notEqual( expression, expression ) ).accept( visitor );

			  // then
			  verify( visitor ).equal( expression, expression );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOptimizeBooleanCombinationsWithConstants()
		 public virtual void ShouldOptimizeBooleanCombinationsWithConstants()
		 {
			  // given
			  Expression expression = invoke( methodReference( this.GetType(), typeof(bool), "TRUE" ) );

			  // then
			  assertSame( expression, and( expression, TRUE ) );
			  assertSame( expression, and( TRUE, expression ) );
			  assertSame( FALSE, and( expression, FALSE ) );
			  assertSame( FALSE, and( FALSE, expression ) );

			  assertSame( expression, or( expression, FALSE ) );
			  assertSame( expression, or( FALSE, expression ) );
			  assertSame( TRUE, or( expression, TRUE ) );
			  assertSame( TRUE, or( TRUE, expression ) );
		 }

		 public static bool True()
		 {
			  return true;
		 }

		 public static object Value()
		 {
			  return null;
		 }
	}

}