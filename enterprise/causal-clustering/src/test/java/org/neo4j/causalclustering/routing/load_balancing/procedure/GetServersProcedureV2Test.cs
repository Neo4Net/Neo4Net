using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.routing.load_balancing.procedure
{
	using Test = org.junit.Test;

	using FieldSignature = Org.Neo4j.@internal.Kernel.Api.procs.FieldSignature;
	using ProcedureSignature = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureSignature;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTInteger;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTMap;

	public class GetServersProcedureV2Test
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveCorrectSignature()
		 public virtual void ShouldHaveCorrectSignature()
		 {
			  // given
			  GetServersProcedureForMultiDC proc = new GetServersProcedureForMultiDC( null );

			  // when
			  ProcedureSignature signature = proc.Signature();

			  // then
			  assertThat( signature.InputSignature(), containsInAnyOrder(FieldSignature.inputField("context", NTMap)) );

			  assertThat( signature.OutputSignature(), containsInAnyOrder(FieldSignature.outputField("ttl", NTInteger), FieldSignature.outputField("servers", NTList(NTMap))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPassClientContextToPlugin() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPassClientContextToPlugin()
		 {
			  // given
			  LoadBalancingPlugin plugin = mock( typeof( LoadBalancingPlugin ) );
			  Org.Neo4j.causalclustering.routing.load_balancing.LoadBalancingProcessor_Result result = mock( typeof( LoadBalancingPlugin.Result ) );
			  when( plugin.Run( anyMap() ) ).thenReturn(result);
			  GetServersProcedureForMultiDC getServers = new GetServersProcedureForMultiDC( plugin );
			  IDictionary<string, string> clientContext = stringMap( "key", "value", "key2", "value2" );

			  // when
			  getServers.Apply( null, new object[]{ clientContext }, null );

			  // then
			  verify( plugin ).run( clientContext );
		 }
	}

}