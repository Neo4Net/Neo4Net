﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.routing.load_balancing.plugins
{

	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;

	/// <summary>
	/// Shuffles the servers of the delegate around so that every client
	/// invocation gets a a little bit of that extra entropy spice.
	/// 
	/// N.B: Lists are shuffled in place.
	/// </summary>
	public class ServerShufflingProcessor : LoadBalancingProcessor
	{
		 private readonly LoadBalancingProcessor @delegate;

		 public ServerShufflingProcessor( LoadBalancingProcessor @delegate )
		 {
			  this.@delegate = @delegate;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.routing.load_balancing.LoadBalancingProcessor_Result run(java.util.Map<String,String> context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override Org.Neo4j.causalclustering.routing.load_balancing.LoadBalancingProcessor_Result Run( IDictionary<string, string> context )
		 {
			  Org.Neo4j.causalclustering.routing.load_balancing.LoadBalancingProcessor_Result result = @delegate.Run( context );

			  Collections.shuffle( result.RouteEndpoints() );
			  Collections.shuffle( result.WriteEndpoints() );
			  Collections.shuffle( result.ReadEndpoints() );

			  return result;
		 }

		 public virtual LoadBalancingProcessor Delegate()
		 {
			  return @delegate;
		 }
	}

}