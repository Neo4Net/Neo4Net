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
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNode.Type.INTERNAL;

	public class InternalTreeLogicDynamicSizeTest : InternalTreeLogicTestBase<RawBytes, RawBytes>
	{
		 protected internal override ValueMerger<RawBytes, RawBytes> Adder
		 {
			 get
			 {
				  return ( existingKey, newKey, @base, add ) =>
				  {
					long baseSeed = LayoutConflict.keySeed( @base );
					long addSeed = LayoutConflict.keySeed( add );
					return LayoutConflict.value( baseSeed + addSeed );
				  };
			 }
		 }

		 protected internal override TreeNode<RawBytes, RawBytes> GetTreeNode( int pageSize, Layout<RawBytes, RawBytes> layout )
		 {
			  return new TreeNodeDynamicSize<RawBytes, RawBytes>( pageSize, layout );
		 }

		 protected internal override TestLayout<RawBytes, RawBytes> Layout
		 {
			 get
			 {
				  return new SimpleByteArrayLayout();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToInsertTooLargeKeys() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToInsertTooLargeKeys()
		 {
			  RawBytes key = LayoutConflict.newKey();
			  RawBytes value = LayoutConflict.newValue();
			  key.Bytes = new sbyte[Node.keyValueSizeCap() + 1];
			  value.Bytes = new sbyte[0];

			  ShouldFailToInsertTooLargeKeyAndValue( key, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToInsertTooLargeKeyAndValueLargeKey() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToInsertTooLargeKeyAndValueLargeKey()
		 {
			  RawBytes key = LayoutConflict.newKey();
			  RawBytes value = LayoutConflict.newValue();
			  key.Bytes = new sbyte[Node.keyValueSizeCap()];
			  value.Bytes = new sbyte[1];

			  ShouldFailToInsertTooLargeKeyAndValue( key, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToInsertTooLargeKeyAndValueLargeValue() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToInsertTooLargeKeyAndValueLargeValue()
		 {
			  RawBytes key = LayoutConflict.newKey();
			  RawBytes value = LayoutConflict.newValue();
			  key.Bytes = new sbyte[1];
			  value.Bytes = new sbyte[Node.keyValueSizeCap()];

			  ShouldFailToInsertTooLargeKeyAndValue( key, value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldFailToInsertTooLargeKeyAndValue(RawBytes key, RawBytes value) throws java.io.IOException
		 private void ShouldFailToInsertTooLargeKeyAndValue( RawBytes key, RawBytes value )
		 {
			  Initialize();
			  try
			  {
					Insert( key, value );
			  }
			  catch ( System.ArgumentException e )
			  {
					assertThat( e.Message, CoreMatchers.containsString( "Index key-value size it to large. Please see index documentation for limitations." ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeOnlyMinimalKeyDividerInInternal() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreOnlyMinimalKeyDividerInInternal()
		 {
			  // given
			  Initialize();
			  long key = 0;
			  while ( NumberOfRootSplits == 0 )
			  {
					Insert( key( key ), Value( key ) );
					key++;
			  }

			  // when
			  RawBytes rawBytes = KeyAt( Root.id(), 0, INTERNAL );

			  // then
			  assertEquals( "expected no tail on internal key but was " + rawBytes.ToString(), Long.BYTES, rawBytes.Bytes.Length );
		 }
	}

}