/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.discovery.procedures
{
	using Neo4Net.Collection;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using Neo4jTypes = Neo4Net.@internal.Kernel.Api.procs.Neo4jTypes;
	using QualifiedName = Neo4Net.@internal.Kernel.Api.procs.QualifiedName;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using Context = Neo4Net.Kernel.api.proc.Context;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureSignature;

	internal abstract class RoleProcedure : Neo4Net.Kernel.api.proc.CallableProcedure_BasicProcedure
	{
		 private const string PROCEDURE_NAME = "role";
		 private static readonly string[] _procedureNamespace = new string[] { "dbms", "cluster" };
		 private const string OUTPUT_NAME = "role";

		 internal RoleProcedure() : base(procedureSignature(new QualifiedName(_procedureNamespace, PROCEDURE_NAME)).@out(OUTPUT_NAME, Neo4jTypes.NTString).description("The role of a specific instance in the cluster.").build())
		 {
		 }

		 public override RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker )
		 {
			  return RawIterator.of<object[], ProcedureException>( new object[]{ Role().name() } );
		 }

		 internal abstract RoleInfo Role();
	}

}