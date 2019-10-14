using System;
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
namespace Neo4Net.causalclustering.stresstests
{
	using Neo4Net.causalclustering.discovery;
	using Neo4Net.causalclustering.discovery;
	using ConsistencyCheckService = Neo4Net.Consistency.ConsistencyCheckService;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.ConsistencyCheckTool.runConsistencyCheckTool;

	/// <summary>
	/// Check the consistency of all the cluster members' stores.
	/// </summary>
	public class ConsistencyCheck : Validation
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private readonly Cluster<object> _cluster;

		 internal ConsistencyCheck( Resources resources ) : base()
		 {
			  _cluster = resources.Cluster();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void validate() throws Exception
		 protected internal override void Validate()
		 {
			  IEnumerable<ClusterMember> members = Iterables.concat( _cluster.coreMembers(), _cluster.readReplicas() );

			  foreach ( ClusterMember member in members )
			  {
					string databasePath = member.databaseDirectory().AbsolutePath;
					ConsistencyCheckService.Result result = runConsistencyCheckTool( new string[]{ databasePath }, System.out, System.err );
					if ( !result.Successful )
					{
						 throw new Exception( "Not consistent database in " + databasePath );
					}
			  }
		 }
	}

}