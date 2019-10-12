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
namespace Neo4Net.Kernel.builtinprocs
{
	using Neo4Net.Collection;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using QualifiedName = Neo4Net.@internal.Kernel.Api.procs.QualifiedName;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using Mode = Neo4Net.Procedure.Mode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asRawIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureSignature;

	/// <summary>
	/// This procedure lists "components" and their version.
	/// While components are currently hard-coded, it is intended
	/// that this implementation will be replaced once a clean
	/// system for component assembly exists where we could dynamically
	/// get a list of which components are loaded and what versions of them.
	/// 
	/// This way, it works as a general mechanism into which capabilities
	/// a given Neo4j system has, and which version of those components
	/// are in use.
	/// 
	/// This would include things like Kernel, Storage Engine, Query Engines,
	/// Bolt protocol versions et cetera.
	/// </summary>
	public class ListComponentsProcedure : Neo4Net.Kernel.api.proc.CallableProcedure_BasicProcedure
	{
		 private readonly string _neo4jVersion;
		 private readonly string _neo4jEdition;

		 public ListComponentsProcedure( QualifiedName name, string neo4jVersion, string neo4jEdition ) : base( procedureSignature( name ).@out( "name", NTString ).@out( "versions", NTList( NTString ) ).@out( "edition", NTString ).mode( Mode.DBMS ).description( "List DBMS components and their versions." ).build() )
		 {
			  this._neo4jVersion = neo4jVersion;
			  this._neo4jEdition = neo4jEdition;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> apply(org.neo4j.kernel.api.proc.Context ctx, Object[] input, org.neo4j.kernel.api.ResourceTracker resourceTracker) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker )
		 {
			  return asRawIterator( singletonList( new object[]{ "Neo4j Kernel", singletonList( _neo4jVersion ), _neo4jEdition } ).GetEnumerator() );
		 }
	}

}