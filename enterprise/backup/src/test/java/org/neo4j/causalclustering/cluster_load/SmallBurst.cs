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
namespace Org.Neo4j.causalclustering.cluster_load
{
	using Org.Neo4j.causalclustering.discovery;
	using DataCreator = Org.Neo4j.causalclustering.helpers.DataCreator;

	public class SmallBurst : ClusterLoad
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start(org.neo4j.causalclustering.discovery.Cluster<?> cluster) throws Exception
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