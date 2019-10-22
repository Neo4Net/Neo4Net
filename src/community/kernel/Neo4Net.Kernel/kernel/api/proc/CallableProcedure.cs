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
	using Neo4Net.Collections;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using ProcedureSignature = Neo4Net.Internal.Kernel.Api.procs.ProcedureSignature;

	public interface CallableProcedure
	{
		 ProcedureSignature Signature();
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.internal.kernel.api.exceptions.ProcedureException> apply(Context ctx, Object[] input, org.Neo4Net.kernel.api.ResourceTracker resourceTracker) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException;
		 RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker );
	}

	 public abstract class CallableProcedure_BasicProcedure : CallableProcedure
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly ProcedureSignature SignatureConflict;

		  protected internal CallableProcedure_BasicProcedure( ProcedureSignature signature )
		  {
				this.SignatureConflict = signature;
		  }

		  public override ProcedureSignature Signature()
		  {
				return SignatureConflict;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.internal.kernel.api.exceptions.ProcedureException> apply(Context ctx, Object[] input, org.Neo4Net.kernel.api.ResourceTracker resourceTracker) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException;
		  public override abstract RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker );
	 }

}