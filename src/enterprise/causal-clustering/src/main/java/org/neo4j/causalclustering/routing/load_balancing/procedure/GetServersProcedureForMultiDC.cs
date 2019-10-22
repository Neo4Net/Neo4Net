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

	using Neo4Net.Collections;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using Neo4NetTypes = Neo4Net.Internal.Kernel.Api.procs.Neo4NetTypes;
	using ProcedureSignature = Neo4Net.Internal.Kernel.Api.procs.ProcedureSignature;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using Context = Neo4Net.Kernel.api.proc.Context;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.load_balancing.procedure.ParameterNames.CONTEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.load_balancing.procedure.ParameterNames.SERVERS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.load_balancing.procedure.ParameterNames.TTL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.load_balancing.procedure.ProcedureNames.GET_SERVERS_V2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.procs.ProcedureSignature.procedureSignature;

	/// <summary>
	/// Returns endpoints and their capabilities.
	/// 
	/// GetServersV2 extends upon V1 by allowing a client context consisting of
	/// key-value pairs to be supplied to and used by the concrete load
	/// balancing strategies.
	/// </summary>
	public class GetServersProcedureForMultiDC : CallableProcedure
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_procedureSignature = _procedureSignature( GET_SERVERS_V2.fullyQualifiedProcedureName() ).@in(CONTEXT.parameterName(), Neo4NetTypes.NTMap).@out(TTL.parameterName(), Neo4NetTypes.NTInteger).@out(SERVERS.parameterName(), Neo4NetTypes.NTList(Neo4NetTypes.NTMap)).description(_description).build();
		}

		 private readonly string _description = "Returns cluster endpoints and their capabilities.";

		 private ProcedureSignature _procedureSignature;

		 private readonly LoadBalancingProcessor _loadBalancingProcessor;

		 public GetServersProcedureForMultiDC( LoadBalancingProcessor loadBalancingProcessor )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._loadBalancingProcessor = loadBalancingProcessor;
		 }

		 public override ProcedureSignature Signature()
		 {
			  return _procedureSignature;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.internal.kernel.api.exceptions.ProcedureException> apply(org.Neo4Net.kernel.api.proc.Context ctx, Object[] input, org.Neo4Net.kernel.api.ResourceTracker resourceTracker) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map<String,String> clientContext = (java.util.Map<String,String>) input[0];
			  IDictionary<string, string> clientContext = ( IDictionary<string, string> ) input[0];

			  Neo4Net.causalclustering.routing.load_balancing.LoadBalancingProcessor_Result result = _loadBalancingProcessor.run( clientContext );

			  return RawIterator.of<object[], ProcedureException>( ResultFormatV1.Build( result ) );
		 }
	}

}