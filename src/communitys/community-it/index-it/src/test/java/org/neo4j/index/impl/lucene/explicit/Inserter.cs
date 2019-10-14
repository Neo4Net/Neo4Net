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
namespace Neo4Net.Index.impl.lucene.@explicit
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Graphdb.index;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

	public class Inserter
	{
		 private Inserter()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
		 public static void Main( string[] args )
		 {
			  File path = new File( args[0] );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.GraphDatabaseService db = new org.neo4j.test.TestGraphDatabaseFactory().newEmbeddedDatabaseBuilder(path).newGraphDatabase();
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(path).newGraphDatabase();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.index.Index<org.neo4j.graphdb.Node> index = getIndex(db);
			  Index<Node> index = GetIndex( db );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] keys = new String[]{"apoc", "zion", "morpheus"};
			  string[] keys = new string[]{ "apoc", "zion", "morpheus" };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] values = new String[]{"hej", "yo", "something", "just a value", "anything"};
			  string[] values = new string[]{ "hej", "yo", "something", "just a value", "anything" };

			  for ( int i = 0; i < 5; i++ )
			  {
					(new Thread(() =>
					{
					while ( true )
					{
						using ( Transaction tx = Db.beginTx() )
						{
							for ( int i1 = 0; i1 < 100; i1++ )
							{
								string key = keys[i1 % keys.Length];
								string value = values[i1 % values.Length] + i1;
								Node node = Db.createNode();
								node.setProperty( key, value );
								index.Add( node, key, value );
							}
							tx.success();
						}
					}
					})).Start();
			  }
			  ( new File( path, "started" ) ).createNewFile();
		 }

		 private static Index<Node> GetIndex( GraphDatabaseService db )
		 {
			  using ( Transaction transaction = Db.beginTx() )
			  {
					Index<Node> index = Db.index().forNodes("myIndex");
					transaction.Success();
					return index;
			  }
		 }
	}

}