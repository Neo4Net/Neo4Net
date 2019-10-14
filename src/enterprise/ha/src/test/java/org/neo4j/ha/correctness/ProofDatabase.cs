using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.ha.correctness
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Helpers.Collections;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

	public class ProofDatabase
	{
		 private readonly GraphDatabaseService _gds;
		 private readonly IDictionary<ClusterState, Node> _stateNodes = new Dictionary<ClusterState, Node>();

		 public ProofDatabase( string location )
		 {
			  File dbDir = new File( location );
			  CleanDbDir( dbDir );
			  this._gds = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(dbDir);
		 }

		 public virtual Node NewState( ClusterState state )
		 {
			  using ( Transaction tx = _gds.beginTx() )
			  {
					Node node = _gds.createNode( label( "State" ) );
					node.SetProperty( "description", state.ToString() );
					tx.Success();

					_stateNodes[state] = node;
					return node;
			  }
		 }

		 public virtual void NewStateTransition( ClusterState originalState, Pair<ClusterAction, ClusterState> transition )
		 {
			  using ( Transaction tx = _gds.beginTx() )
			  {
					Node stateNode = _stateNodes[originalState];

					Node subStateNode = NewState( transition.Other() );

					Relationship msg = stateNode.CreateRelationshipTo( subStateNode, RelationshipType.withName( "MESSAGE" ) );
					msg.SetProperty( "description", transition.First().ToString() );
					tx.Success();
			  }
		 }

		 private void CleanDbDir( File dbDir )
		 {
			  if ( dbDir.exists() )
			  {
					try
					{
						 FileUtils.deleteRecursively( dbDir );
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }
			  else
			  {
					dbDir.ParentFile.mkdirs();
			  }
		 }

		 public virtual void Shutdown()
		 {
			  _gds.shutdown();
		 }

		 public virtual bool IsKnownState( ClusterState state )
		 {
			  return _stateNodes.ContainsKey( state );
		 }

		 public virtual long NumberOfKnownStates()
		 {
			  return _stateNodes.Count;
		 }

		 public virtual long Id( ClusterState nextState )
		 {
			  return _stateNodes[nextState].Id;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void export(GraphVizExporter graphVizExporter) throws java.io.IOException
		 public virtual void Export( GraphVizExporter graphVizExporter )
		 {
			  graphVizExporter.Export( _gds );
		 }
	}

}