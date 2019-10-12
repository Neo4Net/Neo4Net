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
namespace Org.Neo4j.Index.@explicit
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Org.Neo4j.Graphdb.index;
	using Org.Neo4j.Graphdb.index;
	using QueryContext = Org.Neo4j.Index.lucene.QueryContext;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

	public class ExplicitIndexRegressionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule graphdb = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Graphdb = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessAndUpdateIndexInSameTransaction()
		 public virtual void ShouldAccessAndUpdateIndexInSameTransaction()
		 {
			  using ( Transaction tx = Graphdb.beginTx() )
			  {
					Index<Node> version = Graphdb.index().forNodes("version");
					for ( int v = 0; v < 10; v++ )
					{
						 CreateNode( version, v );
					}
					tx.Success();
			  }
		 }

		 private void CreateNode( Index<Node> index, long version )
		 {
			  Highest( "version", index.query( new QueryContext( "version:*" ) ) );
			  {
					Node node = Graphdb.createNode();
					node.SetProperty( "version", version );
					index.Add( node, "version", version );
			  }
			  {
					Node node = index.get( "version", version ).Single;
					Node current = Highest( "version", index.get( "current", "current" ) );
					if ( current != null )
					{
						 index.Remove( current, "current" );
					}
					index.Add( node, "current", "current" );
			  }
		 }

		 private Node Highest( string key, IndexHits<Node> query )
		 {
			  using ( IndexHits<Node> hits = query )
			  {
					long highestValue = long.MinValue;
					Node highestNode = null;
					while ( hits.MoveNext() )
					{
						 Node node = hits.Current;
						 long value = ( ( Number ) node.GetProperty( key ) ).longValue();
						 if ( value > highestValue )
						 {
							  highestValue = value;
							  highestNode = node;
						 }
					}
					return highestNode;
			  }
		 }
	}

}