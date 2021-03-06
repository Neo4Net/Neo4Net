﻿using System;

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
namespace Org.Neo4j.Graphdb.facade
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using CommunityEditionModule = Org.Neo4j.Graphdb.factory.module.edition.CommunityEditionModule;
	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using SchemaWriteGuard = Org.Neo4j.Kernel.Impl.Api.SchemaWriteGuard;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using EphemeralFileSystemExtension = Org.Neo4j.Test.extension.EphemeralFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.facade.GraphDatabaseDependencies.newDependencies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.factory.DatabaseInfo.COMMUNITY;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({EphemeralFileSystemExtension.class, TestDirectoryExtension.class}) class GraphDatabaseFacadeFactoryTest
	internal class GraphDatabaseFacadeFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

		 private readonly GraphDatabaseFacade _mockFacade = mock( typeof( GraphDatabaseFacade ) );
		 private readonly GraphDatabaseFacadeFactory.Dependencies _deps = mock( typeof( GraphDatabaseFacadeFactory.Dependencies ), RETURNS_MOCKS );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setup()
		 internal virtual void Setup()
		 {
			  when( _deps.monitors() ).thenReturn(new Monitors());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowAppropriateExceptionIfStartFails()
		 internal virtual void ShouldThrowAppropriateExceptionIfStartFails()
		 {
			  Exception startupError = new Exception();
			  GraphDatabaseFacadeFactory db = NewFaultyGraphDatabaseFacadeFactory( startupError );
			  Exception startException = assertThrows( typeof( Exception ), () => Db.initFacade(_testDirectory.storeDir(), Collections.emptyMap(), _deps, _mockFacade) );
			  assertEquals( startupError, Exceptions.rootCause( startException ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowAppropriateExceptionIfBothStartAndShutdownFail()
		 internal virtual void ShouldThrowAppropriateExceptionIfBothStartAndShutdownFail()
		 {
			  Exception startupError = new Exception();
			  Exception shutdownError = new Exception();

			  GraphDatabaseFacadeFactory db = NewFaultyGraphDatabaseFacadeFactory( startupError );
			  doThrow( shutdownError ).when( _mockFacade ).shutdown();
			  Exception initException = assertThrows( typeof( Exception ), () => Db.initFacade(_testDirectory.storeDir(), Collections.emptyMap(), _deps, _mockFacade) );

			  assertTrue( initException.Message.StartsWith( "Error starting " ) );
			  assertEquals( startupError, initException.InnerException );
			  assertEquals( shutdownError, initException.Suppressed[0] );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private GraphDatabaseFacadeFactory newFaultyGraphDatabaseFacadeFactory(final RuntimeException startupError)
		 private GraphDatabaseFacadeFactory NewFaultyGraphDatabaseFacadeFactory( Exception startupError )
		 {
			  PlatformModule platformModule = new PlatformModule( _testDirectory.storeDir(), Config.defaults(), COMMUNITY, newDependencies() );
			  AbstractEditionModule editionModule = new CommunityEditionModuleAnonymousInnerClass( this, platformModule );
			  return new GraphDatabaseFacadeFactoryAnonymousInnerClass( this, DatabaseInfo.UNKNOWN, startupError );
		 }

		 private class CommunityEditionModuleAnonymousInnerClass : CommunityEditionModule
		 {
			 private readonly GraphDatabaseFacadeFactoryTest _outerInstance;

			 public CommunityEditionModuleAnonymousInnerClass( GraphDatabaseFacadeFactoryTest outerInstance, PlatformModule platformModule ) : base( platformModule )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override SchemaWriteGuard createSchemaWriteGuard()
			 {
				  return Org.Neo4j.Kernel.Impl.Api.SchemaWriteGuard_Fields.AllowAllWrites;
			 }
		 }

		 private class GraphDatabaseFacadeFactoryAnonymousInnerClass : GraphDatabaseFacadeFactory
		 {
			 private readonly GraphDatabaseFacadeFactoryTest _outerInstance;

			 private Exception _startupError;

			 public GraphDatabaseFacadeFactoryAnonymousInnerClass( GraphDatabaseFacadeFactoryTest outerInstance, DatabaseInfo unknown, Exception startupError ) : base( unknown, p -> editionModule )
			 {
				 this.outerInstance = outerInstance;
				 this._startupError = startupError;
			 }

			 protected internal override PlatformModule createPlatform( File storeDir, Config config, Dependencies dependencies )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.lifecycle.LifeSupport lifeMock = mock(org.neo4j.kernel.lifecycle.LifeSupport.class);
				  LifeSupport lifeMock = mock( typeof( LifeSupport ) );
				  doThrow( _startupError ).when( lifeMock ).start();
				  doAnswer( invocation => invocation.getArgument( 0 ) ).when( lifeMock ).add( any( typeof( Lifecycle ) ) );

				  return new PlatformModuleAnonymousInnerClass( this, storeDir, config, databaseInfo, dependencies, lifeMock );
			 }

			 private class PlatformModuleAnonymousInnerClass : PlatformModule
			 {
				 private readonly GraphDatabaseFacadeFactoryAnonymousInnerClass _outerInstance;

				 private LifeSupport _lifeMock;

				 public PlatformModuleAnonymousInnerClass( GraphDatabaseFacadeFactoryAnonymousInnerClass outerInstance, File storeDir, Config config, UnknownType databaseInfo, Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies dependencies, LifeSupport lifeMock ) : base( storeDir, config, databaseInfo, dependencies )
				 {
					 this.outerInstance = outerInstance;
					 this._lifeMock = lifeMock;
				 }

				 public override LifeSupport createLife()
				 {
					  return _lifeMock;
				 }
			 }
		 }
	}

}