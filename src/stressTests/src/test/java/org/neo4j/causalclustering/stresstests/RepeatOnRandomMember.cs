﻿/*
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
namespace Neo4Net.causalclustering.stresstests
{
	using Neo4Net.causalclustering.discovery;
	using Neo4Net.causalclustering.discovery;
	using Workload = Neo4Net.helper.Workload;

	internal abstract class RepeatOnRandomMember : Workload, WorkOnMember
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.Neo4Net.causalclustering.discovery.Cluster<?> cluster;
		 private readonly Cluster<object> _cluster;

		 internal RepeatOnRandomMember( Control control, Resources resources ) : base( control )
		 {
			  this._cluster = resources.Cluster();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected final void doWork() throws Exception
		 protected internal override void DoWork()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  DoWorkOnMember( _cluster.randomMember( true ).orElseThrow( System.InvalidOperationException::new ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void doWorkOnMember(org.Neo4Net.causalclustering.discovery.ClusterMember member) throws Exception;
		 public override abstract void DoWorkOnMember( ClusterMember member );
	}

}