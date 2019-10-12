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
	public interface ExpressionVisitor
	{
		 void Invoke( Expression target, MethodReference method, Expression[] arguments );

		 void Invoke( MethodReference method, Expression[] arguments );

		 void Load( LocalVariable variable );

		 void GetField( Expression target, FieldReference field );

		 void Constant( object value );

		 void GetStatic( FieldReference field );

		 void LoadThis( string sourceName );

		 void NewInstance( TypeReference type );

		 void Not( Expression expression );

		 void Ternary( Expression test, Expression onTrue, Expression onFalse );

		 void Equal( Expression lhs, Expression rhs );

		 void NotEqual( Expression lhs, Expression rhs );

		 void IsNull( Expression expression );

		 void NotNull( Expression expression );

		 void Or( params Expression[] expressions );

		 void And( params Expression[] expressions );

		 void Add( Expression lhs, Expression rhs );

		 void Gt( Expression lhs, Expression rhs );

		 void Gte( Expression lhs, Expression rhs );

		 void Lt( Expression lhs, Expression rhs );

		 void Lte( Expression lhs, Expression rhs );

		 void Subtract( Expression lhs, Expression rhs );

		 void Multiply( Expression lhs, Expression rhs );

		 void Cast( TypeReference type, Expression expression );

		 void InstanceOf( TypeReference type, Expression expression );

		 void NewArray( TypeReference type, params Expression[] constants );

		 void LongToDouble( Expression expression );

		 void Pop( Expression expression );

		 void Box( Expression expression );

		 void Unbox( Expression expression );
	}

}