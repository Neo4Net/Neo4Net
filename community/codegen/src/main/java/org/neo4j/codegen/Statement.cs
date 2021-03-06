﻿/*
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
	internal abstract class Statement
	{
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static Statement expression(final ExpressionTemplate expression)
		 internal static Statement Expression( ExpressionTemplate expression )
		 {
			  return new StatementAnonymousInnerClass( expression );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private Org.Neo4j.Codegen.ExpressionTemplate _expression;

			 public StatementAnonymousInnerClass( Org.Neo4j.Codegen.ExpressionTemplate expression )
			 {
				 this._expression = expression;
			 }

			 internal override void generate( CodeBlock method )
			 {
				  method.Expression( _expression.materialize( method ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Statement returns(final ExpressionTemplate expression)
		 public static Statement Returns( ExpressionTemplate expression )
		 {
			  return new StatementAnonymousInnerClass2( expression );
		 }

		 private class StatementAnonymousInnerClass2 : Statement
		 {
			 private Org.Neo4j.Codegen.ExpressionTemplate _expression;

			 public StatementAnonymousInnerClass2( Org.Neo4j.Codegen.ExpressionTemplate expression )
			 {
				 this._expression = expression;
			 }

			 internal override void generate( CodeBlock method )
			 {
				  method.Returns( _expression.materialize( method ) );
			 }
		 }

		 internal abstract void Generate( CodeBlock method );

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Statement put(final ExpressionTemplate target, final Lookup<FieldReference> field, final ExpressionTemplate expression)
		 public static Statement Put( ExpressionTemplate target, Lookup<FieldReference> field, ExpressionTemplate expression )
		 {
			  return new StatementAnonymousInnerClass3( target, field, expression );
		 }

		 private class StatementAnonymousInnerClass3 : Statement
		 {
			 private Org.Neo4j.Codegen.ExpressionTemplate _target;
			 private Org.Neo4j.Codegen.Lookup<FieldReference> _field;
			 private Org.Neo4j.Codegen.ExpressionTemplate _expression;

			 public StatementAnonymousInnerClass3( Org.Neo4j.Codegen.ExpressionTemplate target, Org.Neo4j.Codegen.Lookup<FieldReference> field, Org.Neo4j.Codegen.ExpressionTemplate expression )
			 {
				 this._target = target;
				 this._field = field;
				 this._expression = expression;
			 }

			 internal override void generate( CodeBlock method )
			 {
				  method.Put( _target.materialize( method ), _field.lookup( method ), _expression.materialize( method ) );
			 }
		 }
	}

}