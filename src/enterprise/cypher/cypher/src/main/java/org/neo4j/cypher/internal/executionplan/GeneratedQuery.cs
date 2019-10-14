/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Cypher.Internal.executionplan
{
	using Neo4Net.Cypher.Internal.compatibility.v3_5.runtime.executionplan;
	using ExecutionMode = Neo4Net.Cypher.Internal.runtime.ExecutionMode;
	using QueryContext = Neo4Net.Cypher.Internal.runtime.QueryContext;
	using InternalPlanDescription = Neo4Net.Cypher.Internal.runtime.planDescription.InternalPlanDescription;
	using QueryExecutionTracer = Neo4Net.Cypher.Internal.codegen.QueryExecutionTracer;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

	public interface GeneratedQuery
	{
		 GeneratedQueryExecution Execute( QueryContext queryContext, ExecutionMode executionMode, Provider<InternalPlanDescription> description, QueryExecutionTracer tracer, MapValue @params );
	}

}