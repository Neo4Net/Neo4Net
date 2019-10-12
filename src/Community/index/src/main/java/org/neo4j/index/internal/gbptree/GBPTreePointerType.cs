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
namespace Neo4Net.Index.@internal.gbptree
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNode.BYTE_POS_LEFTSIBLING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNode.BYTE_POS_RIGHTSIBLING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNode.BYTE_POS_SUCCESSOR;

	public interface GBPTreePointerType
	{
		 int Offset( TreeNode node );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static GBPTreePointerType leftSibling()
	//	 {
	//		  return SimplePointer.LEFT_SIBLING;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static GBPTreePointerType rightSibling()
	//	 {
	//		  return SimplePointer.RIGHT_SIBLING;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static GBPTreePointerType successor()
	//	 {
	//		  return SimplePointer.SUCCESSOR;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static GBPTreePointerType noPointer()
	//	 {
	//		  return SimplePointer.NO_POINTER;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static GBPTreePointerType child(int pos)
	//	 {
	//		  return new ChildPointer(pos);
	//	 }
	}

	 public enum GBPTreePointerType_SimplePointer
	 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: NO_POINTER { @Override public int offset(TreeNode node) { return 0; } @Override public String toString() { return "no pointer"; } },
		  NO_POINTER
		  {
			  public int offset( TreeNode node ) { return 0; } public string ToString() { return "no pointer"; }
		  },
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: LEFT_SIBLING { @Override public int offset(TreeNode node) { return BYTE_POS_LEFTSIBLING; } @Override public String toString() { return "left sibling"; } },
		  LEFT_SIBLING
		  {
			  public int offset( TreeNode node ) { return BYTE_POS_LEFTSIBLING; } public string ToString() { return "left sibling"; }
		  },
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: RIGHT_SIBLING { @Override public int offset(TreeNode node) { return BYTE_POS_RIGHTSIBLING; } @Override public String toString() { return "right sibling"; } },
		  RIGHT_SIBLING
		  {
			  public int offset( TreeNode node ) { return BYTE_POS_RIGHTSIBLING; } public string ToString() { return "right sibling"; }
		  },
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: SUCCESSOR { @Override public int offset(TreeNode node) { return BYTE_POS_SUCCESSOR; } @Override public String toString() { return "successor"; } }
		  SUCCESSOR
		  {
			  public int offset( TreeNode node ) { return BYTE_POS_SUCCESSOR; } public string ToString() { return "successor"; }
		  }
	 }

	 public class GBPTreePointerType_ChildPointer implements GBPTreePointerType
	 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int pos;
		  int pos;

		  GBPTreePointerType_ChildPointer( int pos )
		  {
				this.pos = pos;
		  }

		  public int offset( TreeNode node )
		  {
				return node.childOffset( pos );
		  }

		  public string ToString()
		  {
				return "child(" + pos + ")";
		  }

		  public bool Equals( object o )
		  {
				if ( this == o )
				{
					 return true;
				}
				if ( o == null || this.GetType() != o.GetType() )
				{
					 return false;
				}
				GBPTreePointerType_ChildPointer that = ( GBPTreePointerType_ChildPointer ) o;
				return pos == that.Pos;
		  }

		  public int GetHashCode()
		  {
				return Objects.hash( pos );
		  }
	 }

}