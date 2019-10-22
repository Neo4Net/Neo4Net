﻿/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Bolt.runtime
{

	using Authentication = Neo4Net.Bolt.security.auth.Authentication;
	using BoltProtocolV1 = Neo4Net.Bolt.v1.BoltProtocolV1;
	using BoltStateMachineV1 = Neo4Net.Bolt.v1.runtime.BoltStateMachineV1;
	using BoltStateMachineV1SPI = Neo4Net.Bolt.v1.runtime.BoltStateMachineV1SPI;
	using TransactionStateMachineV1SPI = Neo4Net.Bolt.v1.runtime.TransactionStateMachineV1SPI;
	using BoltProtocolV2 = Neo4Net.Bolt.v2.BoltProtocolV2;
	using BoltProtocolV3 = Neo4Net.Bolt.v3.BoltProtocolV3;
	using BoltStateMachineV3 = Neo4Net.Bolt.v3.BoltStateMachineV3;
	using TransactionStateMachineV3SPI = Neo4Net.Bolt.v3.runtime.TransactionStateMachineV3SPI;
	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using UsageData = Neo4Net.Udc.UsageData;

	public class BoltStateMachineFactoryImpl : BoltStateMachineFactory
	{
		 private readonly DatabaseManager _databaseManager;
		 private readonly UsageData _usageData;
		 private readonly LogService _logging;
		 private readonly Authentication _authentication;
		 private readonly Config _config;
		 private readonly Clock _clock;
		 private readonly string _activeDatabaseName;

		 public BoltStateMachineFactoryImpl( DatabaseManager databaseManager, UsageData usageData, Authentication authentication, Clock clock, Config config, LogService logging )
		 {
			  this._databaseManager = databaseManager;
			  this._usageData = usageData;
			  this._logging = logging;
			  this._authentication = authentication;
			  this._config = config;
			  this._clock = clock;
			  this._activeDatabaseName = config.Get( GraphDatabaseSettings.active_database );
		 }

		 public override BoltStateMachine NewStateMachine( long protocolVersion, BoltChannel boltChannel )
		 {
			  if ( protocolVersion == BoltProtocolV1.VERSION || protocolVersion == BoltProtocolV2.VERSION )
			  {
					return NewStateMachineV1( boltChannel );
			  }
			  else if ( protocolVersion == BoltProtocolV3.VERSION )
			  {
					return NewStateMachineV3( boltChannel );
			  }
			  else
			  {
					throw new System.ArgumentException( "Failed to create a state machine for protocol version " + protocolVersion );
			  }
		 }

		 private BoltStateMachine NewStateMachineV1( BoltChannel boltChannel )
		 {
			  TransactionStateMachineSPI transactionSPI = new TransactionStateMachineV1SPI( ActiveDatabase, boltChannel, AwaitDuration, _clock );
			  BoltStateMachineSPI boltSPI = new BoltStateMachineV1SPI( _usageData, _logging, _authentication, transactionSPI );
			  return new BoltStateMachineV1( boltSPI, boltChannel, _clock );
		 }

		 private BoltStateMachine NewStateMachineV3( BoltChannel boltChannel )
		 {
			  TransactionStateMachineSPI transactionSPI = new TransactionStateMachineV3SPI( ActiveDatabase, boltChannel, AwaitDuration, _clock );
			  BoltStateMachineSPI boltSPI = new BoltStateMachineV1SPI( _usageData, _logging, _authentication, transactionSPI );
			  return new BoltStateMachineV3( boltSPI, boltChannel, _clock );
		 }

		 private Duration AwaitDuration
		 {
			 get
			 {
				  long bookmarkReadyTimeout = _config.get( GraphDatabaseSettings.bookmark_ready_timeout ).toMillis();
   
				  return Duration.ofMillis( bookmarkReadyTimeout );
			 }
		 }

		 private GraphDatabaseFacade ActiveDatabase
		 {
			 get
			 {
				  return _databaseManager.getDatabaseFacade( _activeDatabaseName ).get();
			 }
		 }
	}

}