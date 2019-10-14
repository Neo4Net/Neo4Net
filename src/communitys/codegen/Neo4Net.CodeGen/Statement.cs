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
namespace Neo4Net.CodeGen
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
			 private Neo4Net.CodeGen.ExpressionTemplate _expression;

			 public StatementAnonymousInnerClass( Neo4Net.CodeGen.ExpressionTemplate expression )
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
			 private Neo4Net.CodeGen.ExpressionTemplate _expression;

			 public StatementAnonymousInnerClass2( Neo4Net.CodeGen.ExpressionTemplate expression )
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
			 private Neo4Net.CodeGen.ExpressionTemplate _target;
			 private Neo4Net.CodeGen.Lookup<FieldReference> _field;
			 private Neo4Net.CodeGen.ExpressionTemplate _expression;

			 public StatementAnonymousInnerClass3( Neo4Net.CodeGen.ExpressionTemplate target, Neo4Net.CodeGen.Lookup<FieldReference> field, Neo4Net.CodeGen.ExpressionTemplate expression )
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