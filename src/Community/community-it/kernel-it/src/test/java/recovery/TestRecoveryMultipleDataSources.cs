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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using MyRelTypes = Neo4Net.Kernel.impl.MyRelTypes;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.exit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.proc.ProcessUtil.getClassPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.proc.ProcessUtil.getJavaExecutable;

	public class TestRecoveryMultipleDataSources
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 /// <summary>
		 /// Tests an issue where loading all relationship types and property indexes after
		 /// the neostore data source had been started internally. The db would be in a
		 /// state where it would need recovery for the neostore data source, as well as some
		 /// other data source. This would fail since eventually TxManager#getTransaction()
		 /// would be called, which would fail since it hadn't as of yet recovered fully.
		 /// Whereas that failure would happen in a listener and merely be logged, one effect
		 /// of it would be that there would seem to be no relationship types in the database.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoverNeoAndIndexHavingAllRelationshipTypesAfterRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoverNeoAndIndexHavingAllRelationshipTypesAfterRecovery()
		 {
			  // Given (create transactions and kill process, leaving it needing for recovery)
			  File storeDir = TestDirectory.storeDir();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  assertEquals( 0, Runtime.exec( new string[]{ JavaExecutable.ToString(), "-Djava.awt.headless=true", "-cp", ClassPath, this.GetType().FullName, storeDir.AbsolutePath } ).waitFor() );

			  // When
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);

			  // Then
			  try
			  {
					  using ( Transaction ignored = Db.beginTx(), ResourceIterator<RelationshipType> typeResourceIterator = Db.AllRelationshipTypes.GetEnumerator() )
					  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						assertEquals( MyRelTypes.TEST.name(), typeResourceIterator.next().name() );
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
		 public static void Main( string[] args )
		 {
			  if ( args.Length != 1 )
			  {
					exit( 1 );
			  }

			  File storeDir = new File( args[0] );
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode().createRelationshipTo(Db.createNode(), MyRelTypes.TEST);
					tx.Success();
			  }

			  ( ( GraphDatabaseAPI ) db ).DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint(new SimpleTriggerInfo("test")
			 );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.index().forNodes("index").add(Db.createNode(), storeDir.AbsolutePath, Db.createNode());
					tx.Success();
			  }

			  exit( 0 );
		 }
	}

}