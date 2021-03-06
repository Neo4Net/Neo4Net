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
namespace Org.Neo4j.Graphdb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using LabelScanStoreTest = Org.Neo4j.Kernel.api.impl.labelscan.LabelScanStoreTest;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	/// <summary>
	/// Tests functionality around missing or corrupted lucene label scan store index, and that
	/// the database should repair (i.e. rebuild) that automatically and just work.
	/// </summary>
	public class NativeLabelScanStoreChaosIT
	{
		private bool InstanceFieldsInitialized = false;

		public NativeLabelScanStoreChaosIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _randomRule ).around( _dbRule );
		}

		 private readonly DatabaseRule _dbRule = new EmbeddedDatabaseRule();
		 private readonly RandomRule _randomRule = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(randomRule).around(dbRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRebuildDeletedLabelScanStoreOnStartup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRebuildDeletedLabelScanStoreOnStartup()
		 {
			  // GIVEN
			  Node node1 = CreateLabeledNode( Labels.First );
			  Node node2 = CreateLabeledNode( Labels.First );
			  Node node3 = CreateLabeledNode( Labels.First );
			  DeleteNode( node2 ); // just to create a hole in the store

			  // WHEN
			  _dbRule.restartDatabase( DeleteTheLabelScanStoreIndex() );

			  // THEN
			  assertEquals( asSet( node1, node3 ), GetAllNodesWithLabel( Labels.First ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rebuildCorruptedLabelScanStoreToStartup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RebuildCorruptedLabelScanStoreToStartup()
		 {
			  Node node = CreateLabeledNode( Labels.First );

			  _dbRule.restartDatabase( CorruptTheLabelScanStoreIndex() );

			  assertEquals( asSet( node ), GetAllNodesWithLabel( Labels.First ) );
		 }

		 private static File StoreFile( DatabaseLayout databaseLayout )
		 {
			  return databaseLayout.LabelScanStore();
		 }

		 private DatabaseRule.RestartAction CorruptTheLabelScanStoreIndex()
		 {
			  return ( fs, directory ) => scrambleFile( StoreFile( directory ) );
		 }

		 private DatabaseRule.RestartAction DeleteTheLabelScanStoreIndex()
		 {
			  return ( fs, directory ) => fs.deleteFile( StoreFile( directory ) );
		 }

		 private Node CreateLabeledNode( params Label[] labels )
		 {
			  using ( Transaction tx = _dbRule.GraphDatabaseAPI.beginTx() )
			  {
					Node node = _dbRule.GraphDatabaseAPI.createNode( labels );
					tx.Success();
					return node;
			  }
		 }

		 private ISet<Node> GetAllNodesWithLabel( Label label )
		 {
			  using ( Transaction ignored = _dbRule.GraphDatabaseAPI.beginTx() )
			  {
					return asSet( _dbRule.GraphDatabaseAPI.findNodes( label ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void scrambleFile(java.io.File file) throws java.io.IOException
		 private void ScrambleFile( File file )
		 {
			  LabelScanStoreTest.scrambleFile( _randomRule.random(), file );
		 }

		 private void DeleteNode( Node node )
		 {
			  using ( Transaction tx = _dbRule.GraphDatabaseAPI.beginTx() )
			  {
					node.Delete();
					tx.Success();
			  }
		 }

		 private enum Labels
		 {
			  First,
			  Second,
			  Third
		 }
	}

}