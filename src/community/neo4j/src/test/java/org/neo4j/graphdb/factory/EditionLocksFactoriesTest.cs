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
namespace Neo4Net.GraphDb.factory
{
	using Test = org.junit.jupiter.api.Test;

	using Config = Neo4Net.Kernel.configuration.Config;
	using LocksFactory = Neo4Net.Kernel.impl.locking.LocksFactory;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using CommunityLocksFactory = Neo4Net.Kernel.impl.locking.community.CommunityLocksFactory;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.EditionLocksFactories.createLockFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.EditionLocksFactories.createLockManager;

	internal class EditionLocksFactoriesTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createLocksForAllResourceTypes()
		 internal virtual void CreateLocksForAllResourceTypes()
		 {
			  LocksFactory lockFactory = mock( typeof( LocksFactory ) );
			  Config config = Config.defaults();
			  Clock clock = Clocks.systemClock();

			  createLockManager( lockFactory, config, clock );

			  verify( lockFactory ).newInstance( eq( config ), eq( clock ), eq( ResourceTypes.values() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createCommunityLocksFactoryWhenNotConfigured()
		 internal virtual void CreateCommunityLocksFactoryWhenNotConfigured()
		 {
			  Config config = Config.defaults();
			  LocksFactory lockFactory = createLockFactory( config, NullLogService.Instance );

			  assertThat( lockFactory, instanceOf( typeof( CommunityLocksFactory ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createCommunityLocksFactoryWhenSpecified()
		 internal virtual void CreateCommunityLocksFactoryWhenSpecified()
		 {
			  Config config = Config.defaults( GraphDatabaseSettings.LockManager, "community" );

			  LocksFactory lockFactory = createLockFactory( config, NullLogService.Instance );

			  assertThat( lockFactory, instanceOf( typeof( CommunityLocksFactory ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failToCreateWhenConfiguredFactoryNotFound()
		 internal virtual void FailToCreateWhenConfiguredFactoryNotFound()
		 {
			  Config config = Config.defaults( GraphDatabaseSettings.LockManager, "notFoundManager" );

			  System.ArgumentException exception = assertThrows( typeof( System.ArgumentException ), () => createLockFactory(config, NullLogService.Instance) );

			  assertEquals( "No lock manager found with the name 'notFoundManager'.", exception.Message );
		 }
	}

}