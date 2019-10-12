using System;
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
namespace Org.Neo4j.Kernel.ha
{

	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using HighAvailabilityMemberState = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberState;
	using HighAvailabilityModeSwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using HighlyAvailableEditionModule = Org.Neo4j.Kernel.ha.factory.HighlyAvailableEditionModule;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;

	/// <summary>
	/// This has all the functionality of an Enterprise Edition embedded database, with the addition of services
	/// for handling clustering.
	/// </summary>
	public class HighlyAvailableGraphDatabase : GraphDatabaseFacade
	{

		 protected internal HighlyAvailableEditionModule Module;

		 public HighlyAvailableGraphDatabase( File storeDir, IDictionary<string, string> @params, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  NewHighlyAvailableFacadeFactory().initFacade(storeDir, @params, dependencies, this);
		 }

		 public HighlyAvailableGraphDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  NewHighlyAvailableFacadeFactory().initFacade(storeDir, config, dependencies, this);
		 }

		 // used for testing in a different project, please do not remove
		 protected internal virtual GraphDatabaseFacadeFactory NewHighlyAvailableFacadeFactory()
		 {
			  return new GraphDatabaseFacadeFactory(DatabaseInfo.HA, platformModule =>
			  {
				Module = new HighlyAvailableEditionModule( platformModule );
				return Module;
			  });
		 }

		 public virtual HighAvailabilityMemberState InstanceState
		 {
			 get
			 {
				  return Module.memberStateMachine.CurrentState;
			 }
		 }

		 public virtual string Role()
		 {
			  return Module.members.CurrentMemberRole;
		 }

		 public virtual bool Master
		 {
			 get
			 {
				  return HighAvailabilityModeSwitcher.MASTER.Equals( Role(), StringComparison.OrdinalIgnoreCase );
			 }
		 }
	}

}