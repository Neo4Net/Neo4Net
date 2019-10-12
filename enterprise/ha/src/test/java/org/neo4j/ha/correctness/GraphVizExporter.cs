using System.Collections.Generic;
using System.IO;

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
namespace Org.Neo4j.ha.correctness
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Transaction = Org.Neo4j.Graphdb.Transaction;

	public class GraphVizExporter
	{
		 private readonly File _target;

		 public GraphVizExporter( File target )
		 {

			  this._target = target;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void export(org.neo4j.graphdb.GraphDatabaseService db) throws java.io.IOException
		 public virtual void Export( GraphDatabaseService db )
		 {
			  FileStream stream = new FileStream( _target, FileMode.Create, FileAccess.Write );
			  PrintWriter @out = new PrintWriter( stream );

			  @out.println( "digraph G {" );
			  @out.println( "    rankdir=LR;" );

			  using ( Transaction tx = Db.beginTx() )
			  {
					ISet<Node> seen = new HashSet<Node>();
					LinkedList<Node> toExplore = new LinkedList<Node>();
					toExplore.AddLast( Db.getNodeById( 0 ) );

					while ( toExplore.Count > 0 )
					{
						 Node current = toExplore.RemoveFirst();

						 @out.println( "    " + current.Id + " [shape=box,label=\"" + current.GetProperty( "description" ) + "\"];" );
						 foreach ( Relationship relationship in current.Relationships )
						 {
							  @out.println( "    " + current.Id + " -> " + relationship.EndNode.Id + " [label=\"" + relationship.GetProperty( "description" ) + "\"];" );

							  if ( !seen.Contains( relationship.EndNode ) )
							  {
									toExplore.AddLast( relationship.EndNode );
									seen.Add( relationship.EndNode );
							  }
						 }
					}

					tx.Success();
			  }

			  @out.println( "}" );

			  @out.flush();
			  stream.Close();
		 }

	}

}