using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Kernel.impl.factory
{

	using Service = Org.Neo4j.Helpers.Service;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using SimpleStatementLocksFactory = Org.Neo4j.Kernel.impl.locking.SimpleStatementLocksFactory;
	using StatementLocksFactory = Org.Neo4j.Kernel.impl.locking.StatementLocksFactory;
	using Log = Org.Neo4j.Logging.Log;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using VisibleForTesting = Org.Neo4j.Util.VisibleForTesting;

	public class StatementLocksFactorySelector
	{
		 private readonly Locks _locks;
		 private readonly Config _config;
		 private readonly Log _log;

		 public StatementLocksFactorySelector( Locks locks, Config config, LogService logService )
		 {
			  this._locks = locks;
			  this._config = config;
			  this._log = logService.GetInternalLog( this.GetType() );
		 }

		 public virtual StatementLocksFactory Select()
		 {
			  StatementLocksFactory statementLocksFactory;

			  string serviceName = typeof( StatementLocksFactory ).Name;
			  IList<StatementLocksFactory> factories = ServiceLoadFactories();
			  if ( factories.Count == 0 )
			  {
					statementLocksFactory = new SimpleStatementLocksFactory();

					_log.info( "No services implementing " + serviceName + " found. " + "Using " + typeof( SimpleStatementLocksFactory ).Name );
			  }
			  else if ( factories.Count == 1 )
			  {
					statementLocksFactory = factories[0];

					_log.info( "Found single implementation of " + serviceName + ". Namely " + statementLocksFactory.GetType().Name );
			  }
			  else
			  {
					throw new System.InvalidOperationException( "Found more than one implementation of " + serviceName + ": " + factories );
			  }

			  statementLocksFactory.Initialize( _locks, _config );

			  return statementLocksFactory;
		 }

		 /// <summary>
		 /// Load all available factories via <seealso cref="Service"/>.
		 /// </summary>
		 /// <returns> list of available factories. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting List<org.neo4j.kernel.impl.locking.StatementLocksFactory> serviceLoadFactories()
		 internal virtual IList<StatementLocksFactory> ServiceLoadFactories()
		 {
			  return Iterables.asList( Service.load( typeof( StatementLocksFactory ) ) );
		 }
	}

}