using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Index
{
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public abstract class Neo4jTestCase
	{
		 private static GraphDatabaseService _graphDb;
		 private Transaction _tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUpDb()
		 public static void SetUpDb()
		 {
			  _graphDb = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDownDb()
		 public static void TearDownDb()
		 {
			  _graphDb.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUpTest()
		 public virtual void SetUpTest()
		 {
			  _tx = _graphDb.beginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDownTest()
		 public virtual void TearDownTest()
		 {
			  if ( !ManageMyOwnTxFinish() )
			  {
					FinishTx( true );
			  }
		 }

		 protected internal virtual bool ManageMyOwnTxFinish()
		 {
			  return false;
		 }

		 protected internal virtual void FinishTx( bool commit )
		 {
			  if ( _tx == null )
			  {
					return;
			  }

			  if ( commit )
			  {
					_tx.success();
			  }
			  _tx.close();
			  _tx = null;
		 }

		 protected internal virtual Transaction BeginTx()
		 {
			  if ( _tx == null )
			  {
					_tx = _graphDb.beginTx();
			  }
			  return _tx;
		 }

		 public static void DeleteFileOrDirectory( File file )
		 {
			  if ( !file.exists() )
			  {
					return;
			  }

			  if ( file.Directory )
			  {
					foreach ( File child in Objects.requireNonNull( file.listFiles() ) )
					{
						 DeleteFileOrDirectory( child );
					}
			  }
			  assertTrue( "delete " + file, file.delete() );
		 }

		 protected internal static GraphDatabaseService GraphDb()
		 {
			  return _graphDb;
		 }

		 public static void AssertContains<T>( ICollection<T> collection, params T[] expectedItems )
		 {
			  string collectionString = Join( ", ", collection.ToArray() );
			  assertEquals( collectionString, expectedItems.Length, collection.Count );
			  foreach ( T item in expectedItems )
			  {
					assertTrue( collection.Contains( item ) );
			  }
		 }

		 public static void AssertContains<T>( IEnumerable<T> items, params T[] expectedItems )
		 {
			  AssertContains( AsCollection( items ), expectedItems );
		 }

		 public static void AssertContainsInOrder<T>( ICollection<T> collection, params T[] expectedItems )
		 {
			  string collectionString = Join( ", ", collection.ToArray() );
			  assertEquals( collectionString, expectedItems.Length, collection.Count );
			  IEnumerator<T> itr = collection.GetEnumerator();
			  for ( int i = 0; itr.MoveNext(); i++ )
			  {
					assertEquals( expectedItems[i], itr.Current );
			  }
		 }

		 public static void AssertContainsInOrder<T>( IEnumerable<T> collection, params T[] expectedItems )
		 {
			  AssertContainsInOrder( AsCollection( collection ), expectedItems );
		 }

		 public static ICollection<T> AsCollection<T>( IEnumerable<T> iterable )
		 {
			  IList<T> list = new List<T>();
			  foreach ( T item in iterable )
			  {
					list.Add( item );
			  }
			  return list;
		 }

		 public static string Join<T>( string delimiter, params T[] items )
		 {
			  StringBuilder buffer = new StringBuilder();
			  foreach ( T item in items )
			  {
					if ( buffer.Length > 0 )
					{
						 buffer.Append( delimiter );
					}
					buffer.Append( item.ToString() );
			  }
			  return buffer.ToString();
		 }
	}

}