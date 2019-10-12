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
namespace Org.Neo4j.Graphdb.factory
{

	using Org.Neo4j.Kernel.extension;
	using ClusterManager = Org.Neo4j.Kernel.impl.ha.ClusterManager;

	public class TestHighlyAvailableGraphDatabaseFactory : HighlyAvailableGraphDatabaseFactory
	{
		 protected internal override void Configure( GraphDatabaseBuilder builder )
		 {
			  base.Configure( builder );
			  builder.Config = ClusterManager.CONFIG_FOR_SINGLE_JVM_CLUSTER;
			  builder.SetConfig( GraphDatabaseSettings.StoreInternalLogLevel, "DEBUG" );
		 }

		 public virtual TestHighlyAvailableGraphDatabaseFactory AddKernelExtensions<T1>( IEnumerable<T1> newKernelExtensions )
		 {
			  CurrentState.addKernelExtensions( newKernelExtensions );
			  return this;
		 }

		 public virtual TestHighlyAvailableGraphDatabaseFactory AddKernelExtension<T1>( KernelExtensionFactory<T1> newKernelExtension )
		 {
			  return AddKernelExtensions( Collections.singletonList( newKernelExtension ) );
		 }

		 public virtual TestHighlyAvailableGraphDatabaseFactory setKernelExtensions<T1>( IEnumerable<T1> newKernelExtensions )
		 {
			  CurrentState.KernelExtensions = newKernelExtensions;
			  return this;
		 }

		 public virtual TestHighlyAvailableGraphDatabaseFactory RemoveKernelExtensions<T1>( System.Predicate<T1> toRemove )
		 {
			  CurrentState.removeKernelExtensions( toRemove );
			  return this;
		 }
	}

}