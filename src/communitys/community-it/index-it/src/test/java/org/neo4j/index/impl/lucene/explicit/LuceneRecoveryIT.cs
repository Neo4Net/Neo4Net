using System;
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
	using CorruptIndexException = Org.Apache.Lucene.Index.CorruptIndexException;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Graphdb.index;
	using Neo4Net.Graphdb.index;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using ProcessUtil = Neo4Net.Test.proc.ProcessUtil;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using VerboseTimeout = Neo4Net.Test.rule.VerboseTimeout;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.proc.ProcessUtil.getJavaExecutable;

	public class LuceneRecoveryIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.VerboseTimeout timeout = org.neo4j.test.rule.VerboseTimeout.builder().withTimeout(30, MINUTES).build();
		 public readonly VerboseTimeout Timeout = VerboseTimeout.builder().withTimeout(30, MINUTES).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testHardCoreRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestHardCoreRecovery()
		 {
			  string path = TestDirectory.storeDir().Path;

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  Process process = Runtime.Runtime.exec( new string[]{ JavaExecutable.ToString(), "-cp", ProcessUtil.ClassPath, typeof(Inserter).FullName, path } );

			  // Let it run for a while and then kill it, and wait for it to die
			  AwaitFile( new File( path, "started" ) );
			  Thread.Sleep( 5000 );
			  process.destroy();
			  process.waitFor();

			  GraphDatabaseService db = null;
			  try
			  {
					db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(TestDirectory.storeDir());
					try
					{
							using ( Transaction transaction = Db.beginTx() )
							{
							 assertTrue( Db.index().existsForNodes("myIndex") );
							 Index<Node> index = Db.index().forNodes("myIndex");
							 foreach ( Node node in Db.AllNodes )
							 {
								  foreach ( string key in node.PropertyKeys )
								  {
										string value = ( string ) node.GetProperty( key );
										bool found = false;
										using ( IndexHits<Node> indexHits = index.get( key, value ) )
										{
											 foreach ( Node indexedNode in indexHits )
											 {
												  if ( indexedNode.Equals( node ) )
												  {
														found = true;
														break;
												  }
											 }
										}
										if ( !found )
										{
											 throw new System.InvalidOperationException( node + " has property '" + key + "'='" + value + "', but not in index" );
										}
								  }
							 }
							}
					}
					catch ( Exception e )
					{
						 if ( Exceptions.contains( e, typeof( CorruptIndexException ) ) || ExceptionContainsStackTraceElementFromPackage( e, "org.apache.lucene" ) )
						 {
							  // On some machines and during some circumstances a lucene index may become
							  // corrupted during a crash. This is out of our control and since this test
							  // is about an explicit (a.k.a. legacy/manual) index the db cannot just re-populate the
							  // index automatically. We have to consider this an OK scenario and we cannot
							  // verify the index any further if it happens.
							  Console.Error.WriteLine( "Lucene exception happened during recovery after a real crash. " + "It may be that the index is corrupt somehow and this is out of control and not " + "something this test can really improve on right now. Printing the exception for reference" );
							  Console.WriteLine( e.ToString() );
							  Console.Write( e.StackTrace );
							  return;
						 }

						 // This was another unknown exception, throw it so that the test fails with it
						 throw e;
					}

					// Added due to a recovery issue where the lucene data source write wasn't released properly after recovery.
					NodeCreator nodeCreator = new NodeCreator( db );
					Thread t = new Thread( nodeCreator );
					t.Start();
					t.Join();
			  }
			  finally
			  {
					if ( db != null )
					{
						 Db.shutdown();
					}
			  }
		 }

		 private static bool ExceptionContainsStackTraceElementFromPackage( Exception e, string packageName )
		 {
			  foreach ( StackTraceElement element in e.StackTrace )
			  {
					if ( element.ClassName.StartsWith( packageName ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void awaitFile(java.io.File file) throws InterruptedException
		 private static void AwaitFile( File file )
		 {
			  long end = DateTimeHelper.CurrentUnixTimeMillis() + TimeUnit.SECONDS.toMillis(300);
			  while ( !file.exists() && DateTimeHelper.CurrentUnixTimeMillis() < end )
			  {
					Thread.Sleep( 100 );
			  }
			  if ( !file.exists() )
			  {
					fail( "The inserter doesn't seem to have run properly" );
			  }
		 }

		 private class NodeCreator : ThreadStart
		 {
			  internal readonly GraphDatabaseService Db;

			  internal NodeCreator( GraphDatabaseService db )
			  {
					this.Db = db;
			  }

			  public override void Run()
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Index<Node> index = Db.index().forNodes("myIndex");
						 index.Add( Db.createNode(), "one", "two" );
						 tx.Success();
					}
			  }
		 }

	}

}