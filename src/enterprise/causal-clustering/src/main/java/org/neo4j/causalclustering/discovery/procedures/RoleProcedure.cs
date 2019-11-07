/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.discovery.procedures
{
	using Neo4Net.Collections;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using Neo4NetTypes = Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes;
	using QualifiedName = Neo4Net.Kernel.Api.Internal.procs.QualifiedName;
	using ResourceTracker = Neo4Net.Kernel.Api.ResourceTracker;
	using CallableProcedure = Neo4Net.Kernel.Api.Procs.CallableProcedure;
	using Context = Neo4Net.Kernel.Api.Procs.Context;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature.procedureSignature;

	internal abstract class RoleProcedure : Neo4Net.Kernel.Api.Procs.CallableProcedure_BasicProcedure
	{
		 private const string PROCEDURE_NAME = "role";
		 private static readonly string[] _procedureNamespace = new string[] { "dbms", "cluster" };
		 private const string OUTPUT_NAME = "role";

		 internal RoleProcedure() : base(procedureSignature(new QualifiedName(_procedureNamespace, PROCEDURE_NAME)).@out(OUTPUT_NAME, Neo4NetTypes.NTString).description("The role of a specific instance in the cluster.").build())
		 {
		 }

		 public override RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker )
		 {
			  return RawIterator.of<object[], ProcedureException>( new object[]{ Role().name() } );
		 }

		 internal abstract RoleInfo Role();
	}

}