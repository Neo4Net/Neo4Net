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
namespace Org.Neo4j.Kernel.api.proc
{
	using Org.Neo4j.Collection;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using ProcedureSignature = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureSignature;

	public interface CallableProcedure
	{
		 ProcedureSignature Signature();
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> apply(Context ctx, Object[] input, org.neo4j.kernel.api.ResourceTracker resourceTracker) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
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
//ORIGINAL LINE: public abstract org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> apply(Context ctx, Object[] input, org.neo4j.kernel.api.ResourceTracker resourceTracker) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
		  public override abstract RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker );
	 }

}