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
namespace Org.Neo4j.Graphdb.factory
{
	using StringUtils = org.apache.commons.lang3.StringUtils;

	using CommunityEditionModule = Org.Neo4j.Graphdb.factory.module.edition.CommunityEditionModule;
	using Service = Org.Neo4j.Helpers.Service;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DynamicLocksFactory = Org.Neo4j.Kernel.impl.locking.DynamicLocksFactory;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using LocksFactory = Org.Neo4j.Kernel.impl.locking.LocksFactory;
	using ResourceTypes = Org.Neo4j.Kernel.impl.locking.ResourceTypes;
	using CommunityLocksFactory = Org.Neo4j.Kernel.impl.locking.community.CommunityLocksFactory;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

	public sealed class EditionLocksFactories
	{
		 public static Locks CreateLockManager( LocksFactory locksFactory, Config config, Clock clock )
		 {
			  return locksFactory.NewInstance( config, clock, ResourceTypes.values() );
		 }

		 public static LocksFactory CreateLockFactory( Config config, LogService logging )
		 {
			  string key = config.Get( GraphDatabaseSettings.LockManager );
			  foreach ( DynamicLocksFactory candidate in Service.load( typeof( DynamicLocksFactory ) ) )
			  {
					string candidateId = candidate.Keys.GetEnumerator().next();
					if ( key.Equals( candidateId ) )
					{
						 return candidate;
					}
					else if ( "".Equals( key ) )
					{
						 logging.GetInternalLog( typeof( CommunityEditionModule ) ).info( "No locking implementation specified, defaulting to '" + candidateId + "'" );
						 return candidate;
					}
			  }

			  if ( "community".Equals( key ) )
			  {
					return new CommunityLocksFactory();
			  }
			  else if ( StringUtils.isEmpty( key ) )
			  {
					logging.GetInternalLog( typeof( CommunityEditionModule ) ).info( "No locking implementation specified, defaulting to 'community'" );
					return new CommunityLocksFactory();
			  }

			  throw new System.ArgumentException( "No lock manager found with the name '" + key + "'." );
		 }
	}

}