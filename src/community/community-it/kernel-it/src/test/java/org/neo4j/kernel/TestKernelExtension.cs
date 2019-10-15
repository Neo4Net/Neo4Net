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
namespace Neo4Net.Kernel
{
	using Test = org.junit.Test;

	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using Config = Neo4Net.Kernel.configuration.Config;
	using GlobalKernelExtensions = Neo4Net.Kernel.extension.GlobalKernelExtensions;
	using KernelExtensionFactoryContractTest = Neo4Net.Kernel.extension.KernelExtensionFactoryContractTest;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using LifecycleStatus = Neo4Net.Kernel.Lifecycle.LifecycleStatus;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	/// <summary>
	/// Test the implementation of the <seealso cref="org.neo4j.kernel.extension.KernelExtensionFactory"/> framework. Treats the
	/// framework as a black box and takes the perspective of the extension, making
	/// sure that the framework fulfills its part of the contract. The parent class (
	/// <seealso cref="KernelExtensionFactoryContractTest"/>) takes the opposite approach, it treats
	/// the extension implementation as a black box to assert that it fulfills the
	/// requirements stipulated by the framework.
	/// </summary>
	public sealed class TestKernelExtension : KernelExtensionFactoryContractTest
	{
		 public TestKernelExtension() : base(DummyExtensionFactory.EXTENSION_ID, typeof(DummyExtensionFactory))
		 {
		 }

		 /// <summary>
		 /// Check that lifecycle status of extension is STARTED
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeStarted()
		 public void ShouldBeStarted()
		 {
			  GraphDatabaseAPI graphdb = GraphDb( 0 );
			  try
			  {
					assertEquals( LifecycleStatus.STARTED, graphdb.DependencyResolver.resolveDependency( typeof( GlobalKernelExtensions ) ).resolveDependency( typeof( DummyExtension ) ).Status );
			  }
			  finally
			  {
					graphdb.Shutdown();
			  }
		 }

		 /// <summary>
		 /// Check that dependencies can be accessed
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dependenciesCanBeRetrieved()
		 public void DependenciesCanBeRetrieved()
		 {
			  GraphDatabaseAPI graphdb = GraphDb( 0 );
			  try
			  {
					assertEquals( graphdb.DependencyResolver.resolveDependency( typeof( Config ) ), graphdb.DependencyResolver.resolveDependency( typeof( GlobalKernelExtensions ) ).resolveDependency( typeof( DummyExtension ) ).Dependencies.Config );
					assertEquals( graphdb.DependencyResolver.resolveDependency( typeof( DatabaseManager ) ), graphdb.DependencyResolver.resolveDependency( typeof( GlobalKernelExtensions ) ).resolveDependency( typeof( DummyExtension ) ).Dependencies.DatabaseManager );
			  }
			  finally
			  {
					graphdb.Shutdown();
			  }
		 }

		 /// <summary>
		 /// Check that lifecycle status of extension is SHUTDOWN
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeShutdown()
		 public void ShouldBeShutdown()
		 {
			  GraphDatabaseAPI graphdb = GraphDb( 0 );
			  graphdb.Shutdown();

			  assertEquals( LifecycleStatus.SHUTDOWN, graphdb.DependencyResolver.resolveDependency( typeof( GlobalKernelExtensions ) ).resolveDependency( typeof( DummyExtension ) ).Status );
		 }
	}

}