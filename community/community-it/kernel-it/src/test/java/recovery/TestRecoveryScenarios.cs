﻿using System.Collections.Generic;

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
namespace Recovery
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using TokenHolder = Org.Neo4j.Kernel.impl.core.TokenHolder;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Transaction_Type.@explicit;

	/// <summary>
	/// Arbitrary recovery scenarios boiled down to as small tests as possible
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class TestRecoveryScenarios
	public class TestRecoveryScenarios
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_label = _label( "label" );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fsRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();
		 private Label _label;
		 private GraphDatabaseAPI _db;

		 private readonly FlushStrategy _flush;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") @Before public void before()
		 public virtual void Before()
		 {
			  _db = ( GraphDatabaseAPI ) DatabaseFactory( FsRule.get() ).newImpermanentDatabase();
		 }

		 public TestRecoveryScenarios( FlushStrategy flush )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._flush = flush;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverTransactionWhereNodeIsDeletedInTheFuture() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverTransactionWhereNodeIsDeletedInTheFuture()
		 {
			  // GIVEN
			  Node node = CreateNodeWithProperty( "key", "value", _label );
			  CheckPoint();
			  SetProperty( node, "other-key", 1 );
			  DeleteNode( node );
			  _flush.flush( _db );

			  // WHEN
			  CrashAndRestart();

			  // THEN
			  // -- really the problem was that recovery threw exception, so mostly assert that.
			  try
			  {
					  using ( Transaction tx = _db.beginTx() )
					  {
						node = _db.getNodeById( node.Id );
						tx.Success();
						fail( "Should not exist" );
					  }
			  }
			  catch ( NotFoundException e )
			  {
					assertEquals( "Node " + node.Id + " not found", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverTransactionWherePropertyIsRemovedInTheFuture() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverTransactionWherePropertyIsRemovedInTheFuture()
		 {
			  // GIVEN
			  CreateIndex( _label, "key" );
			  Node node = CreateNodeWithProperty( "key", "value" );
			  CheckPoint();
			  AddLabel( node, _label );
			  RemoveProperty( node, "key" );
			  _flush.flush( _db );

			  // WHEN
			  CrashAndRestart();

			  // THEN
			  // -- really the problem was that recovery threw exception, so mostly assert that.
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertEquals( "Updates not propagated correctly during recovery", System.Linq.Enumerable.Empty<Node>(), Iterators.asList(_db.findNodes(_label, "key", "value")) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverTransactionWhereManyLabelsAreRemovedInTheFuture() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverTransactionWhereManyLabelsAreRemovedInTheFuture()
		 {
			  // GIVEN
			  CreateIndex( _label, "key" );
			  Label[] labels = new Label[16];
			  for ( int i = 0; i < labels.Length; i++ )
			  {
					labels[i] = label( "Label" + i.ToString( "x" ) );
			  }
			  Node node;
			  using ( Transaction tx = _db.beginTx() )
			  {
					node = _db.createNode( labels );
					node.AddLabel( _label );
					tx.Success();
			  }
			  CheckPoint();
			  SetProperty( node, "key", "value" );
			  RemoveLabels( node, labels );
			  _flush.flush( _db );

			  // WHEN
			  CrashAndRestart();

			  // THEN
			  // -- really the problem was that recovery threw exception, so mostly assert that.
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertEquals( node, _db.findNode( _label, "key", "value" ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverCounts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverCounts()
		 {
			  // GIVEN
			  Node node = CreateNode( _label );
			  CheckPoint();
			  DeleteNode( node );

			  // WHEN
			  CrashAndRestart();

			  // THEN
			  // -- really the problem was that recovery threw exception, so mostly assert that.
			  using ( Org.Neo4j.@internal.Kernel.Api.Transaction tx = _db.DependencyResolver.resolveDependency( typeof( Kernel ) ).beginTransaction( @explicit, LoginContext.AUTH_DISABLED ) )
			  {
					assertEquals( 0, tx.DataRead().countsForNode(-1) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.core.TokenHolder holder = db.getDependencyResolver().resolveDependency(org.neo4j.kernel.impl.core.TokenHolders.class).labelTokens();
					TokenHolder holder = _db.DependencyResolver.resolveDependency( typeof( TokenHolders ) ).labelTokens();
					int labelId = holder.GetIdByName( _label.name() );
					assertEquals( 0, tx.DataRead().countsForNode(labelId) );
					tx.Success();
			  }
		 }

		 private void RemoveLabels( Node node, params Label[] labels )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					foreach ( Label label in labels )
					{
						 node.RemoveLabel( label );
					}
					tx.Success();
			  }
		 }

		 private void RemoveProperty( Node node, string key )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					node.RemoveProperty( key );
					tx.Success();
			  }
		 }

		 private void AddLabel( Node node, Label label )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					node.AddLabel( label );
					tx.Success();
			  }
		 }

		 private Node CreateNode( params Label[] labels )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( labels );
					tx.Success();
					return node;
			  }
		 }

		 private Node CreateNodeWithProperty( string key, string value, params Label[] labels )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( labels );
					node.SetProperty( key, value );
					tx.Success();
					return node;
			  }
		 }

		 private void CreateIndex( Label label, string key )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().indexFor(label).on(key).create();
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexesOnline(10, SECONDS);
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.List<Object[]> flushStrategy()
		 public static IList<object[]> FlushStrategy()
		 {
			  IList<object[]> parameters = new List<object[]>();
			  foreach ( FlushStrategy flushStrategy in FlushStrategy.values() )
			  {
					parameters.Add( flushStrategy.parameters );
			  }
			  return parameters;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") public enum FlushStrategy
		 public abstract class FlushStrategy
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           FORCE_EVERYTHING { void flush(org.neo4j.kernel.internal.GraphDatabaseAPI db) { org.neo4j.io.pagecache.IOLimiter limiter = org.neo4j.io.pagecache.IOLimiter_Fields.UNLIMITED; db.getDependencyResolver().resolveDependency(org.neo4j.storageengine.api.StorageEngine.class).flushAndForce(limiter); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           FLUSH_PAGE_CACHE { void flush(org.neo4j.kernel.internal.GraphDatabaseAPI db) throws java.io.IOException { db.getDependencyResolver().resolveDependency(org.neo4j.io.pagecache.PageCache.class).flushAndForce(); } };

			  private static readonly IList<FlushStrategy> valueList = new List<FlushStrategy>();

			  static FlushStrategy()
			  {
				  valueList.Add( FORCE_EVERYTHING );
				  valueList.Add( FLUSH_PAGE_CACHE );
			  }

			  public enum InnerEnum
			  {
				  FORCE_EVERYTHING,
				  FLUSH_PAGE_CACHE
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private FlushStrategy( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }
			  internal object[] parameters = new object[]{ this };

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void flush(org.neo4j.kernel.internal.GraphDatabaseAPI db) throws java.io.IOException;
			  internal abstract void flush( Org.Neo4j.Kernel.@internal.GraphDatabaseAPI db );

			 public static IList<FlushStrategy> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static FlushStrategy valueOf( string name )
			 {
				 foreach ( FlushStrategy enumInstance in FlushStrategy.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkPoint() throws java.io.IOException
		 private void CheckPoint()
		 {
			  _db.DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint(new SimpleTriggerInfo("test")
			 );
		 }

		 private void DeleteNode( Node node )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					node.Delete();
					tx.Success();
			  }
		 }

		 private void SetProperty( Node node, string key, object value )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					node.SetProperty( key, value );
					tx.Success();
			  }
		 }

		 private TestGraphDatabaseFactory DatabaseFactory( FileSystemAbstraction fs )
		 {
			  return ( new TestGraphDatabaseFactory() ).setFileSystem(fs);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") private void crashAndRestart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private void CrashAndRestart()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.GraphDatabaseService db1 = db;
			  GraphDatabaseService db1 = _db;
			  FileSystemAbstraction uncleanFs = FsRule.snapshot( db1.shutdown );
			  _db = ( GraphDatabaseAPI ) DatabaseFactory( uncleanFs ).newImpermanentDatabase();
		 }
	}

}