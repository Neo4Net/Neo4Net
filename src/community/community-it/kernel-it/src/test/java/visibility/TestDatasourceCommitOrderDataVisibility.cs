using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Visibility
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.Index;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class TestDatasourceCommitOrderDataVisibility
	{
		 private const string INDEX_NAME = "foo";
		 private const string INDEX_KEY = "bar";
		 private const string INDEX_VALUE = "baz";
		 private const string PROPERTY_NAME = "quux";
		 private const int PROPERTY_VALUE = 42;

		 private IGraphDatabaseService _graphDatabaseService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _graphDatabaseService = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _graphDatabaseService.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotMakeIndexWritesVisibleUntilCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotMakeIndexWritesVisibleUntilCommit()
		 {
			  Node commonNode;
			  using ( Transaction tx = _graphDatabaseService.beginTx() )
			  {
					commonNode = _graphDatabaseService.createNode();
					tx.Success();
			  }

			  using ( Transaction transaction = _graphDatabaseService.beginTx() )
			  {
					// index write first so that that datastore is added first
					_graphDatabaseService.index().forNodes(INDEX_NAME).add(commonNode, INDEX_KEY, INDEX_VALUE);
					commonNode.SetProperty( PROPERTY_NAME, PROPERTY_VALUE );

					AssertNodeIsNotIndexedOutsideThisTransaction();
					AssertNodeIsUnchangedOutsideThisTransaction( commonNode );

					transaction.Success();

					AssertNodeIsNotIndexedOutsideThisTransaction();
					AssertNodeIsUnchangedOutsideThisTransaction( commonNode );
			  }

			  AssertNodeIsIndexed( commonNode );
			  AssertNodeHasBeenUpdated( commonNode );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNodeIsNotIndexedOutsideThisTransaction() throws Exception
		 private void AssertNodeIsNotIndexedOutsideThisTransaction()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<Exception> problems = new java.util.HashSet<>();
			  ICollection<Exception> problems = new HashSet<Exception>();

			  Thread thread = new Thread(() =>
			  {
				try
				{
					using ( Transaction ignored = _graphDatabaseService.beginTx(), IndexHits<Node> indexHits = _graphDatabaseService.index().forNodes(INDEX_NAME).get(INDEX_KEY, INDEX_VALUE) )
					{
						 assertThat( indexHits.size(), @is(0) );
					}
				}
				catch ( Exception t )
				{
					 problems.Add( new Exception( t ) );
				}
			  });
			  thread.Start();
			  thread.Join();

			  if ( problems.Count > 0 )
			  {
					throw problems.GetEnumerator().next();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNodeIsUnchangedOutsideThisTransaction(final org.Neo4Net.graphdb.Node commonNode) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void AssertNodeIsUnchangedOutsideThisTransaction( Node commonNode )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<Exception> problems = new java.util.HashSet<>();
			  ICollection<Exception> problems = new HashSet<Exception>();

			  Thread thread = new Thread(() =>
			  {
				try
				{
					using ( Transaction ignored = _graphDatabaseService.beginTx() )
					{
						 assertThat( commonNode.HasProperty( PROPERTY_NAME ), @is( false ) );
					}
				}
				catch ( Exception t )
				{
					 problems.Add( new Exception( t ) );
				}
			  });
			  thread.Start();
			  thread.Join();

			  if ( problems.Count > 0 )
			  {
					throw problems.GetEnumerator().next();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNodeIsIndexed(final org.Neo4Net.graphdb.Node commonNode) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void AssertNodeIsIndexed( Node commonNode )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<Exception> problems = new java.util.HashSet<>();
			  ICollection<Exception> problems = new HashSet<Exception>();

			  Thread thread = new Thread(() =>
			  {
				try
				{
					using ( Transaction ignored = _graphDatabaseService.beginTx() )
					{
						 Node node = _graphDatabaseService.index().forNodes(INDEX_NAME).get(INDEX_KEY, INDEX_VALUE).Single;
						 assertThat( node, @is( commonNode ) );
					}
				}
				catch ( Exception t )
				{
					 problems.Add( new Exception( t ) );
				}
			  });
			  thread.Start();
			  thread.Join();

			  if ( problems.Count > 0 )
			  {
					throw problems.GetEnumerator().next();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNodeHasBeenUpdated(final org.Neo4Net.graphdb.Node commonNode) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void AssertNodeHasBeenUpdated( Node commonNode )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<Exception> problems = new java.util.HashSet<>();
			  ICollection<Exception> problems = new HashSet<Exception>();

			  Thread thread = new Thread(() =>
			  {
				try
				{
					using ( Transaction ignored = _graphDatabaseService.beginTx() )
					{
						 assertThat( commonNode.GetProperty( PROPERTY_NAME ), @is( PROPERTY_VALUE ) );
					}
				}
				catch ( Exception t )
				{
					 problems.Add( new Exception( t ) );
				}
			  });
			  thread.Start();
			  thread.Join();

			  if ( problems.Count > 0 )
			  {
					throw problems.GetEnumerator().next();
			  }
		 }
	}

}