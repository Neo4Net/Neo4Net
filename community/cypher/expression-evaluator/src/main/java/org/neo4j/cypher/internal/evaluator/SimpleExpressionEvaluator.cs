using System;

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
namespace Org.Neo4j.Cypher.@internal.evaluator
{
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using Org.Neo4j.Values;
	using PathValue = Org.Neo4j.Values.@virtual.PathValue;
	using VirtualNodeValue = Org.Neo4j.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Org.Neo4j.Values.@virtual.VirtualRelationshipValue;

	internal class SimpleExpressionEvaluator : ExpressionEvaluator
	{
		 private InternalExpressionEvaluator _evaluator = new SimpleInternalExpressionEvaluator();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T> T evaluate(String expression, Class<T> type) throws EvaluationException
		 public override T Evaluate<T>( string expression, Type type )
		 {
				 type = typeof( T );
			  if ( string.ReferenceEquals( expression, null ) )
			  {
					throw new EvaluationException( "Cannot evaluate null as an expression " );
			  }
			  if ( type == null )
			  {
					throw new EvaluationException( "Cannot evaluate to type null" );
			  }

			  return Cast( Map( _evaluator.evaluate( expression ) ), type );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T> T cast(Object value, Class<T> type) throws EvaluationException
		 private T Cast<T>( object value, Type type )
		 {
				 type = typeof( T );
			  try
			  {
					return type.cast( value );
			  }
			  catch ( System.InvalidCastException e )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
					throw new EvaluationException( string.Format( "Expected expression of be of type `{0}` but it was `{1}`", type.FullName, value.GetType().FullName ), e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Object map(org.neo4j.values.AnyValue value) throws EvaluationException
		 private object Map( AnyValue value )
		 {
			  try
			  {
					return value.Map( MAPPER );
			  }
			  catch ( EvaluationRuntimeException e )
			  {
					throw new EvaluationException( e.Message, e );
			  }
		 }

		 private static ValueMapper<object> MAPPER = new ValueMapper_JavaMapperAnonymousInnerClass();

		 private class ValueMapper_JavaMapperAnonymousInnerClass : Org.Neo4j.Values.ValueMapper_JavaMapper
		 {
			 public override object mapPath( PathValue value )
			 {
				  throw new EvaluationRuntimeException( "Unable to evaluate paths" );
			 }

			 public override object mapNode( VirtualNodeValue value )
			 {
				  throw new EvaluationRuntimeException( "Unable to evaluate nodes" );
			 }

			 public override object mapRelationship( VirtualRelationshipValue value )
			 {
				  throw new EvaluationRuntimeException( "Unable to evaluate relationships" );
			 }
		 }
	}

}