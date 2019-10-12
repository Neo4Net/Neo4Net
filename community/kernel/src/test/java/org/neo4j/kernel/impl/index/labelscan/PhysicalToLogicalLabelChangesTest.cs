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
namespace Org.Neo4j.Kernel.impl.index.labelscan
{
	using Test = org.junit.Test;

	using NodeLabelUpdate = Org.Neo4j.Kernel.api.labelscan.NodeLabelUpdate;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;

	public class PhysicalToLogicalLabelChangesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeSimpleAddition()
		 public virtual void ShouldSeeSimpleAddition()
		 {
			  ConvertAndAssert( Ids(), Ids(2), Ids(), Ids(2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeSimpleRemoval()
		 public virtual void ShouldSeeSimpleRemoval()
		 {
			  ConvertAndAssert( Ids( 2 ), Ids(), Ids(2), Ids() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeSomeAdded()
		 public virtual void ShouldSeeSomeAdded()
		 {
			  ConvertAndAssert( Ids( 1, 3, 5 ), Ids( 1, 2, 3, 4, 5, 6 ), Ids(), Ids(2, 4, 6) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeSomeRemoved()
		 public virtual void ShouldSeeSomeRemoved()
		 {
			  ConvertAndAssert( Ids( 1, 2, 3, 4, 5, 6 ), Ids( 1, 3, 5 ), Ids( 2, 4, 6 ), Ids() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeSomeAddedAndSomeRemoved()
		 public virtual void ShouldSeeSomeAddedAndSomeRemoved()
		 {
			  ConvertAndAssert( Ids( 1, 3, 4, 6 ), Ids( 0, 2, 3, 5, 6 ), Ids( 1, 4 ), Ids( 0, 2, 5 ) );
		 }

		 private void ConvertAndAssert( long[] before, long[] after, long[] expectedRemoved, long[] expectedAdded )
		 {
			  NodeLabelUpdate update = NodeLabelUpdate.labelChanges( 0, before, after );
			  PhysicalToLogicalLabelChanges.ConvertToAdditionsAndRemovals( update );
			  assertArrayEquals( Terminate( update.LabelsBefore ), expectedRemoved );
			  assertArrayEquals( Terminate( update.LabelsAfter ), expectedAdded );
		 }

		 private long[] Terminate( long[] labels )
		 {
			  int length = ActualLength( labels );
			  return length == labels.Length ? labels : Arrays.copyOf( labels, length );
		 }

		 private int ActualLength( long[] labels )
		 {
			  for ( int i = 0; i < labels.Length; i++ )
			  {
					if ( labels[i] == -1 )
					{
						 return i;
					}
			  }
			  return labels.Length;
		 }

		 private long[] Ids( params long[] ids )
		 {
			  return ids;
		 }
	}

}