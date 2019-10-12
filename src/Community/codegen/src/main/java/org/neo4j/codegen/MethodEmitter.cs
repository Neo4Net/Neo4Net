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
namespace Neo4Net.Codegen
{

	public interface MethodEmitter
	{
		 void Done();

		 void Expression( Expression expression );

		 void Put( Expression target, FieldReference field, Expression value );

		 void Returns();

		 void Returns( Expression value );

		 void Continues();

		 void Assign( LocalVariable local, Expression value );

		 void BeginWhile( Expression test );

		 void BeginIf( Expression test );

		 void BeginBlock();

		 void EndBlock();

		 void tryCatchBlock<T>( System.Action<T> body, System.Action<T> handler, LocalVariable exception, T block );

		 void ThrowException( Expression exception );

		 void Declare( LocalVariable local );

		 void AssignVariableInScope( LocalVariable local, Expression value );
	}

}