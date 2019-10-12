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
namespace Org.Neo4j.Kernel.impl.proc
{
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using Context = Org.Neo4j.Kernel.api.proc.Context;
	using ProcedureTransaction = Org.Neo4j.Procedure.ProcedureTransaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Context_Fields.KERNEL_TRANSACTION;

	public class ProcedureTransactionProvider : ComponentRegistry.Provider<ProcedureTransaction>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.procedure.ProcedureTransaction apply(org.neo4j.kernel.api.proc.Context ctx) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override ProcedureTransaction Apply( Context ctx )
		 {
			  KernelTransaction ktx = ctx.Get( KERNEL_TRANSACTION );
			  return new ProcedureTransactionImpl( ktx );
		 }

		 private class ProcedureTransactionImpl : ProcedureTransaction
		 {
			  internal readonly KernelTransaction Ktx;

			  internal ProcedureTransactionImpl( KernelTransaction ktx )
			  {
					this.Ktx = ktx;
			  }

			  public override void Terminate()
			  {
					Ktx.markForTermination( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Terminated );
			  }

			  public override void Failure()
			  {
					Ktx.failure();
			  }
		 }
	}

}