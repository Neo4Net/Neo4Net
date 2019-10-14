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
namespace Neo4Net.Kernel.api.proc
{
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using UserAggregator = Neo4Net.Internal.Kernel.Api.procs.UserAggregator;
	using UserFunctionSignature = Neo4Net.Internal.Kernel.Api.procs.UserFunctionSignature;

	public interface CallableUserAggregationFunction
	{
		 UserFunctionSignature Signature();
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.procs.UserAggregator create(Context ctx) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		 UserAggregator Create( Context ctx );
	}

	 public abstract class CallableUserAggregationFunction_BasicUserAggregationFunction : CallableUserAggregationFunction
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly UserFunctionSignature SignatureConflict;

		  protected internal CallableUserAggregationFunction_BasicUserAggregationFunction( UserFunctionSignature signature )
		  {
				this.SignatureConflict = signature;
		  }

		  public override UserFunctionSignature Signature()
		  {
				return SignatureConflict;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract org.neo4j.internal.kernel.api.procs.UserAggregator create(Context ctx) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		  public override abstract UserAggregator Create( Context ctx );
	 }

}