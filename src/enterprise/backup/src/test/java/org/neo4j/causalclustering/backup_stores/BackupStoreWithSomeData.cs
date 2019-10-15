﻿/*
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
namespace Neo4Net.causalclustering.backup_stores
{

	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.helpers.DataCreator.createEmptyNodes;

	public class BackupStoreWithSomeData : AbstractStoreGenerator
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: CoreClusterMember createData(org.neo4j.causalclustering.discovery.Cluster<?> cluster) throws Exception
		 internal override CoreClusterMember CreateData<T1>( Cluster<T1> cluster )
		 {
			  return createEmptyNodes( cluster, 15 );
		 }

		 internal override void Modify( File backup )
		 {
			  // do nothing
		 }
	}

}