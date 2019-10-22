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
namespace Neo4Net.Kernel.builtinprocs
{
	using Neo4Net.Collections;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using QualifiedName = Neo4Net.Internal.Kernel.Api.procs.QualifiedName;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using Mode = Neo4Net.Procedure.Mode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asRawIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.procs.Neo4NetTypes.NTList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.procs.Neo4NetTypes.NTString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.procs.ProcedureSignature.procedureSignature;

	/// <summary>
	/// This procedure lists "components" and their version.
	/// While components are currently hard-coded, it is intended
	/// that this implementation will be replaced once a clean
	/// system for component assembly exists where we could dynamically
	/// get a list of which components are loaded and what versions of them.
	/// 
	/// This way, it works as a general mechanism into which capabilities
	/// a given Neo4Net system has, and which version of those components
	/// are in use.
	/// 
	/// This would include things like Kernel, Storage Engine, Query Engines,
	/// Bolt protocol versions et cetera.
	/// </summary>
	public class ListComponentsProcedure : Neo4Net.Kernel.api.proc.CallableProcedure_BasicProcedure
	{
		 private readonly string _Neo4NetVersion;
		 private readonly string _Neo4NetEdition;

		 public ListComponentsProcedure( QualifiedName name, string Neo4NetVersion, string Neo4NetEdition ) : base( procedureSignature( name ).@out( "name", NTString ).@out( "versions", NTList( NTString ) ).@out( "edition", NTString ).mode( Mode.DBMS ).description( "List DBMS components and their versions." ).build() )
		 {
			  this._Neo4NetVersion = Neo4NetVersion;
			  this._Neo4NetEdition = Neo4NetEdition;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.internal.kernel.api.exceptions.ProcedureException> apply(org.Neo4Net.kernel.api.proc.Context ctx, Object[] input, org.Neo4Net.kernel.api.ResourceTracker resourceTracker) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker )
		 {
			  return asRawIterator( singletonList( new object[]{ "Neo4Net Kernel", singletonList( _Neo4NetVersion ), _Neo4NetEdition } ).GetEnumerator() );
		 }
	}

}