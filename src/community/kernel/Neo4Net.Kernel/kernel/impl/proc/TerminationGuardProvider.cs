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
namespace Neo4Net.Kernel.impl.proc
{
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using TerminationGuard = Neo4Net.Procedure.TerminationGuard;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.proc.Context_Fields.KERNEL_TRANSACTION;

	public class TerminationGuardProvider : ComponentRegistry.Provider<TerminationGuard>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.procedure.TerminationGuard apply(org.Neo4Net.kernel.api.proc.Context ctx) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 public override TerminationGuard Apply( Context ctx )
		 {
			  KernelTransaction ktx = ctx.Get( KERNEL_TRANSACTION );
			  return new TransactionTerminationGuard( this, ktx );
		 }

		 private class TransactionTerminationGuard : TerminationGuard
		 {
			 private readonly TerminationGuardProvider _outerInstance;

			  internal readonly KernelTransaction Ktx;

			  internal TransactionTerminationGuard( TerminationGuardProvider outerInstance, KernelTransaction ktx )
			  {
				  this._outerInstance = outerInstance;
					this.Ktx = ktx;
			  }

			  public override void Check()
			  {
					Ktx.assertOpen();
			  }
		 }
	}

}