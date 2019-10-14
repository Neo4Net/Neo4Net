using System;

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
namespace Neo4Net.Kernel.Impl.Api
{
	using Values = Neo4Net.Values.Storable.Values;

	/// <summary>
	/// This class is only needed because of a missed dependency in Cypher 2.3 and 3.1.
	/// It can be removed in 4.0.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Deprecated public class PropertyValueComparison
	[Obsolete]
	public class PropertyValueComparison
	{
		 private PropertyValueComparison()
		 {
			  throw new AssertionError( "no instance" );
		 }

		 public static readonly PropertyValueComparator<object> COMPARE_VALUES = new PropertyValueComparatorAnonymousInnerClass();

		 private class PropertyValueComparatorAnonymousInnerClass : PropertyValueComparator<object>
		 {
			 public override int compare( object o1, object o2 )
			 {
				  return Values.COMPARATOR.Compare( Values.of( o1 ), Values.of( o2 ) );
			 }
		 }

		 public static readonly PropertyValueComparator<Number> COMPARE_NUMBERS = new PropertyValueComparatorAnonymousInnerClass2();

		 private class PropertyValueComparatorAnonymousInnerClass2 : PropertyValueComparator<Number>
		 {
			 public override int compare( Number n1, Number n2 )
			 {
				  return Values.COMPARATOR.Compare( Values.numberValue( n1 ), Values.numberValue( n2 ) );
			 }
		 }

		 public static readonly PropertyValueComparator<object> COMPARE_STRINGS = new PropertyValueComparatorAnonymousInnerClass3();

		 private class PropertyValueComparatorAnonymousInnerClass3 : PropertyValueComparator<object>
		 {
			 public override int compare( object o1, object o2 )
			 {
				  return Values.COMPARATOR.Compare( Values.of( o1 ), Values.of( o2 ) );
			 }
		 }
	}

}