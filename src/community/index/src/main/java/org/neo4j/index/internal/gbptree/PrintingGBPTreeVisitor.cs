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
namespace Neo4Net.Index.Internal.gbptree
{
	using Pair = org.apache.commons.lang3.tuple.Pair;


	public class PrintingGBPTreeVisitor<KEY, VALUE> : GBPTreeVisitor_Adaptor<KEY, VALUE>
	{
		 private readonly PrintStream @out;
		 private readonly bool _printValues;
		 private readonly bool _printPosition;
		 private readonly bool _printState;
		 private readonly bool _printHeader;
		 private readonly bool _printFreelist;

		 /// <summary>
		 /// Prints a <seealso cref="GBPTree"/> in human readable form, very useful for debugging.
		 /// Will print sub-tree from that point. Leaves cursor at same page as when called. No guarantees on offset.
		 /// </summary>
		 /// <param name="out"> target to print tree at. </param>
		 /// <param name="printPosition"> whether or not to include positional (slot number) information. </param>
		 /// <param name="printState"> whether or not to also print state pages </param>
		 /// <param name="printHeader"> whether or not to also print header (type, generation, keyCount) of every node </param>
		 /// <param name="printFreelist"> whether or not to also print freelist </param>
		 public PrintingGBPTreeVisitor( PrintStream @out, bool printValues, bool printPosition, bool printState, bool printHeader, bool printFreelist )
		 {

			  this.@out = @out;
			  this._printValues = printValues;
			  this._printPosition = printPosition;
			  this._printState = printState;
			  this._printHeader = printHeader;
			  this._printFreelist = printFreelist;
		 }

		 public override void TreeState( Pair<TreeState, TreeState> statePair )
		 {
			  if ( _printState )
			  {
					@out.println( "StateA: " + statePair.Left );
					@out.println( "StateB: " + statePair.Right );
			  }
		 }

		 public override void BeginLevel( int level )
		 {
			  @out.println( "Level " + level );
		 }

		 public override void BeginNode( long pageId, bool isLeaf, long generation, int keyCount )
		 {
			  if ( _printHeader )
			  {
					string treeNodeType = isLeaf ? "leaf" : "internal";
					@out.print( format( "{%d,%s,generation=%d,keyCount=%d} ", pageId, treeNodeType, generation, keyCount ) );
			  }
			  else
			  {
					@out.print( "{" + pageId + "} " );
			  }
		 }

		 public override void Position( int i )
		 {
			  if ( _printPosition )
			  {
					@out.print( "#" + i + " " );
			  }
		 }

		 public override void Key( KEY key, bool isLeaf )
		 {
			  @out.print( isLeaf ? key : "[" + key + "]" );
		 }

		 public override void Value( VALUE value )
		 {
			  if ( _printValues )
			  {
					@out.print( "=" + value );
			  }
			  @out.print( " " );
		 }

		 public override void Child( long child )
		 {
			  @out.print( " /" + child + "\\ " );
		 }

		 public override void EndNode( long pageId )
		 {
			  @out.println();
		 }

		 public override void BeginFreelistPage( long pageId )
		 {
			  if ( _printFreelist )
			  {
					@out.print( "Freelist{" + pageId + "} " );
			  }
		 }

		 public override void EndFreelistPage( long pageId )
		 {
			  if ( _printFreelist )
			  {
					@out.println();
			  }
		 }

		 public override void FreelistEntry( long pageId, long generation, int pos )
		 {
			  if ( _printFreelist )
			  {
					@out.print( "[" + generation + "," + pageId + "] " );
			  }
		 }
	}

}