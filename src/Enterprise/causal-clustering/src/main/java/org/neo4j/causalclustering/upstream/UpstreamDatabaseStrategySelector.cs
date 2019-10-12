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
namespace Neo4Net.causalclustering.upstream
{

	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.empty;

	public class UpstreamDatabaseStrategySelector
	{
		 private LinkedHashSet<UpstreamDatabaseSelectionStrategy> _strategies = new LinkedHashSet<UpstreamDatabaseSelectionStrategy>();
		 private Log _log;

		 public UpstreamDatabaseStrategySelector( UpstreamDatabaseSelectionStrategy defaultStrategy ) : this( defaultStrategy, empty(), NullLogProvider.Instance )
		 {
		 }

		 public UpstreamDatabaseStrategySelector( UpstreamDatabaseSelectionStrategy defaultStrategy, IEnumerable<UpstreamDatabaseSelectionStrategy> otherStrategies, LogProvider logProvider )
		 {
			  this._log = logProvider.getLog( this.GetType() );

			  if ( otherStrategies != null )
			  {
					foreach ( UpstreamDatabaseSelectionStrategy otherStrategy in otherStrategies )
					{
						 _strategies.add( otherStrategy );
					}
			  }
			  _strategies.add( defaultStrategy );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.identity.MemberId bestUpstreamDatabase() throws UpstreamDatabaseSelectionException
		 public virtual MemberId BestUpstreamDatabase()
		 {
			  MemberId result = null;
			  foreach ( UpstreamDatabaseSelectionStrategy strategy in _strategies )
			  {
					_log.debug( "Trying selection strategy [%s]", strategy.ToString() );
					try
					{
						 if ( strategy.UpstreamDatabase().Present )
						 {
							  result = strategy.UpstreamDatabase().get();
							  break;
						 }
					}
					catch ( NoSuchElementException )
					{
						 // Do nothing, this strategy failed
					}
			  }

			  if ( result == null )
			  {
					throw new UpstreamDatabaseSelectionException( "Could not find an upstream database with which to connect." );
			  }

			  _log.debug( "Selected upstream database [%s]", result );
			  return result;
		 }
	}

}