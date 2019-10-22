﻿/*
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
namespace Neo4Net.Kernel.Impl.Api
{
	using After = org.junit.After;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;

	public class LabelRecoveryTest
	{
		 public readonly EphemeralFileSystemAbstraction Fs = new EphemeralFileSystemAbstraction();
		 private IGraphDatabaseService _database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  if ( _database != null )
			  {
					_database.shutdown();
			  }
			  Fs.Dispose();
		 }

		 /// <summary>
		 /// Reading a node command might leave a node record which referred to
		 /// labels in one or more dynamic records as marked as heavy even if that
		 /// node already had references to dynamic records, changed in a transaction,
		 /// but had no labels on that node changed within that same transaction.
		 /// Now defensively only marks as heavy if there were one or more dynamic
		 /// records provided when providing the record object with the label field
		 /// value. This would give the opportunity to load the dynamic records the
		 /// next time that record would be ensured heavy.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverNodeWithDynamicLabelRecords()
		 public virtual void ShouldRecoverNodeWithDynamicLabelRecords()
		 {
			  // GIVEN
			  _database = ( new TestGraphDatabaseFactory() ).setFileSystem(Fs).newImpermanentDatabase();
			  Node node;
			  Label[] labels = new Label[] { label( "a" ), label( "b" ), label( "c" ), label( "d" ), label( "e" ), label( "f" ), label( "g" ), label( "h" ), label( "i" ), label( "j" ), label( "k" ) };
			  using ( Transaction tx = _database.beginTx() )
			  {
					node = _database.createNode( labels );
					tx.Success();
			  }

			  // WHEN
			  using ( Transaction tx = _database.beginTx() )
			  {
					node.SetProperty( "prop", "value" );
					tx.Success();
			  }
			  EphemeralFileSystemAbstraction snapshot = Fs.snapshot();
			  _database.shutdown();
			  _database = ( new TestGraphDatabaseFactory() ).setFileSystem(snapshot).newImpermanentDatabase();

			  // THEN
			  using ( Transaction ignored = _database.beginTx() )
			  {
					node = _database.getNodeById( node.Id );
					foreach ( Label label in labels )
					{
						 assertTrue( node.HasLabel( label ) );
					}
			  }
		 }
	}

}