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
namespace Neo4Net.causalclustering.cluster_load
{
	using Neo4Net.causalclustering.discovery;
	using DataCreator = Neo4Net.causalclustering.helpers.DataCreator;

	public class SmallBurst : ClusterLoad
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start(Neo4Net.causalclustering.discovery.Cluster<?> cluster) throws Exception
		 public override void Start<T1>( Cluster<T1> cluster )
		 {
			  DataCreator.createEmptyNodes( cluster, 10 );
		 }

		 public override void Stop()
		 {
			  // do nothing
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name;
		 }
	}

}