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
namespace Neo4Net.GraphDb.factory
{

	using Neo4Net.Kernel.extension;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;

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