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
namespace Org.Neo4j.Kernel.impl.core
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using ErrorState = Org.Neo4j.Graphdb.@event.ErrorState;
	using KernelEventHandler = Org.Neo4j.Graphdb.@event.KernelEventHandler;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class TestShutdownSequence
	{
		 private GraphDatabaseService _graphDb;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createGraphDb()
		 public virtual void CreateGraphDb()
		 {
			  _graphDb = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canInvokeShutdownMultipleTimes()
		 public virtual void CanInvokeShutdownMultipleTimes()
		 {
			  _graphDb.shutdown();
			  _graphDb.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eventHandlersAreOnlyInvokedOnceDuringShutdown()
		 public virtual void EventHandlersAreOnlyInvokedOnceDuringShutdown()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger counter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger counter = new AtomicInteger();
			  _graphDb.registerKernelEventHandler( new KernelEventHandlerAnonymousInnerClass( this, counter ) );
			  _graphDb.shutdown();
			  _graphDb.shutdown();
			  assertEquals( 1, counter.get() );
		 }

		 private class KernelEventHandlerAnonymousInnerClass : KernelEventHandler
		 {
			 private readonly TestShutdownSequence _outerInstance;

			 private AtomicInteger _counter;

			 public KernelEventHandlerAnonymousInnerClass( TestShutdownSequence outerInstance, AtomicInteger counter )
			 {
				 this.outerInstance = outerInstance;
				 this._counter = counter;
			 }

			 public void beforeShutdown()
			 {
				  _counter.incrementAndGet();
			 }

			 public object Resource
			 {
				 get
				 {
					  return null;
				 }
			 }

			 public void kernelPanic( ErrorState error )
			 {
				  // do nothing
			 }

			 public Org.Neo4j.Graphdb.@event.KernelEventHandler_ExecutionOrder orderComparedTo( KernelEventHandler other )
			 {
				  return Org.Neo4j.Graphdb.@event.KernelEventHandler_ExecutionOrder.DoesntMatter;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canRemoveFilesAndReinvokeShutdown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanRemoveFilesAndReinvokeShutdown()
		 {
			  GraphDatabaseAPI databaseAPI = ( GraphDatabaseAPI ) this._graphDb;
			  FileSystemAbstraction fileSystemAbstraction = GetDatabaseFileSystem( databaseAPI );
			  _graphDb.shutdown();
			  fileSystemAbstraction.DeleteRecursively( databaseAPI.DatabaseLayout().databaseDirectory() );
			  _graphDb.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canInvokeShutdownFromShutdownHandler()
		 public virtual void CanInvokeShutdownFromShutdownHandler()
		 {
			  _graphDb.registerKernelEventHandler( new KernelEventHandlerAnonymousInnerClass2( this ) );
			  _graphDb.shutdown();
		 }

		 private class KernelEventHandlerAnonymousInnerClass2 : KernelEventHandler
		 {
			 private readonly TestShutdownSequence _outerInstance;

			 public KernelEventHandlerAnonymousInnerClass2( TestShutdownSequence outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public void beforeShutdown()
			 {
				  _outerInstance.graphDb.shutdown();
			 }

			 public object Resource
			 {
				 get
				 {
					  return null;
				 }
			 }

			 public void kernelPanic( ErrorState error )
			 {
				  // do nothing
			 }

			 public Org.Neo4j.Graphdb.@event.KernelEventHandler_ExecutionOrder orderComparedTo( KernelEventHandler other )
			 {
				  return Org.Neo4j.Graphdb.@event.KernelEventHandler_ExecutionOrder.DoesntMatter;
			 }
		 }

		 private static FileSystemAbstraction GetDatabaseFileSystem( GraphDatabaseAPI databaseAPI )
		 {
			  return databaseAPI.DependencyResolver.resolveDependency( typeof( FileSystemAbstraction ) );
		 }
	}

}