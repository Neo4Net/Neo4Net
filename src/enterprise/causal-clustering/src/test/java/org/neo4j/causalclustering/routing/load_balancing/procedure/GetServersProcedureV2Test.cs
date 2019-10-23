using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.routing.load_balancing.procedure
{
	using Test = org.junit.Test;

	using FieldSignature = Neo4Net.Kernel.Api.Internal.procs.FieldSignature;
	using ProcedureSignature = Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature;

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
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTInteger;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTMap;

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
			  Neo4Net.causalclustering.routing.load_balancing.LoadBalancingProcessor_Result result = mock( typeof( LoadBalancingPlugin.Result ) );
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