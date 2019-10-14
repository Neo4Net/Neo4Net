using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.routing.multi_cluster.procedure
{
	using Test = org.junit.Test;


	using FieldSignature = Neo4Net.@internal.Kernel.Api.procs.FieldSignature;
	using Neo4jTypes = Neo4Net.@internal.Kernel.Api.procs.Neo4jTypes;
	using ProcedureSignature = Neo4Net.@internal.Kernel.Api.procs.ProcedureSignature;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class MultiClusterRoutingProcedureTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void subClusterRoutingProcedureShouldHaveCorrectSignature()
		 public virtual void SubClusterRoutingProcedureShouldHaveCorrectSignature()
		 {
			  GetRoutersForDatabaseProcedure proc = new GetRoutersForDatabaseProcedure( null, Config.defaults() );

			  ProcedureSignature procSig = proc.Signature();

			  IList<FieldSignature> input = Collections.singletonList( FieldSignature.inputField( "database", Neo4jTypes.NTString ) );
			  IList<FieldSignature> output = Arrays.asList( FieldSignature.outputField( "ttl", Neo4jTypes.NTInteger ), FieldSignature.outputField( "routers", Neo4jTypes.NTList( Neo4jTypes.NTMap ) ) );

			  assertEquals( "The input signature of the GetRoutersForDatabaseProcedure should not change.", procSig.InputSignature(), input );

			  assertEquals( "The output signature of the GetRoutersForDatabaseProcedure should not change.", procSig.OutputSignature(), output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void superClusterRoutingProcedureShouldHaveCorrectSignature()
		 public virtual void SuperClusterRoutingProcedureShouldHaveCorrectSignature()
		 {
			  GetRoutersForAllDatabasesProcedure proc = new GetRoutersForAllDatabasesProcedure( null, Config.defaults() );

			  ProcedureSignature procSig = proc.Signature();

			  IList<FieldSignature> output = Arrays.asList( FieldSignature.outputField( "ttl", Neo4jTypes.NTInteger ), FieldSignature.outputField( "routers", Neo4jTypes.NTList( Neo4jTypes.NTMap ) ) );

			  assertEquals( "The output signature of the GetRoutersForAllDatabasesProcedure should not change.", procSig.OutputSignature(), output );
		 }
	}

}