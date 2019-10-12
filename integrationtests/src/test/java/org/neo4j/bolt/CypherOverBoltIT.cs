using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Bolt
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Config = Org.Neo4j.driver.v1.Config;
	using Driver = Org.Neo4j.driver.v1.Driver;
	using GraphDatabase = Org.Neo4j.driver.v1.GraphDatabase;
	using Session = Org.Neo4j.driver.v1.Session;
	using StatementResult = Org.Neo4j.driver.v1.StatementResult;
	using Node = Org.Neo4j.driver.v1.types.Node;
	using Neo4jRule = Org.Neo4j.Harness.junit.Neo4jRule;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class CypherOverBoltIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.harness.junit.Neo4jRule graphDb = new org.neo4j.harness.junit.Neo4jRule();
		 public Neo4jRule GraphDb = new Neo4jRule();

		 private URL _url;
		 private readonly int _lineCountInCSV = 3; // needs to be >= 2

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _url = PrepareTestImportFile( _lineCountInCSV );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mixingPeriodicCommitAndLoadCSVShouldWork()
		 public virtual void MixingPeriodicCommitAndLoadCSVShouldWork()
		 {

			  for ( int i = _lineCountInCSV - 1; i < _lineCountInCSV + 1; i++ ) // test with different periodic commit sizes
			  {
					using ( Driver driver = GraphDatabase.driver( GraphDb.boltURI(), Configuration() ), Session session = driver.session() )
					{
						 StatementResult result = session.run( "USING PERIODIC COMMIT " + i + "\n" + "LOAD CSV FROM \"" + _url + "\" as row fieldterminator \" \"\n" + "MERGE (currentnode:Label1 {uuid:row[0]})\n" + "RETURN currentnode;" );
						 int countOfNodes = 0;
						 while ( result.hasNext() )
						 {
							  Node node = result.next().get(0).asNode();
							  assertTrue( node.hasLabel( "Label1" ) );
							  assertEquals( countOfNodes.ToString(), node.get("uuid").asString() );
							  countOfNodes++;
						 }
						 assertEquals( _lineCountInCSV, countOfNodes );
						 session.reset();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mixingPeriodicCommitAndLoadCSVShouldWork2()
		 public virtual void MixingPeriodicCommitAndLoadCSVShouldWork2()
		 {
			  using ( Driver driver = GraphDatabase.driver( GraphDb.boltURI(), Configuration() ), Session session = driver.session() )
			  {
					StatementResult result = session.run( "USING PERIODIC COMMIT " + ( _lineCountInCSV + 1 ) + "\n" + "LOAD CSV FROM \"" + _url + "\" as row fieldterminator \" \"\n" + "MERGE (currentnode:Label1 {uuid:row[0]})\n" + "RETURN currentnode;" );
					int countOfNodes = 0;
					while ( result.hasNext() )
					{
						 Node node = result.next().get(0).asNode();
						 assertTrue( node.hasLabel( "Label1" ) );
						 assertEquals( countOfNodes.ToString(), node.get("uuid").asString() );
						 countOfNodes++;
					}
					assertEquals( _lineCountInCSV, countOfNodes );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mixingPeriodicCommitAndLoadCSVShouldWork3()
		 public virtual void MixingPeriodicCommitAndLoadCSVShouldWork3()
		 {
			  using ( Driver driver = GraphDatabase.driver( GraphDb.boltURI(), Configuration() ), Session session = driver.session() )
			  {
					StatementResult result = session.run( "USING PERIODIC COMMIT " + _lineCountInCSV + "\n" + "LOAD CSV FROM \"" + _url + "\" as row fieldterminator \" \"\n" + "MERGE (currentnode:Label1 {uuid:row[0]})\n" + "RETURN currentnode;" );
					int countOfNodes = 0;
					while ( result.hasNext() )
					{
						 Node node = result.next().get(0).asNode();
						 assertTrue( node.hasLabel( "Label1" ) );
						 assertEquals( countOfNodes.ToString(), node.get("uuid").asString() );
						 countOfNodes++;
					}
					assertEquals( _lineCountInCSV, countOfNodes );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mixingPeriodicCommitAndLoadCSVShouldWorkWithLists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MixingPeriodicCommitAndLoadCSVShouldWorkWithLists()
		 {
			  using ( Driver driver = GraphDatabase.driver( GraphDb.boltURI(), Configuration() ), Session session = driver.session() )
			  {
					StatementResult result = session.run( "USING PERIODIC COMMIT " + ( _lineCountInCSV - 1 ) + "\n" + "LOAD CSV FROM \"" + _url + "\" as row fieldterminator \" \"\n" + "MERGE (currentnode:Label2 {uuid:row[0]})\n" + "RETURN [currentnode];" );
					int countOfNodes = 0;
					while ( result.hasNext() )
					{
						 IEnumerator<object> iterator = result.next().get(0).asList().GetEnumerator();
						 while ( iterator.MoveNext() )
						 {
							  Node node = ( Node ) iterator.Current;
							  assertTrue( node.hasLabel( "Label2" ) );
							  assertEquals( countOfNodes.ToString(), node.get("uuid").asString() );
							  countOfNodes++;
						 }
					}
					assertEquals( _lineCountInCSV, countOfNodes );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mixingPeriodicCommitAndLoadCSVShouldWorkWithListsOfLists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MixingPeriodicCommitAndLoadCSVShouldWorkWithListsOfLists()
		 {
			  using ( Driver driver = GraphDatabase.driver( GraphDb.boltURI(), Configuration() ), Session session = driver.session() )
			  {
					StatementResult result = session.run( "USING PERIODIC COMMIT " + ( _lineCountInCSV - 1 ) + "\n" + "LOAD CSV FROM \"" + _url + "\" as row fieldterminator \" \"\n" + "MERGE (currentnode:Label3 {uuid:row[0]})\n" + "RETURN [[currentnode]];" );
					int countOfNodes = 0;
					while ( result.hasNext() )
					{
						 IEnumerator<object> iterator = result.next().get(0).asList().GetEnumerator(); // iterator over outer list
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 iterator = ( ( IList<object> ) iterator.next() ).GetEnumerator(); // iterator over inner list
						 while ( iterator.MoveNext() )
						 {
							  Node node = ( Node ) iterator.Current;
							  assertTrue( node.hasLabel( "Label3" ) );
							  assertEquals( countOfNodes.ToString(), node.get("uuid").asString() );
							  countOfNodes++;
						 }
					}
					assertEquals( _lineCountInCSV, countOfNodes );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mixingPeriodicCommitAndLoadCSVShouldWorkWithMaps() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MixingPeriodicCommitAndLoadCSVShouldWorkWithMaps()
		 {
			  using ( Driver driver = GraphDatabase.driver( GraphDb.boltURI(), Configuration() ), Session session = driver.session() )
			  {
					StatementResult result = session.run( "USING PERIODIC COMMIT " + ( _lineCountInCSV - 1 ) + "\n" + "LOAD CSV FROM \"" + _url + "\" as row fieldterminator \" \"\n" + "MERGE (currentnode:Label4 {uuid:row[0]})\n" + "RETURN {node:currentnode};" );
					int countOfNodes = 0;
					while ( result.hasNext() )
					{
						 IEnumerator<KeyValuePair<string, object>> iterator = result.next().get(0).asMap().entrySet().GetEnumerator();
						 while ( iterator.MoveNext() )
						 {
							  KeyValuePair<string, object> entry = iterator.Current;
							  assertEquals( "node", entry.Key );
							  Node node = ( Node ) entry.Value;
							  assertTrue( node.hasLabel( "Label4" ) );
							  assertEquals( countOfNodes.ToString(), node.get("uuid").asString() );
							  countOfNodes++;
						 }
					}
					assertEquals( _lineCountInCSV, countOfNodes );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mixingPeriodicCommitAndLoadCSVShouldWorkWithMapsWithinMaps() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MixingPeriodicCommitAndLoadCSVShouldWorkWithMapsWithinMaps()
		 {
			  using ( Driver driver = GraphDatabase.driver( GraphDb.boltURI(), Configuration() ), Session session = driver.session() )
			  {
					StatementResult result = session.run( "USING PERIODIC COMMIT " + ( _lineCountInCSV - 1 ) + "\n" + "LOAD CSV FROM \"" + _url + "\" as row fieldterminator \" \"\n" + "MERGE (currentnode:Label5 {uuid:row[0]})\n" + "RETURN {outer:{node:currentnode}};" );
					int countOfNodes = 0;
					while ( result.hasNext() )
					{
						 IEnumerator<KeyValuePair<string, object>> iterator = result.next().get(0).asMap().entrySet().GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 iterator = ( ( IDictionary<string, object> ) iterator.next().Value ).SetOfKeyValuePairs().GetEnumerator();
						 while ( iterator.MoveNext() )
						 {
							  KeyValuePair<string, object> entry = iterator.Current;
							  assertEquals( "node", entry.Key );
							  Node node = ( Node ) entry.Value;
							  assertTrue( node.hasLabel( "Label5" ) );
							  assertEquals( countOfNodes.ToString(), node.get("uuid").asString() );
							  countOfNodes++;
						 }
					}

					assertEquals( _lineCountInCSV, countOfNodes );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mixingPeriodicCommitAndLoadCSVShouldWorkWithMapsWithLists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MixingPeriodicCommitAndLoadCSVShouldWorkWithMapsWithLists()
		 {
			  using ( Driver driver = GraphDatabase.driver( GraphDb.boltURI(), Configuration() ), Session session = driver.session() )
			  {
					StatementResult result = session.run( "USING PERIODIC COMMIT " + ( _lineCountInCSV - 1 ) + "\n" + "LOAD CSV FROM \"" + _url + "\" as row fieldterminator \" \"\n" + "MERGE (currentnode:Label6 {uuid:row[0]})\n" + "RETURN {outer:[currentnode]};" );
					int countOfNodes = 0;
					while ( result.hasNext() )
					{
						 IEnumerator<KeyValuePair<string, object>> mapIterator = result.next().get(0).asMap().entrySet().GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( mapIterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 IEnumerator<object> iterator = ( ( IList<object> ) mapIterator.next().Value ).GetEnumerator();
						 while ( iterator.MoveNext() )
						 {
							  Node node = ( Node ) iterator.Current;
							  assertTrue( node.hasLabel( "Label6" ) );
							  assertEquals( countOfNodes.ToString(), node.get("uuid").asString() );
							  countOfNodes++;
						 }
					}
					assertEquals( _lineCountInCSV, countOfNodes );
			  }
		 }

		 private Config Configuration()
		 {
			  return Config.build().withEncryptionLevel(Config.EncryptionLevel.NONE).toConfig();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.net.URL prepareTestImportFile(int lines) throws java.io.IOException
		 private URL PrepareTestImportFile( int lines )
		 {
			  File tempFile = File.createTempFile( "testImport", ".csv" );
			  using ( PrintWriter writer = FileUtils.newFilePrintWriter( tempFile, StandardCharsets.UTF_8 ) )
			  {
					for ( int i = 0; i < lines; i++ )
					{
						 writer.println( i + " " + i + " " + i );
					}
			  }
			  return tempFile.toURI().toURL();
		 }
	}

}