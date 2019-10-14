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
namespace Neo4Net.Kernel.impl.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class TestIdReuse
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureIdsGetsReusedForPropertyStore()
		 public virtual void MakeSureIdsGetsReusedForPropertyStore()
		 {
			  MakeSureIdsGetsReused( "neostore.propertystore.db", 10, 200 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureIdsGetsReusedForArrayStore()
		 public virtual void MakeSureIdsGetsReusedForArrayStore()
		 {
			  long[] array = new long[500];
			  for ( int i = 0; i < array.Length; i++ )
			  {
					array[i] = 0xFFFFFFFFFFFFL + i;
			  }
			  MakeSureIdsGetsReused( "neostore.propertystore.db.arrays", array, 20 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureIdsGetsReusedForStringStore()
		 public virtual void MakeSureIdsGetsReusedForStringStore()
		 {
			  string @string = "something";
			  for ( int i = 0; i < 100; i++ )
			  {
					@string += "something else " + i;
			  }
			  MakeSureIdsGetsReused( "neostore.propertystore.db.strings", @string, 20 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();

		 private void MakeSureIdsGetsReused( string fileName, object value, int iterations )
		 {
			  File storeDir = new File( "target/var/idreuse" );
			  File file = new File( storeDir, fileName );
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(Fs.get()).newImpermanentDatabaseBuilder(storeDir).newGraphDatabase();
			  for ( int i = 0; i < 5; i++ )
			  {
					SetAndRemoveSomeProperties( db, value );
			  }
			  Db.shutdown();
			  long sizeBefore = file.length();
			  db = ( new TestGraphDatabaseFactory() ).setFileSystem(Fs.get()).newImpermanentDatabase(storeDir);
			  for ( int i = 0; i < iterations; i++ )
			  {
					SetAndRemoveSomeProperties( db, value );
			  }
			  Db.shutdown();
			  assertEquals( sizeBefore, file.length() );
		 }

		 private void SetAndRemoveSomeProperties( GraphDatabaseService graphDatabaseService, object value )
		 {
			  Node commonNode;
			  using ( Transaction transaction = graphDatabaseService.BeginTx() )
			  {
					commonNode = graphDatabaseService.CreateNode();
					for ( int i = 0; i < 10; i++ )
					{
						 commonNode.SetProperty( "key" + i, value );
					}
					transaction.Success();
			  }

			  using ( Transaction transaction = graphDatabaseService.BeginTx() )
			  {
					for ( int i = 0; i < 10; i++ )
					{
						 commonNode.RemoveProperty( "key" + i );
					}
					transaction.Success();
			  }
		 }
	}

}