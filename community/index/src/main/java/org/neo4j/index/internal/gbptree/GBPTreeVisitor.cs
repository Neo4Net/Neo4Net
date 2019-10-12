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
namespace Org.Neo4j.Index.@internal.gbptree
{
	using Pair = org.apache.commons.lang3.tuple.Pair;

	internal interface GBPTreeVisitor<KEY, VALUE> : IdProvider_IdProviderVisitor
	{
		 void Meta( Meta meta );

		 void TreeState( Pair<TreeState, TreeState> statePair );

		 void BeginLevel( int level );

		 void BeginNode( long pageId, bool isLeaf, long generation, int keyCount );

		 void Key( KEY key, bool isLeaf );

		 void Value( VALUE value );

		 void Child( long child );

		 void Position( int i );

		 void EndNode( long pageId );

		 void EndLevel( int level );
	}

	 internal class GBPTreeVisitor_Adaptor<KEY, VALUE> : GBPTreeVisitor<KEY, VALUE>
	 {
		  public override void Meta( Meta meta )
		  {
		  }

		  public override void TreeState( Pair<TreeState, TreeState> statePair )
		  {
		  }

		  public override void BeginLevel( int level )
		  {
		  }

		  public override void BeginNode( long pageId, bool isLeaf, long generation, int keyCount )
		  {
		  }

		  public override void Key( KEY key, bool isLeaf )
		  {
		  }

		  public override void Value( VALUE value )
		  {
		  }

		  public override void Child( long child )
		  {
		  }

		  public override void Position( int i )
		  {
		  }

		  public override void EndNode( long pageId )
		  {
		  }

		  public override void EndLevel( int level )
		  {
		  }

		  public override void BeginFreelistPage( long pageId )
		  {
		  }

		  public override void EndFreelistPage( long pageId )
		  {
		  }

		  public override void FreelistEntry( long pageId, long generation, int pos )
		  {
		  }
	 }

}