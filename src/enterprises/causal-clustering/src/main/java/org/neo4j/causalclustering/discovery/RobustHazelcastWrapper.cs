/*
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
namespace Neo4Net.causalclustering.discovery
{
	using HazelcastInstance = com.hazelcast.core.HazelcastInstance;


	/// <summary>
	/// A class which attempts to capture behaviours necessary to make interacting
	/// with hazelcast robust, e.g. reconnect on failures. This class is not aimed
	/// at high-performance and thus uses synchronization heavily.
	/// </summary>
	internal class RobustHazelcastWrapper
	{
		 private readonly HazelcastConnector _connector;
		 private HazelcastInstance _hzInstance;
		 private bool _shutdown;

		 internal RobustHazelcastWrapper( HazelcastConnector connector )
		 {
			  this._connector = connector;
		 }

		 internal virtual void Shutdown()
		 {
			 lock ( this )
			 {
				  if ( _hzInstance != null )
				  {
						_hzInstance.shutdown();
						_hzInstance = null;
						_shutdown = true;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private synchronized com.hazelcast.core.HazelcastInstance tryEnsureConnection() throws HazelcastInstanceNotActiveException
		 private HazelcastInstance TryEnsureConnection()
		 {
			 lock ( this )
			 {
				  if ( _shutdown )
				  {
						throw new HazelcastInstanceNotActiveException( "Shutdown" );
				  }
      
				  if ( _hzInstance == null )
				  {
						_hzInstance = _connector.connectToHazelcast();
				  }
				  return _hzInstance;
			 }
		 }

		 private void InvalidateConnection()
		 {
			 lock ( this )
			 {
				  _hzInstance = null;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized <T> T apply(System.Func<com.hazelcast.core.HazelcastInstance,T> function) throws HazelcastInstanceNotActiveException
		 internal virtual T Apply<T>( System.Func<HazelcastInstance, T> function )
		 {
			 lock ( this )
			 {
				  HazelcastInstance hzInstance = TryEnsureConnection();
      
				  try
				  {
						return function( hzInstance );
				  }
				  catch ( com.hazelcast.core.HazelcastInstanceNotActiveException e )
				  {
						InvalidateConnection();
						throw new HazelcastInstanceNotActiveException( e );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized void perform(System.Action<com.hazelcast.core.HazelcastInstance> operation) throws HazelcastInstanceNotActiveException
		 internal virtual void Perform( System.Action<HazelcastInstance> operation )
		 {
			 lock ( this )
			 {
				  HazelcastInstance hzInstance = TryEnsureConnection();
      
				  try
				  {
						operation( hzInstance );
				  }
				  catch ( com.hazelcast.core.HazelcastInstanceNotActiveException e )
				  {
						InvalidateConnection();
						throw new HazelcastInstanceNotActiveException( e );
				  }
			 }
		 }
	}

}