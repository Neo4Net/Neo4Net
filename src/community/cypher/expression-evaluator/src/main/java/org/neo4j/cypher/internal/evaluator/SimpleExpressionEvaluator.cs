using System;

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
namespace Neo4Net.Cypher.Internal.evaluator
{
	using AnyValue = Neo4Net.Values.AnyValue;
	using Neo4Net.Values;
	using PathValue = Neo4Net.Values.@virtual.PathValue;
	using VirtualNodeValue = Neo4Net.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Neo4Net.Values.@virtual.VirtualRelationshipValue;

	internal class SimpleExpressionEvaluator : ExpressionEvaluator
	{
		 private InternalExpressionEvaluator _evaluator = new SimpleInternalExpressionEvaluator();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T> T Evaluate(String expression, Class<T> type) throws EvaluationException
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

			  return Cast( Map( _evaluator.Evaluate( expression ) ), type );
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
//ORIGINAL LINE: private Object map(Neo4Net.values.AnyValue value) throws EvaluationException
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

		 private static IValueMapper<object> MAPPER = new ValueMapper_JavaMapperAnonymousInnerClass();

		 private class ValueMapper_JavaMapperAnonymousInnerClass : Neo4Net.Values.ValueMapper_JavaMapper
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