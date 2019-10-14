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
namespace Neo4Net.Collections.primitive.hopscotch
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") class ClosingTablesTest
	internal class ClosingTablesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intCollectionsMustDelegateCloseToTable()
		 internal virtual void IntCollectionsMustDelegateCloseToTable()
		 {
			  // Given
			  Table table = mock( typeof( Table ) );
			  AbstractIntHopScotchCollection coll = new AbstractIntHopScotchCollectionAnonymousInnerClass( this, table );

			  // When
			  coll.close();

			  // Then
			  verify( table ).close();
		 }

		 private class AbstractIntHopScotchCollectionAnonymousInnerClass : AbstractIntHopScotchCollection
		 {
			 private readonly ClosingTablesTest _outerInstance;

			 public AbstractIntHopScotchCollectionAnonymousInnerClass( ClosingTablesTest outerInstance, Table table ) : base( table )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool Equals( object other )
			 {
				  return false;
			 }

			 public override int GetHashCode()
			 {
				  return 0;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void longCollectionsMustDelegateCloseToTable()
		 internal virtual void LongCollectionsMustDelegateCloseToTable()
		 {
			  // Given
			  Table table = mock( typeof( Table ) );
			  AbstractLongHopScotchCollection coll = new AbstractLongHopScotchCollectionAnonymousInnerClass( this, table );

			  // When
			  coll.close();

			  // Then
			  verify( table ).close();
		 }

		 private class AbstractLongHopScotchCollectionAnonymousInnerClass : AbstractLongHopScotchCollection
		 {
			 private readonly ClosingTablesTest _outerInstance;

			 public AbstractLongHopScotchCollectionAnonymousInnerClass( ClosingTablesTest outerInstance, Table table ) : base( table )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool Equals( object other )
			 {
				  return false;
			 }

			 public override int GetHashCode()
			 {
				  return 0;
			 }
		 }
	}

}