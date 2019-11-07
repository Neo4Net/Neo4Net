﻿using System;

/*
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
namespace Neo4Net.Kernel.Impl.@event
{
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using ErrorState = Neo4Net.GraphDb.Events.ErrorState;
	using KernelEventHandler = Neo4Net.GraphDb.Events.KernelEventHandler;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.AbstractNeo4NetTestCase.deleteFileOrDirectory;

	public class TestKernelEvents
	{
		 private const string PATH = "target/var/neodb";

		 private static readonly object _resource1 = new object();
		 private static readonly object _resource2 = new object();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void doBefore()
		 public static void DoBefore()
		 {
			  deleteFileOrDirectory( PATH );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRegisterUnregisterHandlers()
		 public virtual void TestRegisterUnregisterHandlers()
		 {
			  IGraphDatabaseService graphDb = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  KernelEventHandler handler1 = new DummyKernelEventHandlerAnonymousInnerClass( this, _resource1 );
			  KernelEventHandler handler2 = new DummyKernelEventHandlerAnonymousInnerClass2( this, _resource2 );

			  try
			  {
					graphDb.UnregisterKernelEventHandler( handler1 );
					fail( "Shouldn't be able to do unregister on a " + "unregistered handler" );
			  }
			  catch ( System.InvalidOperationException )
			  { // Good
			  }

			  assertSame( handler1, graphDb.RegisterKernelEventHandler( handler1 ) );
			  assertSame( handler1, graphDb.RegisterKernelEventHandler( handler1 ) );
			  assertSame( handler1, graphDb.UnregisterKernelEventHandler( handler1 ) );

			  try
			  {
					graphDb.UnregisterKernelEventHandler( handler1 );
					fail( "Shouldn't be able to do unregister on a " + "unregistered handler" );
			  }
			  catch ( System.InvalidOperationException )
			  { // Good
			  }

			  assertSame( handler1, graphDb.RegisterKernelEventHandler( handler1 ) );
			  assertSame( handler2, graphDb.RegisterKernelEventHandler( handler2 ) );
			  assertSame( handler1, graphDb.UnregisterKernelEventHandler( handler1 ) );
			  assertSame( handler2, graphDb.UnregisterKernelEventHandler( handler2 ) );

			  graphDb.Shutdown();
		 }

		 private class DummyKernelEventHandlerAnonymousInnerClass : DummyKernelEventHandler
		 {
			 private readonly TestKernelEvents _outerInstance;

			 public DummyKernelEventHandlerAnonymousInnerClass( TestKernelEvents outerInstance, object resource1 ) : base( resource1 )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override Neo4Net.GraphDb.Events.KernelEventHandler_ExecutionOrder orderComparedTo( KernelEventHandler other )
			 {
				  return Neo4Net.GraphDb.Events.KernelEventHandler_ExecutionOrder.DoesntMatter;
			 }
		 }

		 private class DummyKernelEventHandlerAnonymousInnerClass2 : DummyKernelEventHandler
		 {
			 private readonly TestKernelEvents _outerInstance;

			 public DummyKernelEventHandlerAnonymousInnerClass2( TestKernelEvents outerInstance, object resource2 ) : base( resource2 )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override Neo4Net.GraphDb.Events.KernelEventHandler_ExecutionOrder orderComparedTo( KernelEventHandler other )
			 {
				  return Neo4Net.GraphDb.Events.KernelEventHandler_ExecutionOrder.DoesntMatter;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testShutdownEvents()
		 public virtual void TestShutdownEvents()
		 {
			  IGraphDatabaseService graphDb = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  DummyKernelEventHandler handler1 = new DummyKernelEventHandlerAnonymousInnerClass3( this, _resource1 );
			  DummyKernelEventHandler handler2 = new DummyKernelEventHandlerAnonymousInnerClass4( this, _resource1 );
			  graphDb.RegisterKernelEventHandler( handler1 );
			  graphDb.RegisterKernelEventHandler( handler2 );

			  graphDb.Shutdown();

			  assertEquals( Convert.ToInt32( 0 ), handler2.BeforeShutdownConflict );
			  assertEquals( Convert.ToInt32( 1 ), handler1.BeforeShutdownConflict );
		 }

		 private class DummyKernelEventHandlerAnonymousInnerClass3 : DummyKernelEventHandler
		 {
			 private readonly TestKernelEvents _outerInstance;

			 public DummyKernelEventHandlerAnonymousInnerClass3( TestKernelEvents outerInstance, object resource1 ) : base( resource1 )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override Neo4Net.GraphDb.Events.KernelEventHandler_ExecutionOrder orderComparedTo( KernelEventHandler other )
			 {
				  if ( ( ( DummyKernelEventHandler ) other ).ResourceConflict == _resource2 )
				  {
						return Neo4Net.GraphDb.Events.KernelEventHandler_ExecutionOrder.After;
				  }
				  return Neo4Net.GraphDb.Events.KernelEventHandler_ExecutionOrder.DoesntMatter;
			 }
		 }

		 private class DummyKernelEventHandlerAnonymousInnerClass4 : DummyKernelEventHandler
		 {
			 private readonly TestKernelEvents _outerInstance;

			 public DummyKernelEventHandlerAnonymousInnerClass4( TestKernelEvents outerInstance, object resource1 ) : base( resource1 )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override Neo4Net.GraphDb.Events.KernelEventHandler_ExecutionOrder orderComparedTo( KernelEventHandler other )
			 {
				  if ( ( ( DummyKernelEventHandler ) other ).ResourceConflict == _resource1 )
				  {
						return Neo4Net.GraphDb.Events.KernelEventHandler_ExecutionOrder.Before;
				  }
				  return Neo4Net.GraphDb.Events.KernelEventHandler_ExecutionOrder.DoesntMatter;
			 }
		 }

		 private abstract class DummyKernelEventHandler : KernelEventHandler
		 {
			 public abstract KernelEventHandler_ExecutionOrder OrderComparedTo( KernelEventHandler other );
			  internal static int Counter;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int? BeforeShutdownConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int? KernelPanicConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly object ResourceConflict;

			  internal DummyKernelEventHandler( object resource )
			  {
					this.ResourceConflict = resource;
			  }

			  public override void BeforeShutdown()
			  {
					BeforeShutdownConflict = Counter++;
			  }

			  public virtual object Resource
			  {
				  get
				  {
						return this.ResourceConflict;
				  }
			  }

			  public override void KernelPanic( ErrorState error )
			  {
					KernelPanicConflict = Counter++;
			  }
		 }
	}

}