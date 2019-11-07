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
namespace Neo4Net.Kernel.Api.Procs
{
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using UserFunctionSignature = Neo4Net.Kernel.Api.Internal.procs.UserFunctionSignature;
	using AnyValue = Neo4Net.Values.AnyValue;

	public interface ICallableUserFunction
	{
		 UserFunctionSignature Signature();
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4Net.values.AnyValue apply(Context ctx, Neo4Net.values.AnyValue[] input) throws Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
		 AnyValue Apply( Context ctx, AnyValue[] input );
	}

	 public abstract class CallableUserFunction_BasicUserFunction : CallableUserFunction
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly UserFunctionSignature SignatureConflict;

		  protected internal CallableUserFunction_BasicUserFunction( UserFunctionSignature signature )
		  {
				this.SignatureConflict = signature;
		  }

		  public override UserFunctionSignature Signature()
		  {
				return SignatureConflict;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract Neo4Net.values.AnyValue apply(Context ctx, Neo4Net.values.AnyValue[] input) throws Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
		  public override abstract AnyValue Apply( Context ctx, AnyValue[] input );
	 }

}