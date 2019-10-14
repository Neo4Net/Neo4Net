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
namespace Neo4Net.Index.@internal.gbptree
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;

	internal class SimpleByteArrayLayoutTest
	{
		private bool InstanceFieldsInitialized = false;

		public SimpleByteArrayLayoutTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_left = _layout.newKey();
			_right = _layout.newKey();
			_minimalSplitter = _layout.newKey();
		}

		 private readonly SimpleByteArrayLayout _layout = new SimpleByteArrayLayout( false );
		 private RawBytes _left;
		 private RawBytes _right;
		 private RawBytes _minimalSplitter;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void minimalSplitterLengthZero()
		 internal virtual void MinimalSplitterLengthZero()
		 {
			  _left.bytes = new sbyte[0];
			  _right.bytes = new sbyte[1];

			  _layout.minimalSplitter( _left, _right, _minimalSplitter );

			  assertArrayEquals( _right.bytes, _minimalSplitter.bytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void minimalSplitterLengthZeroWithLongRight()
		 internal virtual void MinimalSplitterLengthZeroWithLongRight()
		 {

			  _left.bytes = new sbyte[0];
			  _right.bytes = new sbyte[]{ 1, 1 };
			  _layout.minimalSplitter( _left, _right, _minimalSplitter );

			  assertArrayEquals( new sbyte[]{ 1 }, _minimalSplitter.bytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void minimalSplitterLengthOne()
		 internal virtual void MinimalSplitterLengthOne()
		 {
			  _left.bytes = new sbyte[]{ 0 };
			  _right.bytes = new sbyte[]{ 1 };

			  _layout.minimalSplitter( _left, _right, _minimalSplitter );

			  assertArrayEquals( _right.bytes, _minimalSplitter.bytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void minimalSplitterDifferOnLength()
		 internal virtual void MinimalSplitterDifferOnLength()
		 {
			  _left.bytes = new sbyte[1];
			  _right.bytes = new sbyte[2];

			  _layout.minimalSplitter( _left, _right, _minimalSplitter );

			  assertArrayEquals( _right.bytes, _minimalSplitter.bytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void minimalSplitterDifferMoreOnLength()
		 internal virtual void MinimalSplitterDifferMoreOnLength()
		 {
			  _left.bytes = new sbyte[1];
			  _right.bytes = new sbyte[3];

			  _layout.minimalSplitter( _left, _right, _minimalSplitter );

			  assertArrayEquals( new sbyte[2], _minimalSplitter.bytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void minimalSplitterDifferOnLast()
		 internal virtual void MinimalSplitterDifferOnLast()
		 {
			  _left.bytes = new sbyte[]{ 0, 0 };
			  _right.bytes = new sbyte[]{ 0, 1 };

			  _layout.minimalSplitter( _left, _right, _minimalSplitter );

			  assertArrayEquals( _right.bytes, _minimalSplitter.bytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void minimalSplitterDifferOnFirst()
		 internal virtual void MinimalSplitterDifferOnFirst()
		 {
			  _left.bytes = new sbyte[]{ 0, 0 };
			  _right.bytes = new sbyte[]{ 1, 0 };

			  _layout.minimalSplitter( _left, _right, _minimalSplitter );

			  assertArrayEquals( new sbyte[]{ 1 }, _minimalSplitter.bytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void minimalSplitterDifferOnFirstLeftShorter()
		 internal virtual void MinimalSplitterDifferOnFirstLeftShorter()
		 {
			  _left.bytes = new sbyte[]{ 0 };
			  _right.bytes = new sbyte[]{ 1, 0 };

			  _layout.minimalSplitter( _left, _right, _minimalSplitter );

			  assertArrayEquals( new sbyte[]{ 1 }, _minimalSplitter.bytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void minimalSplitterDifferOnFirstRightShorter()
		 internal virtual void MinimalSplitterDifferOnFirstRightShorter()
		 {
			  _left.bytes = new sbyte[]{ 0, 0 };
			  _right.bytes = new sbyte[]{ 1 };

			  _layout.minimalSplitter( _left, _right, _minimalSplitter );

			  assertArrayEquals( _right.bytes, _minimalSplitter.bytes );
		 }
	}

}