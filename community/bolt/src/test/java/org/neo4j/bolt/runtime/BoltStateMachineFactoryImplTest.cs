﻿/*
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
namespace Org.Neo4j.Bolt.runtime
{
	using Test = org.junit.jupiter.api.Test;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using ValueSource = org.junit.jupiter.@params.provider.ValueSource;


	using Authentication = Org.Neo4j.Bolt.security.auth.Authentication;
	using BoltTestUtil = Org.Neo4j.Bolt.testing.BoltTestUtil;
	using BoltProtocolV1 = Org.Neo4j.Bolt.v1.BoltProtocolV1;
	using BoltStateMachineV1 = Org.Neo4j.Bolt.v1.runtime.BoltStateMachineV1;
	using BoltProtocolV2 = Org.Neo4j.Bolt.v2.BoltProtocolV2;
	using BoltStateMachineV3 = Org.Neo4j.Bolt.v3.BoltStateMachineV3;
	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using GraphDatabaseQueryService = Org.Neo4j.Kernel.GraphDatabaseQueryService;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using OnDemandJobScheduler = Org.Neo4j.Test.OnDemandJobScheduler;
	using UsageData = Org.Neo4j.Udc.UsageData;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class BoltStateMachineFactoryImplTest
	{
		 private const string CUSTOM_DB_NAME = "customDbName";
		 private static readonly Clock _clock = Clock.systemUTC();
		 private static readonly BoltChannel _channel = BoltTestUtil.newTestBoltChannel();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest(name = "V{0}") @ValueSource(longs = {org.neo4j.bolt.v1.BoltProtocolV1.VERSION, org.neo4j.bolt.v2.BoltProtocolV2.VERSION}) void shouldCreateBoltStateMachinesV1(long protocolVersion)
		 internal virtual void ShouldCreateBoltStateMachinesV1( long protocolVersion )
		 {
			  BoltStateMachineFactoryImpl factory = NewBoltFactory();

			  BoltStateMachine boltStateMachine = factory.NewStateMachine( protocolVersion, _channel );

			  assertNotNull( boltStateMachine );
			  assertThat( boltStateMachine, instanceOf( typeof( BoltStateMachineV1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateBoltStateMachinesV3()
		 internal virtual void ShouldCreateBoltStateMachinesV3()
		 {
			  BoltStateMachineFactoryImpl factory = NewBoltFactory();

			  BoltStateMachine boltStateMachine = factory.NewStateMachine( 3L, _channel );

			  assertNotNull( boltStateMachine );
			  assertThat( boltStateMachine, instanceOf( typeof( BoltStateMachineV3 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest(name = "V{0}") @ValueSource(longs = {999, -1}) void shouldThrowExceptionIfVersionIsUnknown(long protocolVersion)
		 internal virtual void ShouldThrowExceptionIfVersionIsUnknown( long protocolVersion )
		 {
			  BoltStateMachineFactoryImpl factory = NewBoltFactory();

			  System.ArgumentException error = assertThrows( typeof( System.ArgumentException ), () => factory.NewStateMachine(protocolVersion, _channel) );
			  assertThat( error.Message, startsWith( "Failed to create a state machine for protocol version" ) );
		 }

		 private static BoltStateMachineFactoryImpl NewBoltFactory()
		 {
			  return NewBoltFactory( NewDbMock() );
		 }

		 private static BoltStateMachineFactoryImpl NewBoltFactory( DatabaseManager databaseManager )
		 {
			  Config config = Config.defaults( GraphDatabaseSettings.active_database, CUSTOM_DB_NAME );
			  return new BoltStateMachineFactoryImpl( databaseManager, new UsageData( new OnDemandJobScheduler() ), mock(typeof(Authentication)), _clock, config, NullLogService.Instance );
		 }

		 private static DatabaseManager NewDbMock()
		 {
			  GraphDatabaseFacade db = mock( typeof( GraphDatabaseFacade ) );
			  DependencyResolver dependencyResolver = mock( typeof( DependencyResolver ) );
			  when( Db.DependencyResolver ).thenReturn( dependencyResolver );
			  GraphDatabaseQueryService queryService = mock( typeof( GraphDatabaseQueryService ) );
			  when( queryService.DependencyResolver ).thenReturn( dependencyResolver );
			  when( dependencyResolver.ResolveDependency( typeof( GraphDatabaseQueryService ) ) ).thenReturn( queryService );
			  DatabaseManager databaseManager = mock( typeof( DatabaseManager ) );
			  when( databaseManager.GetDatabaseFacade( CUSTOM_DB_NAME ) ).thenReturn( db );
			  return databaseManager;
		 }
	}

}